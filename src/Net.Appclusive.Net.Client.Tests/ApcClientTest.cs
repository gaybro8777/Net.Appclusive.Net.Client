/**
 * Copyright 2018 d-fens GmbH
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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Net.Appclusive.Net.Client.Tests
{
    [TestClass]
    public class ApcClientTest
    {
        private const string API_BASE_URI = "http://appclusive/api";

        [TestMethod]
        public void InstantiateApcClientWithNullApiBaseUriThrowsContractException()
        {
            // Arrange

            // Act
            var apcClient = new ApcClient(null);


            // Assert

        }

        [TestMethod]
        public void InstantiateApcClientWithEmptyApiBaseUriThrowsContractException()
        {
            // Arrange

            // Act
            var apcClient = new ApcClient("");


            // Assert

        }

        [TestMethod]
        public void InstantiateApcClientWithNonUriStringAsApiBaseUriThrowsContractException()
        {
            // Arrange

            // Act
            var apcClient = new ApcClient("arbitrary");

            // Assert

        }

        [TestMethod]
        public void InstantiateApcClientWithNonAbsoluteUriStringAsApiBaseUriThrowsContractException()
        {
            // Arrange

            // Act
            var apcClient = new ApcClient("/arbitrary");

            // Assert

        }

        [TestMethod]
        public void InstantiateApcClientWithValidApiBaseUriSucceedsAndSetsApiBaseUriPropertyAccordingly()
        {
            // Arrange

            // Act
            var apcClient = new ApcClient(API_BASE_URI);

            // Assert
            Assert.AreEqual(API_BASE_URI, apcClient.ApiBaseUri.AbsoluteUri);
        }
    }
}
