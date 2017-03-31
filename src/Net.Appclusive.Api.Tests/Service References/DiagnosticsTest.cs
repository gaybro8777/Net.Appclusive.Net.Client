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
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Net.Appclusive.Api.Diagnostics;
using Telerik.JustMock;

namespace Net.Appclusive.Api.Tests.Service_References
{
    [TestClass]
    public class DiagnosticsTest
    {
        private static readonly Uri _serviceRoot;

        static DiagnosticsTest()
        {
            var apiBaseUri = ConfigurationManager.AppSettings["Service.Reference.ServiceRoot"];
            _serviceRoot = new Uri(apiBaseUri + nameof(Diagnostics));
        }

        [TestMethod]
        public void AttachingDetachedEntitySucceeds()
        {
            // Arrange
            var svc = new Diagnostics.Diagnostics(_serviceRoot);
            var entity = new CacheItem();
            Mock.Arrange(() => svc.AttachTo(Arg.AnyString, Arg.AnyObject)).OccursOnce();

            // Act
            svc.AttachIfNeeded(entity);

            // Assert
            Mock.Assert(svc);
        }

        [TestMethod]
        public void AttachingEntityToInvalidEntitySetThrowsException()
        {
            // Arrange
            var svc = new Diagnostics.Diagnostics(_serviceRoot);
            var entity = new CacheItem();
            var entitySetName = "InvalidEntitySetName";
            Mock.Arrange(() => svc.AttachTo(Arg.AnyString, Arg.AnyObject)).OccursOnce();

            // Act
            svc.AttachIfNeeded(entitySetName, entity);

            // Assert
            Mock.Assert(svc);
        }
    }
}
