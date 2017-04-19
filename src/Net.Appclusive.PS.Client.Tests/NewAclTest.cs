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
using System.Data.Services.Client;
using System.Linq;
using System.Management.Automation;
using Api::Net.Appclusive.Api;
using biz.dfch.CS.Testing.Attributes;
using biz.dfch.CS.Testing.PowerShell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Api::Net.Appclusive.Public.Domain.Security;
using Telerik.JustMock;

namespace Net.Appclusive.PS.Client.Tests
{
    [TestClass]
    public class NewAclTest
    {
        private const string ACL_NAME = "ArbitraryAcl";
        private const long PARENT_ID = 1;

        private readonly Type sut = typeof(NewAcl);

        [TestMethod]
        [ExpectParameterBindingException(MessagePattern = @"'Name'.+'System\.String'")]
        public void InvokeWithNotSpecifiedNameThrowsParameterBindingException()
        {
            var parameters = @"-Name ";
            PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectParameterBindingValidationException(MessagePattern = @"'Name'")]
        public void InvokeWithEmptyNameParameterThrowsParameterBindingValidationException()
        {
            var parameters = string.Format("-Name '' -ParentId {0}", PARENT_ID);
            PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectParameterBindingValidationException(MessagePattern = @"'Name'")]
        public void InvokeWithNullNameParameterThrowsParameterBindingValidationException()
        {
            var parameters = string.Format("-Name $null -ParentId {0}", PARENT_ID);
            PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectParameterBindingValidationException(MessagePattern = @"'Name'")]
        public void InvokeWithInvalidNameParameterThrowsParameterBindingValidationException()
        {
            var invalidName = "invalid-name";
            var parameters = string.Format("-Name '{0}'", invalidName);
            PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectParameterBindingException(MessagePattern = @"Name")]
        public void InvokeWithMissingNameParameterThrowsParameterBindingException()
        {
            var parameters = string.Format("-ParentId {0}", PARENT_ID);
            PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectParameterBindingException(MessagePattern = @"'ParentId'.+.System\.Int64.")]
        public void InvokeWithInvalidParentIdParameterThrowsParameterBindingException()
        {
            var parameters = string.Format("-Name '{0}' -ParentId ivalid-parentId", ACL_NAME);
            PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectParameterBindingValidationException(MessagePattern = @"'ParentId'")]
        public void InvokeWithZeroParentIdParameterThrowsParameterBindingValidationException()
        {
            var parameters = string.Format("-Name '{0}' -ParentId 0", ACL_NAME);
            PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectParameterBindingException(MessagePattern = @"ParentId")]
        public void InvokeWithMissingParentIdParameterThrowsParameterBindingException()
        {
            var parameters = string.Format("-Name '{0}'", ACL_NAME);
            PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectParameterBindingValidationException(MessagePattern = @"'Svc'")]
        public void InvokeWithNullSvcParameterThrowsParameterBindingValidationException()
        {
            var parameters = string.Format("-Name '{0}' -ParentId {1} -Svc $null", ACL_NAME, PARENT_ID);
            PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectedException(typeof(IncompleteParseException))]
        public void InvokeWithInvalidStringThrowsIncompleteParseException()
        {
            // Arrange
            // missing string terminator
            var parameters = string.Format(@"-Name '{0}", ACL_NAME);

            // Act
            PsCmdletAssert.Invoke(sut, parameters);

            // Assert
        }

        [TestCategory("SkipOnTeamCity")]
        [TestMethod]
        public void InvokeNewAclWithValidParametersCallsApiAndCreatesAcl()
        {
            // Arrange
            var coreContext = Mock.Create<Api::Net.Appclusive.Api.Core.Core>();
            var response = Mock.Create<DataServiceResponse>();
            var changeOperationResponse = Mock.Create<ChangeOperationResponse>();

            Mock.Arrange(() => coreContext.AddToAcls(Arg.IsAny<Acl>()))
                .DoNothing()
                .OccursOnce();

            Mock.Arrange(() => coreContext.SaveChanges())
                .Returns(response)
                .OccursOnce();

            Mock.Arrange(() => response.FirstOrDefault())
                .Returns(changeOperationResponse)
                .OccursOnce();

            Mock.Arrange(() => changeOperationResponse.StatusCode)
                .Returns(201)
                .OccursOnce();

            var svc = new Dictionary<string, DataServiceContextBase>
            {
                {
                    nameof(Api::Net.Appclusive.Api.Core.Core),
                    coreContext
                }
            };

            var parameters = new Dictionary<string, object>()
            {
                {nameof(NewAcl.Name), ACL_NAME},
                {nameof(NewAcl.ParentId), PARENT_ID},
                {nameof(svc), svc}
            };

            // Act
            var results = PsCmdletAssert.Invoke(sut, parameters);

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);

            var acl = results[0].BaseObject as Acl;
            Assert.IsNotNull(acl);
            Assert.AreEqual(ACL_NAME, acl.Name);
            Assert.AreEqual(PARENT_ID, acl.ParentId);
            Assert.IsFalse(acl.NoInheritance);

            Mock.Assert(coreContext);
            Mock.Assert(response);
            Mock.Assert(changeOperationResponse);
        }
    }
}
