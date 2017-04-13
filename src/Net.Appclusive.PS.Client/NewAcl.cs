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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Management.Automation;
using Api::Net.Appclusive.Api;
using Api::Net.Appclusive.Public.Domain.Security;
using biz.dfch.CS.PowerShell.Commons;

namespace Net.Appclusive.PS.Client
{
    /// <summary>
    /// This class defines the NewAcl Cmdlet that creates a new Acl
    /// </summary>
    [Cmdlet(
        VerbsCommon.New, nameof(Acl)
        ,
        ConfirmImpact = ConfirmImpact.Low
        ,
        SupportsShouldProcess = true
        ,
        HelpUri = "http://docs.appclusive.net/en/latest/Clients/PowerShell/#new-cmdlets"
    )]
    [OutputType(typeof(Acl))]
    public class NewAcl : PsCmdletBase
    {
        /// <summary>
        /// Specifies the name of the Acl
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        [ValidatePattern("^[a-zA-Z][a-zA-Z0-9 _]+$")]
        public string Name { get; set; }

        /// <summary>
        /// Specifies the ParentId of the Acl
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        [ValidateRange(1, long.MaxValue)]
        public long ParentId { get; set; }

        /// <summary>
        /// Specifies the description of the Acl
        /// </summary>
        [Parameter(Mandatory = false)]
        // DFTODO - set name as default value
        //[PSDefaultValue(Value = )]
        public string Description { get; set; }

        /// <summary>
        /// Specifies NoInheritance of Acl
        /// </summary>
        [Parameter(Mandatory = false)]
        [PSDefaultValue(Value = false)]
        public SwitchParameter NoInheritance { get; set; }

        /// <summary>
        /// Data Service Clients for Appclusive
        /// </summary>
        [Parameter(Mandatory = false)]
        [Alias("Services")]
        [ValidateNotNull]
        public Dictionary<string, DataServiceContextBase> Svc { get; set; }

        /// <summary>
        /// ProcessRecord
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (null == Svc)
            {
                Svc = ModuleConfiguration.Current.DataServiceContexts;
            }

            Contract.Assert(null != Svc, Messages.Cmdlet_ProcessRecord__NotLoggedIn);

            var shouldProcessMessage = nameof(NewAcl);
            if (!ShouldProcess(shouldProcessMessage))
            {
                return;
            }

            var acl = new Acl
            {
                Name = Name,
                Description = Description,
                ParentId = ParentId,
                NoInheritance = NoInheritance
            };

            var coreContext = (Api::Net.Appclusive.Api.Core.Core)Svc[nameof(Api::Net.Appclusive.Api.Core.Core)];

            coreContext.AddToAcls(acl);
            var response = Svc[nameof(Api::Net.Appclusive.Api.Core.Core)].SaveChanges();
            Contract.Assert(201 == response.BatchStatusCode);

            WriteObject(acl);
        }
    }
}
