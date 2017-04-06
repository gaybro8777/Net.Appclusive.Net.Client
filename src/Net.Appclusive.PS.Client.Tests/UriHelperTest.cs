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

using System.Collections.Generic;
using biz.dfch.CS.Testing.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Net.Appclusive.PS.Client.Tests
{
    [TestClass]
    public class UriHelperTest
    {
        private const string APPCLUSIVE_API_BASE_URI = "https://appclusive/api/";
        private const string BASIC_LOGIN_SUFFIX = "/BasicLogin";

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "baseUri")]
        public void ConcatUriWithInvalidBaseUriThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.ConcatUri(null, BASIC_LOGIN_SUFFIX);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "baseUri")]
        public void ConcatUriWithEmptyBaseUriThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.ConcatUri(" ", BASIC_LOGIN_SUFFIX);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "uriSuffix")]
        public void ConcatUriWithNullUriSuffixThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.ConcatUri(APPCLUSIVE_API_BASE_URI, null);

            // Assert
        }

        [TestMethod]
        [ExpectContractFailure(MessagePattern = "uriSuffix")]
        public void ConcatUriWithEmptyUriSuffixThrowsContractException()
        {
            // Arrange

            // Act
            UriHelper.ConcatUri(APPCLUSIVE_API_BASE_URI, " ");

            // Assert
        }

        [TestMethod]
        public void ConcatUriReturnsValidUri()
        {
            // Arrange
            var expectedUri = "http://appclusive/api/BasicLogin";

            // Act

            // Assert
            Assert.AreEqual(expectedUri, UriHelper.ConcatUri("http://appclusive/api/", "BasicLogin"));
            Assert.AreEqual(expectedUri, UriHelper.ConcatUri("http://appclusive/api/", "/BasicLogin"));
            Assert.AreEqual(expectedUri, UriHelper.ConcatUri("http://appclusive/api/", "BasicLogin/"));
            Assert.AreEqual(expectedUri, UriHelper.ConcatUri("http://appclusive/api/", "/BasicLogin/"));
            Assert.AreEqual(expectedUri, UriHelper.ConcatUri("http://appclusive/api", "BasicLogin"));
            Assert.AreEqual(expectedUri, UriHelper.ConcatUri("http://appclusive/api", "/BasicLogin"));
            Assert.AreEqual(expectedUri, UriHelper.ConcatUri("http://appclusive/api", "BasicLogin/"));
            Assert.AreEqual(expectedUri, UriHelper.ConcatUri("http://appclusive/api", "/BasicLogin/"));
        }
    }
}
