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

using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Management.Automation;
using biz.dfch.CS.PowerShell.Commons.Converters;

namespace Net.Appclusive.PS.Client
{
    /// <summary>
    /// This class defines the module context and configuration of the module
    /// </summary>
    public class ModuleContextConfigurationSection : ConfigurationSection
    {
        private const string API_BASE_URI_PROPERTY_NAME = "apiBaseUri";
        private const string CREDENTIAL_PROPERTY_NAME = "credential";
        private const string SOURCE_LEVELS_PROPERTY_NAME = "sourceLevels";

        /// <summary>
        /// The name of the configuration section
        /// </summary>
        public const string SECTION_NAME = ModuleConfiguration.MODULE_VARIABLE_NAME;


        /// <summary>
        /// Specifies the base uri of the Appclusive API
        /// </summary>
        [ConfigurationProperty(API_BASE_URI_PROPERTY_NAME, DefaultValue = "http://appclusive/api", IsRequired = false)]
        public Uri ApiBaseUri
        {
            get { return (Uri)this[API_BASE_URI_PROPERTY_NAME]; }
            set { this[API_BASE_URI_PROPERTY_NAME] = value; }
        }

        /// <summary>
        /// Specifies the credential consisting of a comma separated string to connect with
        /// </summary>
        [TypeConverter(typeof(PsCredentialTypeConverter))]
        [ConfigurationProperty(CREDENTIAL_PROPERTY_NAME, DefaultValue = "Arbitrary,P@ssw0rd", IsRequired = false)]
        public PSCredential Credential
        {
            get { return (PSCredential)this[CREDENTIAL_PROPERTY_NAME]; }
            set { this[CREDENTIAL_PROPERTY_NAME] = value; }
        }

        /// <summary>
        /// Specifies the source levels used for logging
        /// </summary>
        [ConfigurationProperty(SOURCE_LEVELS_PROPERTY_NAME, DefaultValue = SourceLevels.All, IsRequired = false)]
        public SourceLevels SourceLevels
        {
            get { return (SourceLevels)this[SOURCE_LEVELS_PROPERTY_NAME]; }
            set { this[SOURCE_LEVELS_PROPERTY_NAME] = value; }
        }
    }
}
