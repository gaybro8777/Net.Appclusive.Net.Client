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
using System.IO;
using biz.dfch.CS.Testing.Attributes;
using biz.dfch.CS.Testing.PowerShell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Net.Appclusive.PS.Client.Tests
{
    [TestClass]
    public class ImportConfigurationTest
    {
        private readonly Type sut = typeof(ImportConfiguration);
        
        [TestMethod]
        [ExpectParameterBindingException(MessagePattern = "Path")]
        public void InvokeWithEmptyPathThrowsContractException()
        {
            var parameters = @"-Path ''";
            PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestCategory("SkipOnTeamCity")]
        [TestMethod]
        [ExpectContractFailure(MessagePattern = "Directory.Exists.fileInfo.FullName.+existing-directory")]
        public void InvokeWithExistingDirectoryAsPathThrowsContractException()
        {
            Mock.SetupStatic(typeof(Directory));
            Mock.Arrange(() => Directory.Exists(Arg.IsAny<string>()))
                .OnAllThreads()
                .Returns(true);

            var parameters = @"-Path existing-directory";
            PsCmdletAssert.Invoke(sut, parameters, ex => ex);
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "File.Exists.fileInfo.FullName.+invalid-configuration-file-name")]
        public void InvokeWithInvalidPathThrowsContractException()
        {
            var parameters = @"-Path invalid-configuration-file-name";
            PsCmdletAssert.Invoke(sut, parameters, ex => ex);
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = ModuleConfiguration.CONFIGURATION_FILE_NAME)]
        public void InvokeWithEmptyPathResolvesDefaultConfigurationFileName()
        {
            Mock.SetupStatic(typeof(File));
            Mock.Arrange(() => File.Exists(Arg.IsAny<string>()))
                .OnAllThreads()
                .Returns(false);

            var parameters = @";";
            PsCmdletAssert.Invoke(sut, parameters, ex => ex);
        }

        [TestCategory("SkipOnTeamCity")]
        [TestMethod]
        public void InvokeWithEmptyPathSucceeds()
        {
            var parameters = @";";
            var results = PsCmdletAssert.Invoke(sut, parameters);
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            var result = results[0].BaseObject;
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ModuleContext));
        }

        [TestCategory("SkipOnTeamCity")]
        [TestMethod]
        public void InvokeWithEmptyPathAndDisplayOnlyTrueSucceeds()
        {
            var parameters = @"-DisplayOnly:$true; Get-Variable net_Appclusive_PS_Client -ValueOnly -ErrorAction:SilentlyContinue;";
            var results = PsCmdletAssert.Invoke(sut, parameters);
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            var result = results[0].BaseObject;
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ModuleContext));
        }

        [TestCategory("SkipOnTeamCity")]
        [TestMethod]
        public void InvokeWithEmptyPathAndDisplayOnlyFalseSucceeds()
        {
            var parameters = @"-DisplayOnly:$false; Get-Variable net_Appclusive_PS_Client -ValueOnly;";
            var results = PsCmdletAssert.Invoke(sut, parameters);
            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Count);

            var moduleContext = results[0].BaseObject;
            Assert.IsNotNull(moduleContext);
            Assert.IsInstanceOfType(moduleContext, typeof(ModuleContext));

            var variable = results[1].BaseObject;
            Assert.IsNotNull(variable);
            Assert.IsInstanceOfType(variable, typeof(ModuleContext));

            Assert.AreEqual(variable, moduleContext);
        }
    }
}
