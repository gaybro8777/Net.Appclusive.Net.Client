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

using Net.Appclusive.Api.Constants;
using System;
using System.Linq;
using System.Net;
using System.Security;

namespace Net.Appclusive.Net.Client
{
    public class ApcClient
    {
        public Uri ApiBaseUri { get; protected set; }

        // DFTODO - AuthenticationInformation (protected setter)
        private ICredentials Credentials {get; set; }

        // DFTODO - TenantId
        // DFTODO - support tenant switch by setting tenantId differently
        
        /// <summary>
        /// Indicates that the call to the /login endpoint succeeded
        /// with the provided authentication information.
        /// </summary>
        public bool IsLoggedIn { get; protected set; }

        public ApcClient(Uri apiBaseUri)
        {
            ApiBaseUri = apiBaseUri;
        }

        // DFTODO - login -> uri, authenticationInformation
        public void Authenticate(string oAuth2Token)
        {
            Authenticate(Authentication.AUTHORIZATION_BAERER_USER_NAME, oAuth2Token);
        }

        public void Authenticate(string username, string password)
        {
            var secureString = new SecureString();
            password.ToCharArray().ToList().ForEach(c => secureString.AppendChar(c));
            Credentials = new NetworkCredential(username, secureString);

            // var dataServiceContexts = CreateDataServiceContexts();

            // var loginEndpoint = ResolveLoginEndpoint(Credential);

            // DFTODO - add logging

            // dataServiceContexts[nameof(Api::Net.Appclusive.Api.Core.Core)].InvokeEntitySetActionWithSingleResult<User>(nameof(Api::Net.Appclusive.Api.Core.Core.Authentications), loginEndpoint, null);

            // DFTODO - add logging
        }

        // DFTODO - logout

        // DFTODO - CRUD
    }
}
