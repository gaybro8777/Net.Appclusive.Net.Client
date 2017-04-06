/**
 * Copyright 2017 d-fens GmbH
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

extern alias Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Security;
using Api::Net.Appclusive.Api;
using Api::Net.Appclusive.Api.Constants;
using Api::Net.Appclusive.Public.Domain.Identity;
using biz.dfch.CS.PowerShell.Commons;

namespace Net.Appclusive.PS.Client
{
    /// <summary>
    /// This class defines the EnterServer Cmdlet that performs a login to a Appclusive instance
    /// </summary>
    [Cmdlet(
         VerbsCommon.Enter, "Server"
         ,
         ConfirmImpact = ConfirmImpact.Medium
         ,
         DefaultParameterSetName = ParameterSets.PLAIN
         ,
         SupportsShouldProcess = true
         ,
         HelpUri = "http://docs.appclusive.net/en/latest/Clients/PowerShell/#login-enter-server"
    )]
    [OutputType(typeof(Dictionary<string, DataServiceContextBase>))]
    public class EnterServer : PsCmdletBase
    {
        /// <summary>
        /// Defines all valid parameter sets for this cmdlet
        /// </summary>
        public static class ParameterSets
        {
            /// <summary>
            /// ParameterSetName used when specifying a credential object
            /// </summary>
            public const string CREDENTIAL = "cred";

            /// <summary>
            /// ParameterSetName used when specifying plain username and password
            /// </summary>
            public const string PLAIN = "plain";

            /// <summary>
            /// ParameterSetName used when using settings from ModuleContext
            /// </summary>
            public const string MODULE_CONTEXT = "config";
        }

        /// <summary>
        /// Specifies the base URI of the Appclusive API
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSets.PLAIN)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSets.CREDENTIAL)]
        [Alias(nameof(Uri))]
        public Uri ApiBaseUri { get; set; }

        /// <summary>
        /// Specifies the username to log in with
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = ParameterSets.PLAIN)]
        [ValidateNotNullOrEmpty]
        public string Username { get; set; }

        /// <summary>
        /// Specifies the password to log in with
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = ParameterSets.PLAIN)]
        [ValidateNotNullOrEmpty]
        public string Password { get; set; }

        /// <summary>
        /// Specifies the PSCredential object containing username and password to log in with
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = ParameterSets.CREDENTIAL)]
        [Alias("cred")]
        public PSCredential Credential { get; set; }

        /// <summary>
        /// Specifies the tenant id for the user to log in with. If not specified no tenant id header will not be sent
        /// </summary>
        [Parameter(Mandatory = false)]
        [Alias("tid")]
        public Guid TenantId { get; set; }

        /// <summary>
        /// Use settings from ModuleContext variable to log in
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSets.MODULE_CONTEXT)]
        [Alias("ctx", "context")]
        [PSDefaultValue(Value = false)]
        public SwitchParameter UseModuleContext { get; set; }

        /// <summary>
        /// ProcessRecord
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var shouldProcessMessage = string.Format(Messages.Cmdlet_ProcessRecord__ShouldProcess, ParameterSetName);
            if (!ShouldProcess(shouldProcessMessage))
            {
                return;
            }

            if (ParameterSets.PLAIN.Equals(ParameterSetName))
            {
                // Convert password to secure string
                var secureString = new SecureString();
                Password.ToCharArray().ToList().ForEach(c => secureString.AppendChar(c));
                Credential = new PSCredential(Username, secureString);
            }

            if (ParameterSets.MODULE_CONTEXT.Equals(ParameterSetName))
            {
                Contract.Assert(null != ModuleConfiguration.Current.Credential);
                Contract.Assert(null != ModuleConfiguration.Current.ApiBaseUri);

                Credential = ModuleConfiguration.Current.Credential;
                ApiBaseUri = ModuleConfiguration.Current.ApiBaseUri;
            }

            var dataServiceClients = CreateDataServiceClients();

            // perform login
            // DFTODO - support negotiate authentication
            string loginEndpoint = ResolveLoginEndpoint(Credential);

            try
            {
                ModuleConfiguration.Current.TraceSource.TraceEvent(TraceEventType.Verbose, (int)Constants.Logging.EventId.EnterServer, Messages.EnterServer_ProcessRecord__Login, ApiBaseUri.AbsoluteUri, loginEndpoint);

                dataServiceClients[nameof(Api::Net.Appclusive.Api.Core.Core)].InvokeEntitySetActionWithSingleResult<User>(nameof(Api::Net.Appclusive.Api.Core.Core.Authentications), loginEndpoint, null);

                ModuleConfiguration.Current.TraceSource.TraceEvent(TraceEventType.Information, (int)Constants.Logging.EventId.EnterServer, Messages.EnterServer_ProcessRecord__LoginSucceeded, ApiBaseUri.AbsoluteUri, loginEndpoint);
            }
            catch (AggregateException aggrex)
            {
                ModuleConfiguration.Current.TraceSource.TraceEvent(TraceEventType.Error, (int)Constants.Logging.EventId.EnterServer, Messages.EnterServer_ProcessRecord__LoginFailed, ApiBaseUri.AbsoluteUri, loginEndpoint);

                if (null == ProcessAggregateException(aggrex))
                {
                    return;
                }

                throw;
            }

            // Set DataServiceClients variable of current configuration after successful login
            ModuleConfiguration.Current.DataServiceClients = dataServiceClients;
            WriteObject(dataServiceClients);
        }

        private string ResolveLoginEndpoint(PSCredential credential)
        {
            if (credential.UserName == Authentication.AUTHORIZATION_BAERER_USER_NAME)
            {
                return "BearerLogin";
            }

            return "BasicLogin";
        }

        private Dictionary<string, DataServiceContextBase> CreateDataServiceClients()
        {
            Contract.Requires(null != ApiBaseUri);
            Contract.Requires(null != Credential);
            Contract.Ensures(Contract.Result<Dictionary<string, DataServiceContextBase>>().ContainsKey(nameof(Api::Net.Appclusive.Api.Core.Core)));

            var dataServiceClients = new Dictionary<string, DataServiceContextBase>();
            var credential = Credential.GetNetworkCredential();

            var dataServiceClientTypes =
                typeof(DataServiceContextBase).Assembly.DefinedTypes.Where(
                    t => t.BaseType == typeof(DataServiceContextBase));

            foreach (var dataServiceClientType in dataServiceClientTypes)
            {
                var serviceRootUri = new Uri(UriHelper.ConcatUri(ApiBaseUri.AbsoluteUri, dataServiceClientType.Name));
                var dataServiceClient = (DataServiceContextBase)Activator.CreateInstance(dataServiceClientType, serviceRootUri);
                dataServiceClient.Credentials = credential;

                if (default(Guid) != TenantId)
                {
                    dataServiceClient.TenantId = TenantId.ToString();
                }

                dataServiceClients.Add(dataServiceClientType.Name, dataServiceClient);
            }

            return dataServiceClients;
        }

        // this exception is rather nasty - so let's try to extract something useful from it
        private Exception ProcessAggregateException(AggregateException exception)
        {
            Contract.Requires(null != exception);

            var httpReqEx = exception.InnerExceptions.FirstOrDefault();
            if (null == httpReqEx)
            {
                return exception;
            }
            var ex = httpReqEx.InnerException as WebException;
            if (null == ex)
            {
                return exception;
            }

            var errorRecord = new ErrorRecord(ex, Constants.Logging.EventId.EnterServerFailed.ToString(), ErrorCategory.ConnectionError, this);
            WriteError(errorRecord);

            return null;
        }
    }
}
