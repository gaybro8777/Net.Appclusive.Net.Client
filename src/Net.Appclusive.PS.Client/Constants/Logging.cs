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
    public static class Logging
    {
        private const int EVENT_ID_OFFSET = 64;

        /// <summary>
        /// Index for all cmdlets in this module
        /// </summary>
        public enum EventId
        {
            /// <summary>
            /// Enter-Server
            /// </summary>
            EnterServer = 16384,
            /// <summary>
            /// Login Failed AggregateException
            /// </summary>
            EnterServerFailed,
            /// <summary>
            /// Import-Configuration
            /// </summary>
            ImportConfiguration = EnterServer + EVENT_ID_OFFSET,
            /// <summary>
            /// Get-Machine
            /// </summary>
            GetTenant = ImportConfiguration + EVENT_ID_OFFSET,
            /// <summary>
            /// GetMachineIdNotFound
            /// </summary>
            GetTenantIdNotFound,
            /// <summary>
            /// GetMachineIdNotFound
            /// </summary>
            GetTenantNameNotFound,
        }
    }
}
