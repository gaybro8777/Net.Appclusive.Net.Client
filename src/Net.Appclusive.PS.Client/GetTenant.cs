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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Management.Automation;
using Api::Net.Appclusive.Api;
using Api::Net.Appclusive.Public.Domain.Identity;
using biz.dfch.CS.Commons.Linq;
using biz.dfch.CS.PowerShell.Commons;
using Net.Appclusive.PS.Client.Constants;

namespace Net.Appclusive.PS.Client
{
    /// <summary>
    /// This class defines the GetTenant Cmdlet that retrieves a list of tenants
    /// </summary>
    [Cmdlet(
         VerbsCommon.Get, nameof(Tenant)
         ,
         ConfirmImpact = ConfirmImpact.Low
         ,
         DefaultParameterSetName = ParameterSets.LIST
         ,
         SupportsShouldProcess = true
         ,
         HelpUri = "http://docs.appclusive.net/en/latest/Clients/PowerShell/#get-tenant"
     )]
    [OutputType(typeof(Tenant))]
    public class GetTenant : PsCmdletBase
    {
        /// <summary>
        /// Defines all valid parameter sets for this cmdlet
        /// </summary>
        public static class ParameterSets
        {
            /// <summary>
            /// ParameterSetName used when requesting all entities
            /// </summary>
            public const string LIST = "list";

            /// <summary>
            /// ParameterSetName used when requesting an entity by id
            /// </summary>
            public const string ID = "id";

            /// <summary>
            /// ParameterSetName used when requesting an entity by name
            /// </summary>
            public const string NAME = "name";
        }

        /// <summary>
        /// Specifies the entity id
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSets.ID)]
        [ValidateRange(1, int.MaxValue)]
        public int Id { get; set; }

        /// <summary>
        /// Specifies the entity name
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSets.NAME)]
        public string Name { get; set; }

        /// <summary>
        /// Retrieve all entities for the current tenant
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSets.LIST)]
        public SwitchParameter ListAvailable { get; set; }

        /// <summary>
        /// Data Service Clients for Appclusive
        /// </summary>
        [Parameter(Mandatory = false)]
        [Alias("Services")]
        public Dictionary<string, DataServiceContextBase> Svc { get; set; }

        /// <summary>
        /// ProcessRecord
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (null == Svc)
            {
                Svc = ModuleConfiguration.Current.DataServiceClients;
            }

            Contract.Assert(null != Svc, Messages.Cmdlet_ProcessRecord__NotLoggedIn);

            var shouldProcessMessage = string.Format(Messages.Cmdlet_ProcessRecord__ShouldProcess, ParameterSetName);
            if (!ShouldProcess(shouldProcessMessage))
            {
                return;
            }

            switch (ParameterSetName)
            {
                case ParameterSets.LIST:
                    {
                        ProcessParameterSetList();
                        return;
                    }

                case ParameterSets.ID:
                    {
                        ProcessParameterSetId();
                        return;
                    }

                case ParameterSets.NAME:
                    {
                        ProcessParameterSetName();
                        return;
                    }

                default:
                    bool isValidParameterSetName = false;
                    Contract.Assert(isValidParameterSetName, ParameterSetName);
                    break;
            }

        }

        private void ProcessParameterSetId()
        {
            try
            {
                var query = string.Format(Odata.BY_ID_QUERY_TEMPLATE, Id);
                var coreDataServiceClient = (Api::Net.Appclusive.Api.Core.Core)ModuleConfiguration.Current.DataServiceClients[nameof(Api::Net.Appclusive.Api.Core.Core)];
                var result = coreDataServiceClient.Tenants.AddQueryOption(Odata.FILTER, query);
                WriteObject(result);
            }
            catch (Exception ex)
            {
                WriteError(ErrorRecordFactory.GetGeneric(ex));
                WriteError(ErrorRecordFactory.GetNotFound(Messages.Cmdlet_ProcessParameterSetId__NotFound, Constants.Logging.EventId.GetTenantIdNotFound.ToString(), nameof(Tenant), Id), writeToTraceSource: true);
            }
        }

        private void ProcessParameterSetName()
        {
            var query = string.Format(Odata.BY_NAME_QUERY_TEMPLATE, Name);
            var coreDataServiceClient = (Api::Net.Appclusive.Api.Core.Core)ModuleConfiguration.Current.DataServiceClients[nameof(Api::Net.Appclusive.Api.Core.Core)];
            var results = coreDataServiceClient.Tenants.AddQueryOption(Odata.FILTER, query).Execute();

            if (!results.Any())
            {
                WriteError(ErrorRecordFactory.GetNotFound(Messages.Cmdlet_ProcessParamaterSetName__NotFound, Constants.Logging.EventId.GetTenantNameNotFound.ToString(), nameof(Tenant), Name));
                return;
            }

            results.ForEach(WriteObject);
        }

        private void ProcessParameterSetList()
        {
            var coreDataServiceClient = (Api::Net.Appclusive.Api.Core.Core)ModuleConfiguration.Current.DataServiceClients[nameof(Api::Net.Appclusive.Api.Core.Core)];
            var results = coreDataServiceClient.Tenants.Execute();
            results.ForEach(WriteObject);
        }
    }
}
