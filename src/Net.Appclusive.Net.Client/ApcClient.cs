/**
 * Copyright 2018 d-fens GmbH
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

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Security;
using biz.dfch.CS.Commons.Diagnostics;
using Net.Appclusive.Public.Constants;
using Authentication = Net.Appclusive.Api.Constants.Authentication;
using TraceSource = biz.dfch.CS.Commons.Diagnostics.TraceSource;

namespace Net.Appclusive.Net.Client
{
    public class ApcClient
    {
        private static readonly TraceSource _logger = Logger.Get(Logging.TraceSourceName.APPCLUSIVE_NET_CLIENT);

        /// <summary>
        /// Base URI of the Appclusive API
        /// </summary>
        public Uri ApiBaseUri { get; protected set; }

        /// <summary>
        /// Credentials for authentication purposes
        /// </summary>
        public ICredentials Credentials { get; protected set; }
        
        /// <summary>
        /// Indicates, if the login succeeded using the provided API base URI in combination with the credentials
        /// </summary>
        public bool IsLoggedIn { get; protected set; }

        public ApcClient(string apiBaseUri)
        {
            Contract.Requires(!string.IsNullOrEmpty(apiBaseUri));
            Contract.Requires(Uri.IsWellFormedUriString(apiBaseUri, UriKind.Absolute));

            ApiBaseUri = new Uri(apiBaseUri);
        }

        public void Login(string oAuth2Token)
        {
            Login(Authentication.AUTHORIZATION_BAERER_USER_NAME, oAuth2Token);
        }

        public bool Login(string username, string password)
        {
            var secureString = new SecureString();
            password.ToCharArray().ToList().ForEach(c => secureString.AppendChar(c));
            Credentials = new NetworkCredential(username, secureString);

            // var dataServiceContexts = CreateDataServiceContexts();

            // var loginEndpoint = ResolveLoginEndpoint(Credential);

            _logger.TraceEvent(TraceEventType.Information, (int)Logging.EventId.Start, Messages.ApcClient_Login__START);

            // dataServiceContexts[nameof(Api::Net.Appclusive.Api.Core.Core)].InvokeEntitySetActionWithSingleResult<User>(nameof(Api::Net.Appclusive.Api.Core.Core.Authentications), loginEndpoint, null);

            _logger.TraceEvent(TraceEventType.Information, (int)Logging.EventId.Stop, Messages.ApcClient_Login__SUCCEEDED);

            return true;
        }

        public void Logout()
        {
            // DFTODO - add logging

            // DFTODO - logout
            // DFTODO - set isLoggedIn to false

            // DFTODO - add logging
        }

        // DFTODO - support tenant switch by setting tenantId differently (either by a property or by a method that sets the tenantId in Contexts)

        // DFTODO - CRUD
        // DFTODO - get by id (entity)
        // DFTODO - get by id (tenant)
        // DFTODO - get by filter
        // DFTODO - attachIfNeeded on update

        // DFTODO - action invocation for single entity (void, single and collection result)
        // DFTODO - action invocation for entity set (void, single and collection result)

        // DFTODO - get metadata???
    }
}
