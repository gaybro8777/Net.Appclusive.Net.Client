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

using System.Net;
using biz.dfch.CS.Testing.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Net.Appclusive.Api.Constants;

namespace Net.Appclusive.Net.Client.Tests
{
    [TestClass]
    public class ApcClientTest
    {
        private const string API_BASE_URI = "http://appclusive/api";
        private const string OAUTH2_TOKEN = "ey8324ac79df78==";

        [TestMethod]
        [ExpectContractFailure(MessagePattern = @"Precondition.+IsNullOrWhiteSpace.+apiBaseUri")]
        public void InstantiateApcClientWithNullApiBaseUriThrowsContractException()
        {
            // Arrange

            // Act
            var apcClient = new ApcClient(null);


            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = @"Precondition.+IsNullOrWhiteSpace.+apiBaseUri")]
        public void InstantiateApcClientWithEmptyApiBaseUriThrowsContractException()
        {
            // Arrange

            // Act
            var apcClient = new ApcClient(" ");


            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = @"Precondition.+IsWellFormedUriString.+apiBaseUri")]
        public void InstantiateApcClientWithNonUriStringAsApiBaseUriThrowsContractException()
        {
            // Arrange

            // Act
            var apcClient = new ApcClient("arbitrary");

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = @"Precondition.+IsWellFormedUriString.+apiBaseUri")]
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

        [TestMethod]
        [ExpectContractFailure(MessagePattern = @"Precondition.+IsNullOrWhiteSpace.+oAuth2Token")]
        public void LoginWithNullOAuth2TokenThrowsContractException()
        {
            // Arrange
            var apcClient = new ApcClient(API_BASE_URI);

            // Act
            apcClient.Login(null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = @"Precondition.+IsNullOrWhiteSpace.+oAuth2Token")]
        public void LoginWithEmptyOAuth2TokenThrowsContractException()
        {
            // Arrange
            var apcClient = new ApcClient(API_BASE_URI);

            // Act
            apcClient.Login(" ");

            // Assert
        }

        [TestMethod]
        public void LoginWithValidOAuth2TokenSetsCredentialAndReturnsTrue()
        {
            // Arrange
            var apcClient = new ApcClient(API_BASE_URI);

            // Act
            var result = apcClient.Login(OAUTH2_TOKEN);

            // Assert

            Assert.IsTrue(result);
            Assert.IsNotNull(apcClient);
            Assert.IsNotNull(apcClient.Credentials);
            var credential = apcClient.Credentials as NetworkCredential;
            Assert.IsNotNull(credential);
            Assert.AreEqual(Authentication.AUTHORIZATION_BAERER_USER_NAME, credential.UserName);
        }
    }
}
