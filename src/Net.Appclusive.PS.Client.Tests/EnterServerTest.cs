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
using biz.dfch.CS.Testing.Attributes;
using biz.dfch.CS.Testing.PowerShell;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Net.Appclusive.PS.Client.Tests
{
    [TestClass]
    public class EnterServerTest
    {
        private readonly Type sut = typeof(EnterServer);

        [TestMethod]
        [ExpectParameterBindingException(MessagePattern = @"'ApiBaseUri'.+'System\.Uri'")]
        public void InvokeWithInvalidApiBaseUriParameterThrowsParameterBindingException1()
        {
            var parameters = @"-ApiBaseUri ";
            PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectParameterBindingException(MessagePattern = @"Username.+Password")]
        public void InvokeWithMissingParameterThrowsParameterBindingException()
        {
            var parameters = @"-ApiBaseUri https://appclusive.example.com/api/";
            PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectParameterBindingValidationException(MessagePattern = @"Credential")]
        public void InvokeWithNullCredentialParameterThrowsParameterBindingValidationException()
        {
            var parameters = @"-ApiBaseUri https://appclusive.example.com/api/ -Credential $null";
            PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectParameterBindingException(MessagePattern = @"'Credential'.+.System\.String.")]
        public void InvokeWithInvalidCredentialParameterThrowsParameterBindingException()
        {
            var parameters = @"-ApiBaseUri https://appclusive.example.com/api/ -Credential arbitrary-user-as-string";
            PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestCategory("SkipOnTeamCity")]
        [TestMethod]
        public void InvokeWithParameterSetPlainSucceeds()
        {
            var uri = new Uri("http://appclusive/api/");
            var user = "Arbitrary";
            var password = "P@ssw0rd";
            var parameters = string.Format(@"-ApiBaseUri {0} -User '{1}' -Password '{2}'", uri, user, password);

            //Mock.Arrange(() => Client.Login(Arg.IsAny<string>(), Arg.IsAny<IAuthenticationInformation>()))
            //    .IgnoreInstance()
            //    .Returns(true);

            var results = PsCmdletAssert.Invoke(sut, parameters);

            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            //Assert.IsInstanceOfType(results[0].BaseObject, typeof(Dictionary<string, DataServ>));
            //var result = (BaseAbiquoClient)results[0].BaseObject;
        }
    }
}
