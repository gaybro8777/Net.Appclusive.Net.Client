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

using System.Configuration;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;

namespace Net.Appclusive.PS.Client
{
    /// <summary>
    /// The factory / manager class for returning the module context
    /// </summary>
    public class ModuleConfiguration
    {
        /// <summary>
        /// The default name of the configuration file
        /// </summary>
        public const string CONFIGURATION_FILE_NAME = "Net.Appclusive.PS.Client.config";

        /// <summary>
        /// The name of the module configuration variable
        /// </summary>
        public const string MODULE_VARIABLE_NAME = "net_Appclusive_PS_Client";

        /// <summary>
        /// LOGGER_NAME
        /// </summary>
        public const string LOGGER_NAME = "Net.Appclusive.PS.Client";

        /// <summary>
        /// Returns the current module context (singleton)
        /// </summary>
        /// <returns>ModuleContext containing module configuration information</returns>
        public static ModuleContext Current { get; } = new ModuleContext();

        /// <summary>
        /// Gets and validates a configuration file name or returns the default configuration file name
        /// </summary>
        /// <param name="fileInfo">FileInfo specifying a configuration file name or null</param>
        /// <returns>FileInfo specifying the configuration file name</returns>
        public static FileInfo ResolveConfigurationFileInfo(FileInfo fileInfo)
        {
            Contract.Ensures(null != Contract.Result<FileInfo>());

            if (null == fileInfo)
            {
                var location = typeof(ImportConfiguration).Assembly.Location;
                Contract.Assert(!string.IsNullOrWhiteSpace(location));

                var path = Path.GetDirectoryName(location);
                Contract.Assert(Directory.Exists(path), path);

                var configFile = Path.Combine(path, CONFIGURATION_FILE_NAME);

                fileInfo = new FileInfo(configFile);
            }

            Contract.Assert(!Directory.Exists(fileInfo.FullName), string.Format(Messages.ModuleConfiguration_ResolveConfigurationFileInfo__FileInfoIsDirectory, fileInfo.FullName));
            Contract.Assert(File.Exists(fileInfo.FullName), string.Format(Messages.ModuleConfiguration_ResolveConfigurationFileInfo__FileDoesNotExist, fileInfo.FullName));

            return fileInfo;
        }

        /// <summary>
        /// Gets the module context configuration section from the specified configuration file
        /// </summary>
        /// <param name="fileInfo">Specifies the location of the configuration file to be loaded</param>
        /// <returns>ModuleContextSection of current module</returns>
        public static ModuleContextConfigurationSection GetModuleContextConfigurationSection(FileInfo fileInfo)
        {
            Contract.Requires(null != fileInfo);
            Contract.Ensures(null != Contract.Result<ModuleContextConfigurationSection>());

            var configurationFileMap = new ConfigurationFileMap(fileInfo.FullName);
            Contract.Assert(null != configurationFileMap, string.Format(Messages.ModuleConfiguration_GetModuleContextConfigurationSection__FileMapCreationFailed, fileInfo.FullName));

            var configuration = ConfigurationManager.OpenMappedMachineConfiguration(configurationFileMap);
            Contract.Assert(null != configuration, string.Format(Messages.ModuleConfiguration_GetModuleContextConfigurationSection__OpenConfigurationFailed, fileInfo.FullName));
            Contract.Assert(configuration.HasFile, string.Format(Messages.ModuleConfiguration_GetModuleContextConfigurationSection__NoConfigurationFile, fileInfo.FullName));

            var moduleContextConfigurationSection = configuration.GetSection(ModuleContextConfigurationSection.SECTION_NAME) as ModuleContextConfigurationSection;
            Contract.Assert(null != moduleContextConfigurationSection, string.Format(Messages.ModuleConfiguration_GetModuleContextConfigurationSection__OpenConfigurationSectionFailed, fileInfo.FullName, ModuleContextConfigurationSection.SECTION_NAME));

            return moduleContextConfigurationSection;
        }

        /// <summary>
        /// Sets the module context based on a configuration section
        /// </summary>
        public static void SetModuleContext(ModuleContextConfigurationSection moduleContextConfigurationSection)
        {
            Contract.Requires(null != moduleContextConfigurationSection);

            const BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

            var propertyInfos = moduleContextConfigurationSection.GetType().GetProperties(BINDING_FLAGS);
            foreach (var propertyInfo in propertyInfos)
            {
                var configurationProperty = propertyInfo.GetCustomAttribute<ConfigurationPropertyAttribute>();
                if (null == configurationProperty)
                {
                    continue;
                }

                var targetPropertyInfo = Current.GetType().GetProperty(propertyInfo.Name, BINDING_FLAGS);
                if (null == targetPropertyInfo)
                {
                    continue;
                }

                var value = propertyInfo.GetValue(moduleContextConfigurationSection, null);
                targetPropertyInfo.SetValue(Current, value, null);
            }
        }
    }
}

