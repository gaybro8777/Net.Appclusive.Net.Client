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
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Net.Appclusive.Public.Domain.Identity;

namespace Net.Appclusive.Api.Tests
{
    [TestClass]
    public class AuthenticationIntegrationTest
    {
        private const string AUTHENTICATION_ENTITY_SET_NAME = "Authentications";

        private static readonly Uri _serviceRoot;

        static AuthenticationIntegrationTest()
        {
            var apiBaseUri = ConfigurationManager.AppSettings["Service.Reference.ServiceRoot"];
            _serviceRoot = new Uri(apiBaseUri + nameof(Core));
        }

        /// <summary>
        /// Prerequisites
        /// 
        /// - Basic authentication has to be activated in 'authenticationManagerConfiguration' of Web.config
        /// - Use the correct password for the actual user
        /// </summary>
        [TestMethod]
        public void BasicAuthenticationSucceeds()
        {
            // Arrange
            var svc = new Core.Core(_serviceRoot)
            {
                Credentials = new NetworkCredential()
                {
                    UserName = System.Environment.UserName,
                    Password = "P@ssw0rd"
                }
            };
            svc.Format.UseJson();

            Assert.IsNotNull(svc.InvokeEntitySetActionWithSingleResult<User>(AUTHENTICATION_ENTITY_SET_NAME, "BasicLogin", null));

            // Act
            var user = svc.Users.FirstOrDefault();

            // Assert
            Assert.IsNotNull(user);
        }

        /// <summary>
        /// Prerequisites
        /// 
        /// - Bearer authentication has to be activated in 'authenticationManagerConfiguration' of Web.config
        /// - Provide a valid JWT token
        /// </summary>
        [TestMethod]
        public void BearerAuthenticationSucceeds()
        {
            // Arrange
            var svc = new Core.Core(_serviceRoot)
            {
                Credentials = new NetworkCredential()
                {
                    UserName = DataServiceContextBase.AUTHORISATION_BAERER_USER_NAME,
                    Password = "JWT_TOKEN_HERE"
                }
            };
            svc.Format.UseJson();

            Assert.IsNotNull(svc.InvokeEntitySetActionWithSingleResult<User>(AUTHENTICATION_ENTITY_SET_NAME, "BearerLogin", null));

            // Act
            var user = svc.Users.FirstOrDefault();

            // Assert
            Assert.IsNotNull(user);
        }

        /// <summary>
        /// Prerequisites
        /// 
        /// - Negotiate authentication has to be activated in 'authenticationManagerConfiguration' of Web.config
        /// </summary>
        [TestMethod]
        public void NegotiateAuthenticationSucceeds()
        {
            // Arrange
            var svc = new Core.Core(_serviceRoot);
            svc.Format.UseJson();

            Assert.IsNotNull(svc.InvokeEntitySetActionWithSingleResult<User>(AUTHENTICATION_ENTITY_SET_NAME, "NegotiateLogin", null));

            // Act
            var user = svc.Users.FirstOrDefault();

            // Assert
            Assert.IsNotNull(user);
        }

        /// <summary>
        /// Prerequisites
        /// 
        /// - testAuthentication has to be activated in 'authenticationManagerConfiguration' of Web.config
        /// - Provide a valid test token
        /// </summary>
        [TestMethod]
        public void BearerAuthenticationWithTestTokenSucceeds()
        {
            // Arrange
            var svc = new Core.Core(_serviceRoot)
            {
                Credentials = new NetworkCredential()
                {
                    UserName = DataServiceContextBase.AUTHORISATION_BAERER_USER_NAME,
                    Password = "TOKEN2"
                }
            };
            svc.Format.UseJson();

            // Act
            var user = svc.Users.FirstOrDefault();

            // Assert
            Assert.IsNotNull(user);
        }
    }
}
