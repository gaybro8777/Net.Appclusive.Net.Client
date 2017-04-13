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

using System.Diagnostics.Contracts;
using System.IO;
using System.Management.Automation;
using biz.dfch.CS.PowerShell.Commons;

namespace Net.Appclusive.PS.Client
{
    /// <summary>
    /// This class defines the ImportConfiguration Cmdlet that imports the configuration of the module
    /// </summary>
    [Cmdlet(
         VerbsData.Import, "Configuration"
         ,
         ConfirmImpact = ConfirmImpact.Medium
         ,
         SupportsShouldProcess = true
         ,
         HelpUri = "http://docs.appclusive.net/en/latest/Clients/PowerShell/#configuration"
    )]
    [OutputType(typeof(ModuleContext))]
    public class ImportConfiguration : PsCmdletBase
    {
        /// <summary>
        /// Specifies the configuration file name to be imported. If no value is specified the default configuration file name will be used.
        /// </summary>
        [Parameter(Mandatory = false, Position = 0, ValueFromPipeline = true)]
        public FileInfo Path { get; set; }

        /// <summary>
        /// Specifies whether the configuration should be saved to the module configuration variable
        /// </summary>
        [Parameter(Mandatory = false)]
        [Alias("show")]
        [PSDefaultValue(Value = false)]
        public SwitchParameter DisplayOnly { get; set; }

        /// <summary>
        /// Main processing logic.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Path = ModuleConfiguration.ResolveConfigurationFileInfo(Path);

            var shouldProcessMessage = string.Format(Messages.ImportConfiguration_ProcessRecord__ShouldProcess, Path.FullName);
            if (!ShouldProcess(shouldProcessMessage))
            {
                return;
            }

            var moduleContextSection = ModuleConfiguration.GetModuleContextConfigurationSection(Path);
            ModuleConfiguration.SetModuleContext(moduleContextSection);
            WriteObject(ModuleConfiguration.Current);

            if (DisplayOnly)
            {
                return;
            }

            SessionState.PSVariable.Set(ModuleConfiguration.MODULE_VARIABLE_NAME, ModuleConfiguration.Current);

            var importedModuleVariable = GetVariableValue(ModuleConfiguration.MODULE_VARIABLE_NAME);
            Contract.Assert(null != importedModuleVariable, Messages.ImportConfiguration_ProcessRecord__SaveToModuleVariableFailed);
        }
    }
}
