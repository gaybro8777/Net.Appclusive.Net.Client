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
using Api::Net.Appclusive.Api;
using Api::Net.Appclusive.Public.Domain.Identity;
using biz.dfch.CS.Testing.Attributes;
using biz.dfch.CS.Testing.PowerShell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Net.Appclusive.PS.Client.Constants;
using Telerik.JustMock;

namespace Net.Appclusive.PS.Client.Tests
{
    [TestClass]
    public class GetTenantTest
    {
        private readonly Type sut = typeof(GetTenant);

        [TestMethod]
        [ExpectParameterBindingException(MessagePattern = @"'Id'.+'System\.Guid'")]
        public void InvokeWithInvalidIdParameterThrowsParameterBindingException()
        {
            var parameters = @"-Id ";
            PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectParameterBindingException(MessagePattern = @"'Id'.+System\.Guid")]
        public void InvokeWithNonGuidIdParameterThrowsParameterBindingException()
        {
            var parameters = @"-Id 123";
            PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectParameterBindingValidationException(MessagePattern = @"Name")]
        public void InvokeWithEmptyNameParameterThrowsParameterBindingValidationException()
        {
            var parameters = @"-Name ''";
            PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectParameterBindingValidationException(MessagePattern = @"Svc")]
        public void InvokeWithNullSvcParameterThrowsParameterBindingValidationException()
        {
            var parameters = "-Svc $null";
            PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        [ExpectParameterBindingException(MessagePattern = @"'Svc'.+System\.Collections\.Generic\.Dictionary")]
        public void InvokeWithInvalidSvcParameterThrowsParameterBindingException()
        {
            var parameters = "-Svc 123";
            PsCmdletAssert.Invoke(sut, parameters);
        }

        [TestMethod]
        public void InvokeGetTenantListAvailableCallsApiAndReturnsResult()
        {
            // Arrange
            var tenant = CreateSampleTenant();
            var tenant2 = CreateSampleTenant();
            var tenants = new List<Tenant>
            {
                tenant,
                tenant2
            };

            var coreContext = Mock.Create<Api::Net.Appclusive.Api.Core.Core>();
            var dataServiceQuery = Mock.Create <DataServiceQuery<Tenant>>();

            Mock.Arrange(() => coreContext.Tenants)
                .Returns(dataServiceQuery)
                .OccursOnce();

            Mock.Arrange(() => dataServiceQuery.Execute())
                .Returns(tenants)
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
                {nameof(svc), svc}
            };

            // Act
            var results = PsCmdletAssert.Invoke(sut, parameters);

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Count);
            var result0 = results[0].BaseObject as Tenant;
            var result1 = results[1].BaseObject as Tenant;

            Assert.IsNotNull(result0);
            Assert.IsNotNull(result1);

            Mock.Assert(coreContext);
            Mock.Assert(dataServiceQuery);
        }

        [TestMethod]
        public void InvokeGetTenantWithIdParameterCallsApiWithFilterAndReturnsResult()
        {
            // Arrange
            var tenant = CreateSampleTenant();
            var tenantId = tenant.Id;

            var coreContext = Mock.Create<Api::Net.Appclusive.Api.Core.Core>();
            var dataServiceQuery = Mock.Create<DataServiceQuery<Tenant>>();

            Mock.Arrange(() => coreContext.Tenants)
                .Returns(dataServiceQuery)
                .OccursOnce();

            Mock.Arrange(() => dataServiceQuery.Id(tenantId))
                .Returns(tenant)
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
                {nameof(GetTenant.Id), tenantId},
                {nameof(svc), svc}
            };

            // Act
            var results = PsCmdletAssert.Invoke(sut, parameters);

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            var result0 = results[0].BaseObject as Tenant;

            Assert.IsNotNull(result0);
            Assert.AreEqual(tenantId, result0.Id);

            Mock.Assert(coreContext);
            Mock.Assert(dataServiceQuery);
        }

        [TestMethod]
        public void InvokeGetTenantWithNameParameterCallsApiWithFilterAndReturnsResult()
        {
            // Arrange
            var tenant = CreateSampleTenant();
            var tenant2 = CreateSampleTenant();
            var tenantName = tenant.Name;
            var tenants = new List<Tenant>
            {
                tenant,
                tenant2
            };

            var coreContext = Mock.Create<Api::Net.Appclusive.Api.Core.Core>();
            var dataServiceQuery = Mock.Create<DataServiceQuery<Tenant>>();

            Mock.Arrange(() => coreContext.Tenants)
                .Returns(dataServiceQuery)
                .OccursOnce();

            var query = string.Format(Odata.BY_NAME_QUERY_TEMPLATE, tenantName);
            Mock.Arrange(() => dataServiceQuery.Filter(query).Execute())
                .Returns(tenants)
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
                {nameof(GetTenant.Name), tenantName},
                {nameof(svc), svc}
            };

            // Act
            var results = PsCmdletAssert.Invoke(sut, parameters);

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Count);
            var result0 = results[0].BaseObject as Tenant;
            var result1 = results[1].BaseObject as Tenant;

            Assert.IsNotNull(result0);
            Assert.IsNotNull(result1);

            Mock.Assert(coreContext);
            Mock.Assert(dataServiceQuery);
        }

        private Tenant CreateSampleTenant()
        {
            return new Tenant
            {
                Id = default(Guid)
                ,
                Name = nameof(Tenant.Name)
                ,
                Description = nameof(Tenant.Description)
                ,
                MappedId = nameof(Tenant.MappedId)
                ,
                MappedType = nameof(Tenant.MappedType)
                ,
                ParentId = Guid.NewGuid()
                ,
                Namespace = "net.sharedop"
                ,
                CustomerId = 42
            };
        }
    }
}
