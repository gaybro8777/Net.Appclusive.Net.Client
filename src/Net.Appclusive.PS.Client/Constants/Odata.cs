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

namespace Net.Appclusive.PS.Client.Constants
{
    public static class Odata
    {
        public const string BY_ID_LONG_QUERY_TEMPLATE = "Id eq {0}L";
        public const string BY_ID_GUID_QUERY_TEMPLATE = "Id eq guid'{0}'";
        public const string BY_NAME_QUERY_TEMPLATE = "Name eq '{0}'";
    }
}
