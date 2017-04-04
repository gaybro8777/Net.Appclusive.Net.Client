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

namespace Net.Appclusive.Api.Constants
{
    public static class Authentication
    {
        public const string AUTHORIZATION_BAERER_USER_NAME = "[bearer@auth.appclusive.net]";
        public const string AUTHORIZATION_BASIC_TEMPLATE = "Basic {0}";
        public const string AUTHORIZATION_BEARER_TEMPLATE = "Bearer {0}";

        public static class Header
        {
            public const string AUTHORIZATION = "Authorization";
            public const string DEFAULT_TENANT_HEADER_NAME = "TenantId";
        }
    }
}
