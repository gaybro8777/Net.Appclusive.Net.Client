#
# module initialisation script goes here
#

$traceSource = [biz.dfch.CS.Commons.Diagnostics.Logger]::Get([Net.Appclusive.PS.Client.ModuleConfiguration]::LOGGER_NAME);
$traceSource.TraceTransfer(0, ("[{0}] Host.InstanceId" -f $PID), $Host.InstanceId);

$path = [Net.Appclusive.PS.Client.ModuleConfiguration]::ResolveConfigurationFileInfo($null)
$moduleContextSection = [Net.Appclusive.PS.Client.ModuleConfiguration]::GetModuleContextConfigurationSection($path);
[Net.Appclusive.PS.Client.ModuleConfiguration]::SetModuleContext($moduleContextSection)

Set-Variable -Name $([Net.Appclusive.PS.Client.ModuleConfiguration]::MODULE_VARIABLE_NAME) -Value $([Net.Appclusive.Client.ModuleConfiguration]::Current) -Scope Global;

# 
# Copyright 2013-2017 d-fens GmbH
# 
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# 
# http://www.apache.org/licenses/LICENSE-2.0
# 
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# 
