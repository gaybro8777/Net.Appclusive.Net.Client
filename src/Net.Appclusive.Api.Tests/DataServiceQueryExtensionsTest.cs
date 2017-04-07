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
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using biz.dfch.CS.Testing.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Net.Appclusive.Api.Constants;
using Net.Appclusive.Public.Domain.Identity;
using Telerik.JustMock;
using System.Linq.Dynamic;
using Telerik.JustMock.Helpers;

namespace Net.Appclusive.Api.Tests
{
    [TestClass]
    public class DataServiceQueryExtensionsTest
    {
        [ExpectContractFailure(MessagePattern = "Precondition.+filterValue")]
        [TestMethod]
        public void FilterExtensionMethodWithNullFilterValueThrowsContractException()
        {
            // Arrange
            var sut = Mock.Create<DataServiceQuery<Tenant>>();

            // Act
            sut.Filter(null);

            // Assert
        }

        [TestMethod]
        public void FilterAddsFilterQueryOptionToDataServiceQuery()
        {
            // Arrange
            var sut = Mock.Create<DataServiceQuery<Tenant>>();
            var filterValue = "Name eq 'Arbitrary'";

            Mock.Arrange(() => sut.AddQueryOption(DataService.QueryOption.FILTER, filterValue))
                .Returns(sut)
                .OccursOnce();

            // Act
            DataServiceQueryExtensions.Filter(sut, filterValue);

            // Assert
            Mock.Assert(sut);
        }

        [TestMethod]
        public void FilterExtensionMethodAddsFilterQueryOptionToDataServiceQuery()
        {
            // Arrange
            var sut = Mock.Create<DataServiceQuery<Tenant>>();
            var filterValue = "Name eq 'Arbitrary'";

            Mock.Arrange(() => sut.AddQueryOption(DataService.QueryOption.FILTER, filterValue))
                .Returns(sut)
                .OccursOnce();

            // Act
            sut.Filter(filterValue);

            // Assert
            Mock.Assert(sut);
        }

        [ExpectContractFailure(MessagePattern = "Precondition.+id")]
        [TestMethod]
        public void IdExtensionMethodWithInvalidLongIdThrowsContractException()
        {
            // Arrange
            var sut = Mock.Create<DataServiceQuery<User>>();
            var id = 0L;

            // Act
            sut.Id(id);

            // Assert
        }

        [TestMethod]
        public void IdExtensionMethodAppliesIdToUri()
        {
            // Arrange
            var sut = Mock.Create<DataServiceQuery<User>>();
            var id = 42L;
            var users = new List<User>
            {
                new User()
            };

            Mock.Arrange(() => sut.Where("Id == @0", id))
                .Returns(users.AsQueryable)
                .OccursOnce();

            // Act
            var result = sut.Id(id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(User));

            Mock.Assert(sut);
        }

        [ExpectContractFailure(MessagePattern = "Precondition.+id")]
        [TestMethod]
        public void IdExtensionMethodForTenantWithDefaultIdThrowsContractException()
        {
            // Arrange
            var sut = Mock.Create<DataServiceQuery<Tenant>>();
            var id = default(Guid);

            // Act
            sut.Id(id);

            // Assert
        }

        [TestMethod]
        public void IdExtensionMethodForTenantAppliesIdToUri()
        {
            // Arrange
            var sut = Mock.Create<DataServiceQuery<Tenant>>();
            var id = Guid.NewGuid();

            Mock.Arrange(() => sut.FirstOrDefault(entity => entity.Id == id))
                .Returns(new Tenant())
                .OccursOnce();

            // Act
            var result = sut.Id(id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(Tenant));

            Mock.Assert(sut);
        }
    }
}
