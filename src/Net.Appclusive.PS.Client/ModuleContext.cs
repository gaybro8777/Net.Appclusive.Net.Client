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
using System.Management.Automation;
using Api::Net.Appclusive.Api;
using biz.dfch.CS.Commons.Diagnostics;
using biz.dfch.CS.PowerShell.Commons;
using TraceSource = biz.dfch.CS.Commons.Diagnostics.TraceSource;

namespace Net.Appclusive.PS.Client
{
    /// <summary>
    /// ModuleContext
    /// </summary>
    public class ModuleContext
    {
        /// <summary>
        /// Base URI of the Appclusive API
        /// </summary>
        public Uri ApiBaseUri { get; set; }

        /// <summary>
        /// Credentials to use
        /// </summary>
        public PSCredential Credential { get; set; }

        /// <summary>
        /// Returns a dictionary of data service context references created during last Enter-Server call
        /// </summary>
        public Dictionary<string, DataServiceContextBase> DataServiceClients { get; set; }

        private static readonly Lazy<TraceSource> _traceSource = new Lazy<TraceSource>(() =>
        {
            Contract.Ensures(null != Contract.Result<TraceSource>());

            var traceSource = Logger.Get(ModuleConfiguration.LOGGER_NAME);

            ContractFailedEventHandler.RegisterTraceSource(traceSource);

            return traceSource;
        });

        /// <summary>
        /// Returns a reference to the TraceSource instance of this module
        /// </summary>
        public TraceSource TraceSource => _traceSource.Value;
    }
}
