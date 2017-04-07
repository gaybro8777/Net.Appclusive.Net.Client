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
using System.Collections;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Net.Appclusive.Api.Constants;

namespace Net.Appclusive.Api
{
    public class DataServiceContextBase :
        DataServiceContext,
        IEditableDataServiceContext,
        IOdataActionExecutor
    {
        // Headers
        private const string USERAGENT_HEADER_NAME = "UserAgent";
        private const string CONTENT_TYPE_APPLICATION_XML = "application/xml";

        // General
        private const string PLURALISATION_SUFFIX = "s";
        private const string URI_DELIMITER = "/";
        private const char URI_DELIMITER_CHAR = '/';

        private static readonly string _httpPostMethod = nameof(HttpMethod.Post).ToUpper();

        private volatile string metadata;
        private readonly object syncRoot = new object();

        private string tenantHeaderName;
        public string TenantHeaderName
        {
            get
            {
                if (string.IsNullOrEmpty(tenantHeaderName))
                {
                    tenantHeaderName = Authentication.Header.DEFAULT_TENANT_HEADER_NAME;
                }
                return tenantHeaderName;
            }
            set
            {
                tenantHeaderName = value;
            }
        }

        private string tenantId;
        public string TenantId
        {
            get
            {
                return tenantId;
            }
            set
            {
                if (value != tenantId)
                {
                    tenantId = value;
                    RegisterSendingRequestEvent();
                }
            }
        }

        public new ICredentials Credentials
        {
            get
            {
                return base.Credentials;
            }
            set
            {
                if (base.Credentials != value)
                {
                    base.Credentials = value;
                    RegisterSendingRequestEvent();
                }
            }
        }

        static DataServiceContextBase()
        {
            // this is a runtime check to ensure we have not forgotten 
            // to change the base class of an auto-generated service reference
            // to DataServiceContextBase
            var assembly = typeof(DataServiceContextBase).Assembly;
            foreach (var definedType in assembly.DefinedTypes)
            {
                if (!(definedType.IsPublic || definedType.IsNested))
                {
                    continue;
                }

                if (definedType.IsInterface)
                {
                    continue;
                }

                if (definedType.BaseType != typeof(DataServiceContext))
                {
                    continue;
                }

                var dataServiceContextFullName = definedType.FullName;
                var definedTypeParts = dataServiceContextFullName.Split('.');
                Contract.Assert(2 <= definedTypeParts.Length);
                Contract.Assert(definedTypeParts[definedTypeParts.Length - 1] != definedTypeParts[definedTypeParts.Length - 2], $"'{dataServiceContextFullName}' must derive from DataServiceContextBase");
            }
        }

        #region Constructors from DataServiceContext

        // Summary:
        //     Initializes a new instance of the System.Data.Services.Client.DataServiceContext
        //     class.
        //
        // Remarks:
        //     It is expected that the BaseUri or ResolveEntitySet properties will be set
        //     before using the newly created context.
        public DataServiceContextBase()
        {
            // N/A
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Data.Services.Client.DataServiceContext
        //     class with the specified serviceRoot.
        //
        // Parameters:
        //   serviceRoot:
        //     An absolute URI that identifies the root of a data service.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     When the serviceRoot is null.
        //
        //   System.ArgumentException:
        //     If the serviceRoot is not an absolute URI -or-If the serviceRoot is a well
        //     formed URI without a query or query fragment.
        //
        // Remarks:
        //     The library expects the Uri to point to the root of a data service, but does
        //     not issue a request to validate it does indeed identify the root of a service.
        //      If the Uri does not identify the root of the service, the behavior of the
        //     client library is undefined. A Uri provided with a trailing slash is equivalent
        //     to one without such a trailing character.  With Silverlight, the serviceRoot
        //     can be a relative Uri that will be combined with System.Windows.Browser.HtmlPage.Document.DocumentUri.
        public DataServiceContextBase(Uri serviceRoot)
            : base(serviceRoot)
        {
            // N/A
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Data.Services.Client.DataServiceContext
        //     class with the specified serviceRoot and targeting the specific maxProtocolVersion.
        //
        // Parameters:
        //   serviceRoot:
        //     An absolute URI that identifies the root of a data service.
        //
        //   maxProtocolVersion:
        //     A System.Data.Services.Common.DataServiceProtocolVersion value that is the
        //     maximum protocol version that the client understands.
        //
        // Remarks:
        //     The library expects the Uri to point to the root of a data service, but does
        //     not issue a request to validate it does indeed identify the root of a service.
        //      If the Uri does not identify the root of the service, the behavior of the
        //     client library is undefined. A Uri provided with a trailing slash is equivalent
        //     to one without such a trailing character.  With Silverlight, the serviceRoot
        //     can be a relative Uri that will be combined with System.Windows.Browser.HtmlPage.Document.DocumentUri.
        public DataServiceContextBase(Uri serviceRoot, DataServiceProtocolVersion maxProtocolVersion)
            : base(serviceRoot, maxProtocolVersion)
        {
            // N/A
        }

        #endregion Constructors from DataServiceContext

        public static Version GetVersion()
        {
            Contract.Ensures(null != Contract.Result<Version>());

            var assembly = typeof(DataServiceContextBase).Assembly;
            var assemblyName = assembly.GetName();
            return assemblyName.Version;
        }

        public string GetMetadata()
        {
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            if (null != metadata)
            {
                return metadata;
            }

            lock (syncRoot)
            {
                if (null != metadata)
                {
                    return metadata;
                }

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add(USERAGENT_HEADER_NAME, GetType().FullName);

                    var authorizationHeaderValue = BuildAuthorizationHeaderValue();
                    if (default(string) != authorizationHeaderValue)
                    {
                        httpClient.DefaultRequestHeaders.Add(Authentication.Header.AUTHORIZATION, authorizationHeaderValue);
                    }

                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(CONTENT_TYPE_APPLICATION_XML));

                    if (IsTenantSpecified())
                    {
                        httpClient.DefaultRequestHeaders.Add(TenantHeaderName, TenantId);
                    }

                    var response = httpClient.GetAsync(GetMetadataUri().AbsoluteUri).Result;
                    response.EnsureSuccessStatusCode();

                    metadata = response.Content.ReadAsStringAsync().Result;
                    return metadata;
                }
            }
        }

        #region IEditableDataServiceContext

        public void AttachIfNeeded(object entity)
        {
            var entitySetName = string.Concat(entity.GetType().Name, PLURALISATION_SUFFIX);

            AttachIfNeededPrivate(entitySetName, entity);
        }

        public void AttachIfNeeded(string entitySetName, object entity)
        {
            AttachIfNeededPrivate(entitySetName, entity);
        }

        private void AttachIfNeededPrivate(string entitySetName, object entity)
        {
            try
            {
                AttachTo(entitySetName, entity);
            }
            catch (InvalidOperationException ex)
            {
                if (!ex.Message.Equals("The context is already tracking the entity."))
                {
                    throw;
                }
            }
        }

        public bool HasPendingEntityChanges()
        {
            var hasChanges = Entities.Any(e => e.State != EntityStates.Unchanged);
            return hasChanges;
        }

        public bool HasPendingLinkChanges()
        {
            var hasChanges = Links.Any(e => e.State != EntityStates.Unchanged);
            return hasChanges;
        }

        public bool HasPendingChanges()
        {
            return HasPendingEntityChanges() || HasPendingLinkChanges();
        }

        public void RevertEntityState(object entity)
        {
            var entityDescriptor = GetEntityDescriptor(entity);
            if (EntityStates.Added == entityDescriptor.State || EntityStates.Deleted == entityDescriptor.State)
            {
                ChangeState(entity, EntityStates.Detached);
            }
            else
            {
                ChangeState(entity, EntityStates.Unchanged);
            }
        }

        #endregion IEditableDataServiceContext

        #region IOdataActionExecutor

        public void InvokeEntitySetActionWithVoidResult(object entity, string actionName, object inputParameters)
        {
            var entitySetName = string.Concat(entity.GetType().Name, PLURALISATION_SUFFIX);
            InvokeEntitySetActionWithVoidResult(entitySetName, actionName, inputParameters);
        }

        public void InvokeEntitySetActionWithVoidResult(string entitySetName, string actionName, object inputParameters)
        {
            var uriAction = new Uri(string.Concat(BaseUri.AbsoluteUri.TrimEnd(URI_DELIMITER_CHAR), URI_DELIMITER, entitySetName, URI_DELIMITER, actionName));

            BodyOperationParameter[] bodyParameters;
            if (inputParameters is Hashtable)
            {
                bodyParameters = GetBodyOperationParametersFromHashtable((Hashtable) inputParameters);
            }
            else if (inputParameters is Dictionary<string, object>)
            {
                bodyParameters = GetBodyOperationParametersFromDictionary((Dictionary<string, object>) inputParameters);
            }
            else
            {
                bodyParameters = GetBodyOperationParametersFromObject(inputParameters);
            }
            Execute(uriAction, _httpPostMethod, bodyParameters);
        }

        public object InvokeEntitySetActionWithSingleResult(string entitySetName, string actionName, Type type, object inputParameters)
        {
            var methodInfo = GetCallerType().GetMethods().First(m => m.Name == nameof(InvokeEntitySetActionWithSingleResult) && m.IsGenericMethod && m.GetParameters()[0].Name == "entitySetName");
            Contract.Assert(null != methodInfo, "No generic method type found.");
            var genericMethod = methodInfo.MakeGenericMethod(type);
            Contract.Assert(null != genericMethod, "Cannot create generic method.");

            var result = genericMethod.Invoke(this, new[] { entitySetName, actionName, inputParameters });
            return result;
        }

        public object InvokeEntitySetActionWithSingleResult(object entity, string actionName, Type type, object inputParameters)
        {
            var methodInfo = GetCallerType().GetMethods().First(m => m.Name == nameof(InvokeEntitySetActionWithSingleResult) && m.IsGenericMethod && m.GetParameters()[0].Name == "entity");
            Contract.Assert(null != methodInfo, "No generic method type found.");
            var genericMethod = methodInfo.MakeGenericMethod(type);
            Contract.Assert(null != genericMethod, "Cannot create generic method.");

            var result = genericMethod.Invoke(this, new[] { entity, actionName, inputParameters });
            return result;
        }

        public object InvokeEntitySetActionWithSingleResult(string entitySetName, string actionName, object type, object inputParameters)
        {
            var methodInfo = GetCallerType().GetMethods().First(m => m.Name == nameof(InvokeEntitySetActionWithSingleResult) && m.IsGenericMethod && m.GetParameters()[0].Name == "entitySetName");
            Contract.Assert(null != methodInfo, "No generic method type found.");
            var genericMethod = methodInfo.MakeGenericMethod(type.GetType());
            Contract.Assert(null != genericMethod, "Cannot create generic method.");

            var result = genericMethod.Invoke(this, new[] { entitySetName, actionName, inputParameters });
            return result;
        }

        public object InvokeEntitySetActionWithSingleResult(object entity, string actionName, object type, object inputParameters)
        {
            var methodInfo = GetCallerType().GetMethods().First(m => m.Name == nameof(InvokeEntitySetActionWithSingleResult) && m.IsGenericMethod && m.GetParameters()[0].Name == "entity");
            Contract.Assert(null != methodInfo, "No generic method type found.");
            var genericMethod = methodInfo.MakeGenericMethod(type.GetType());
            Contract.Assert(null != genericMethod, "Cannot create generic method.");

            var result = genericMethod.Invoke(this, new[] { entity, actionName, inputParameters });
            return result;
        }

        public T InvokeEntitySetActionWithSingleResult<T>(string entitySetName, string actionName, object inputParameters)
        {
            var uriAction = new Uri(string.Concat(BaseUri.AbsoluteUri.TrimEnd(URI_DELIMITER_CHAR), URI_DELIMITER, entitySetName, URI_DELIMITER, actionName));

            BodyOperationParameter[] bodyParameters;
            if (inputParameters is Hashtable)
            {
                bodyParameters = GetBodyOperationParametersFromHashtable((Hashtable) inputParameters);
            }
            else if (inputParameters is Dictionary<string, object>)
            {
                bodyParameters = GetBodyOperationParametersFromDictionary((Dictionary<string, object>) inputParameters);
            }
            else
            {
                bodyParameters = GetBodyOperationParametersFromObject(inputParameters);
            }

            var result = Execute<T>(uriAction, _httpPostMethod, true, bodyParameters).Single();
            return result;
        }

        public T InvokeEntitySetActionWithSingleResult<T>(object entity, string actionName, object inputParameters)
        {
            var entitySetName = string.Concat(entity.GetType().Name, PLURALISATION_SUFFIX);

            var result = InvokeEntitySetActionWithSingleResult<T>(entitySetName, actionName, inputParameters);
            return result;
        }

        public object InvokeEntitySetActionWithListResult(string entitySetName, string actionName, Type type, object inputParameters)
        {
            var methodInfo = GetCallerType().GetMethods().First(m => m.Name == nameof(InvokeEntitySetActionWithListResult) && m.IsGenericMethod && m.GetParameters()[0].Name == "entitySetName");
            Contract.Assert(null != methodInfo, "No generic method type found.");
            var genericMethod = methodInfo.MakeGenericMethod(type);
            Contract.Assert(null != genericMethod, "Cannot create generic method.");

            var result = genericMethod.Invoke(this, new[] { entitySetName, actionName, inputParameters });
            return result;
        }

        public object InvokeEntitySetActionWithListResult(object entity, string actionName, Type type, object inputParameters)
        {
            var methodInfo = GetCallerType().GetMethods().First(m => m.Name == nameof(InvokeEntitySetActionWithListResult) && m.IsGenericMethod && m.GetParameters()[0].Name == "entity");
            Contract.Assert(null != methodInfo, "No generic method type found.");
            var genericMethod = methodInfo.MakeGenericMethod(type);
            Contract.Assert(null != genericMethod, "Cannot create generic method.");

            var result = genericMethod.Invoke(this, new[] { entity, actionName, inputParameters });
            return result;
        }

        public object InvokeEntitySetActionWithListResult(string entitySetName, string actionName, object type, object inputParameters)
        {
            var methodInfo = GetCallerType().GetMethods().First(m => m.Name == nameof(InvokeEntitySetActionWithListResult) && m.IsGenericMethod && m.GetParameters()[0].Name == "entitySetName");
            Contract.Assert(null != methodInfo, "No generic method type found.");
            var genericMethod = methodInfo.MakeGenericMethod(type.GetType());
            Contract.Assert(null != genericMethod, "Cannot create generic method.");

            var result = genericMethod.Invoke(this, new[] { entitySetName, actionName, inputParameters });
            return result;
        }

        public object InvokeEntitySetActionWithListResult(object entity, string actionName, object type, object inputParameters)
        {
            var methodInfo = GetCallerType().GetMethods().First(m => m.Name == nameof(InvokeEntitySetActionWithListResult) && m.IsGenericMethod && m.GetParameters()[0].Name == "entity");
            Contract.Assert(null != methodInfo, "No generic method type found.");
            var genericMethod = methodInfo.MakeGenericMethod(type.GetType());
            Contract.Assert(null != genericMethod, "Cannot create generic method.");

            var result = genericMethod.Invoke(this, new[] { entity, actionName, inputParameters });
            return result;
        }

        public IEnumerable<T> InvokeEntitySetActionWithListResult<T>(string entitySetName, string actionName, object inputParameters)
        {
            var uriAction = new Uri(string.Concat(BaseUri.AbsoluteUri.TrimEnd(URI_DELIMITER_CHAR), URI_DELIMITER, entitySetName, URI_DELIMITER, actionName));

            BodyOperationParameter[] bodyParameters;
            if (inputParameters is Hashtable)
            {
                bodyParameters = GetBodyOperationParametersFromHashtable((Hashtable) inputParameters);
            }
            else if (inputParameters is Dictionary<string, object>)
            {
                bodyParameters = GetBodyOperationParametersFromDictionary((Dictionary<string, object>) inputParameters);
            }
            else
            {
                bodyParameters = GetBodyOperationParametersFromObject(inputParameters);
            }

            return Execute<T>(uriAction, _httpPostMethod, false, bodyParameters).ToList();
        }

        public IEnumerable<T> InvokeEntitySetActionWithListResult<T>(object entity, string actionName, object inputParameters)
        {
            var entitySetName = string.Concat(entity.GetType().Name, PLURALISATION_SUFFIX);

            var result = InvokeEntitySetActionWithListResult<T>(entitySetName, actionName, inputParameters);
            return result;
        }

        public void InvokeEntityActionWithVoidResult(object entity, string actionName, object inputParameters)
        {
            var entitySetName = string.Concat(entity.GetType().Name, PLURALISATION_SUFFIX);
            dynamic dynamicEntity = entity;
            var id = dynamicEntity.Id;

            InvokeEntityActionWithVoidResult(entitySetName, id, actionName, inputParameters);
        }

        public void InvokeEntityActionWithVoidResult(string entitySetName, long id, string actionName, object inputParameters)
        {
            InvokeEntityActionWithVoidResult(entitySetName, (object)id, actionName, inputParameters);
        }

        public void InvokeEntityActionWithVoidResult(string entitySetName, object id, string actionName, object inputParameters)
        {
            var entityUrl = GetEntityUrl(entitySetName, id);
            var uriAction = new Uri(string.Concat(BaseUri.AbsoluteUri.TrimEnd(URI_DELIMITER_CHAR), URI_DELIMITER, entityUrl, URI_DELIMITER, actionName));

            BodyOperationParameter[] bodyParameters;
            if (inputParameters is Hashtable)
            {
                bodyParameters = GetBodyOperationParametersFromHashtable((Hashtable) inputParameters);
            }
            else if (inputParameters is Dictionary<string, object>)
            {
                bodyParameters = GetBodyOperationParametersFromDictionary((Dictionary<string, object>) inputParameters);
            }
            else
            {
                bodyParameters = GetBodyOperationParametersFromObject(inputParameters);
            }
            Execute(uriAction, _httpPostMethod, bodyParameters);
        }

        public object InvokeEntityActionWithSingleResult(string entitySetName, long id, string actionName, Type type, object inputParameters)
        {
            var methodInfo = GetCallerType().GetMethods().First(m => m.Name == nameof(InvokeEntityActionWithSingleResult) && m.IsGenericMethod && m.GetParameters()[0].Name == "entitySetName");
            Contract.Assert(null != methodInfo, "No generic method type found.");
            var genericMethod = methodInfo.MakeGenericMethod(type);
            Contract.Assert(null != genericMethod, "Cannot create generic method.");

            var result = genericMethod.Invoke(this, new[] { entitySetName, id, actionName, inputParameters });
            return result;
        }

        public object InvokeEntityActionWithSingleResult(object entity, string actionName, Type type, object inputParameters)
        {
            var methodInfo = GetCallerType().GetMethods().First(m => m.Name == nameof(InvokeEntityActionWithSingleResult) && m.IsGenericMethod && m.GetParameters()[0].Name == "entity");
            Contract.Assert(null != methodInfo, "No generic method type found.");
            var genericMethod = methodInfo.MakeGenericMethod(type);
            Contract.Assert(null != genericMethod, "Cannot create generic method.");

            var result = genericMethod.Invoke(this, new[] { entity, actionName, inputParameters });
            return result;
        }

        public object InvokeEntityActionWithSingleResult(string entitySetName, long id, string actionName, object type, object inputParameters)
        {
            var methodInfo = GetCallerType().GetMethods().First(m => m.Name == nameof(InvokeEntityActionWithSingleResult) && m.IsGenericMethod && m.GetParameters()[0].Name == "entitySetName");
            Contract.Assert(null != methodInfo, "No generic method type found.");
            var genericMethod = methodInfo.MakeGenericMethod(type.GetType());
            Contract.Assert(null != genericMethod, "Cannot create generic method.");

            var result = genericMethod.Invoke(this, new[] { entitySetName, id, actionName, inputParameters });
            return result;
        }

        public object InvokeEntityActionWithSingleResult(object entity, string actionName, object type, object inputParameters)
        {
            var methodInfo = GetCallerType().GetMethods().First(m => m.Name == nameof(InvokeEntityActionWithSingleResult) && m.IsGenericMethod && m.GetParameters()[0].Name == "entity");
            Contract.Assert(null != methodInfo, "No generic method type found.");
            var genericMethod = methodInfo.MakeGenericMethod(type.GetType());
            Contract.Assert(null != genericMethod, "Cannot create generic method.");

            var result = genericMethod.Invoke(this, new[] { entity, actionName, inputParameters });
            return result;
        }

        public T InvokeEntityActionWithSingleResult<T>(string entitySetName, long id, string actionName, object inputParameters)
        {
            return InvokeEntityActionWithSingleResult<T>(entitySetName, (object)id, actionName, inputParameters);
        }

        public T InvokeEntityActionWithSingleResult<T>(string entitySetName, object id, string actionName, object inputParameters)
        {
            var entityUrl = GetEntityUrl(entitySetName, id);
            var uriAction = new Uri(string.Concat(BaseUri.AbsoluteUri.TrimEnd(URI_DELIMITER_CHAR), URI_DELIMITER, entityUrl, URI_DELIMITER, actionName));

            BodyOperationParameter[] bodyParameters;
            if (inputParameters is Hashtable)
            {
                bodyParameters = GetBodyOperationParametersFromHashtable((Hashtable) inputParameters);
            }
            else if (inputParameters is Dictionary<string, object>)
            {
                bodyParameters = GetBodyOperationParametersFromDictionary((Dictionary<string, object>) inputParameters);
            }
            else
            {
                bodyParameters = GetBodyOperationParametersFromObject(inputParameters);
            }

            var result = Execute<T>(uriAction, _httpPostMethod, true, bodyParameters).Single();
            return result;
        }

        public T InvokeEntityActionWithSingleResult<T>(object entity, string actionName, object inputParameters)
        {
            var entitySetName = string.Concat(entity.GetType().Name, PLURALISATION_SUFFIX);

            dynamic dynamicEntity = entity;
            var id = dynamicEntity.Id;

            var result = InvokeEntityActionWithSingleResult<T>(entitySetName, id, actionName, inputParameters);
            return result;
        }

        public object InvokeEntityActionWithListResult(string entitySetName, long id, string actionName, Type type, object inputParameters)
        {
            var methodInfo = GetCallerType().GetMethods().First(m => m.Name == nameof(InvokeEntityActionWithListResult) && m.IsGenericMethod && m.GetParameters()[0].Name == "entitySetName");
            Contract.Assert(null != methodInfo, "No generic method type found.");
            var genericMethod = methodInfo.MakeGenericMethod(type);
            Contract.Assert(null != genericMethod, "Cannot create generic method.");

            var result = genericMethod.Invoke(this, new[] { entitySetName, id, actionName, inputParameters });
            return result;
        }

        public object InvokeEntityActionWithListResult(object entity, string actionName, Type type, object inputParameters)
        {
            var methodInfo = GetCallerType().GetMethods().First(m => m.Name == nameof(InvokeEntityActionWithListResult) && m.IsGenericMethod && m.GetParameters()[0].Name == "entity");
            Contract.Assert(null != methodInfo, "No generic method type found.");
            var genericMethod = methodInfo.MakeGenericMethod(type);
            Contract.Assert(null != genericMethod, "Cannot create generic method.");

            var result = genericMethod.Invoke(this, new[] { entity, actionName, inputParameters });
            return result;
        }

        public object InvokeEntityActionWithListResult(string entitySetName, long id, string actionName, object type, object inputParameters)
        {
            var methodInfo = GetCallerType().GetMethods().First(m => m.Name == nameof(InvokeEntityActionWithListResult) && m.IsGenericMethod && m.GetParameters()[0].Name == "entitySetName");
            Contract.Assert(null != methodInfo, "No generic method type found.");
            var genericMethod = methodInfo.MakeGenericMethod(type.GetType());
            Contract.Assert(null != genericMethod, "Cannot create generic method.");

            var result = genericMethod.Invoke(this, new[] { entitySetName, id, actionName, inputParameters });
            return result;
        }

        public object InvokeEntityActionWithListResult(object entity, string actionName, object type, object inputParameters)
        {
            var methodInfo = GetCallerType().GetMethods().First(m => m.Name == nameof(InvokeEntityActionWithListResult) && m.IsGenericMethod && m.GetParameters()[0].Name == "entity");
            Contract.Assert(null != methodInfo, "No generic method type found.");
            var genericMethod = methodInfo.MakeGenericMethod(type.GetType());
            Contract.Assert(null != genericMethod, "Cannot create generic method.");

            var result = genericMethod.Invoke(this, new[] { entity, actionName, inputParameters });
            return result;
        }

        public IEnumerable<T> InvokeEntityActionWithListResult<T>(string entitySetName, long id, string actionName, object inputParameters)
        {
            return InvokeEntityActionWithListResult<T>(entitySetName, (object)id, actionName, inputParameters);
        }

        public IEnumerable<T> InvokeEntityActionWithListResult<T>(string entitySetName, object id, string actionName, object inputParameters)
        {
            var entityUrl = GetEntityUrl(entitySetName, id);
            var uriAction = new Uri(string.Concat(BaseUri.AbsoluteUri.TrimEnd(URI_DELIMITER_CHAR), URI_DELIMITER, entityUrl, URI_DELIMITER, actionName));

            BodyOperationParameter[] bodyParameters;
            if (inputParameters is Hashtable)
            {
                bodyParameters = GetBodyOperationParametersFromHashtable((Hashtable) inputParameters);
            }
            else if (inputParameters is Dictionary<string, object>)
            {
                bodyParameters = GetBodyOperationParametersFromDictionary((Dictionary<string, object>) inputParameters);
            }
            else
            {
                bodyParameters = GetBodyOperationParametersFromObject(inputParameters);
            }

            return Execute<T>(uriAction, _httpPostMethod, false, bodyParameters).ToList();
        }

        public IEnumerable<T> InvokeEntityActionWithListResult<T>(object entity, string actionName, object inputParameters)
        {
            var entitySetName = string.Concat(entity.GetType().Name, PLURALISATION_SUFFIX);

            dynamic dynamicEntity = entity;
            var id = dynamicEntity.Id;

            var result = InvokeEntityActionWithListResult<T>(entitySetName, id, actionName, inputParameters);
            return result;
        }

        private string GetEntityUrl(string entitySetName, object id)
        {
            string entityUrl;
            if (id is long)
            {
                entityUrl = $"{entitySetName}({id}L)";
            }
            else if (id is Guid)
            {
                entityUrl = $"{entitySetName}(guid'{id}')";
            }
            else
            {
                throw new Exception($"Id type '{id.GetType()}' not supported");
            }
            return entityUrl;
        }

        private static Type GetCallerType()
        {
            // this is an internal method that will retrieve the caller type of the caller 
            var frame = new StackFrame(1);
            var method = frame.GetMethod();

            var type = method.DeclaringType;
            return type;
        }

        public BodyOperationParameter[] GetBodyOperationParametersFromObject(object input)
        {
            var operationParameters = new List<BodyOperationParameter>();
            if (null == input)
            {
                return operationParameters.ToArray();
            }

            const BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.Instance;

            var properties = input.GetType().GetProperties(BINDING_FLAGS);
            foreach (var property in properties)
            {
                var operationParameter = new BodyOperationParameter(property.Name, property.GetValue(input));
                operationParameters.Add(operationParameter);
            }
            var fields = input.GetType().GetFields(BINDING_FLAGS);
            foreach (var field in fields)
            {
                var operationParameter = new BodyOperationParameter(field.Name, field.GetValue(input));
                operationParameters.Add(operationParameter);
            }
            return operationParameters.ToArray();
        }

        public BodyOperationParameter[] GetBodyOperationParametersFromHashtable(Hashtable input)
        {
            var operationParameters = new List<BodyOperationParameter>();
            if (null == input)
            {
                return operationParameters.ToArray();
            }

            foreach (DictionaryEntry entry in input)
            {
                var operationParameter = new BodyOperationParameter(entry.Key.ToString(), entry.Value);
                operationParameters.Add(operationParameter);
            }
            return operationParameters.ToArray();
        }

        public BodyOperationParameter[] GetBodyOperationParametersFromDictionary(Dictionary<string, object> input)
        {
            var operationParameters = new List<BodyOperationParameter>();
            if (null == input)
            {
                return operationParameters.ToArray();
            }

            foreach (var entry in input)
            {
                var operationParameter = new BodyOperationParameter(entry.Key, entry.Value);
                operationParameters.Add(operationParameter);
            }
            return operationParameters.ToArray();
        }

        public object GetSingleEntity(Type type, long id)
        {
            Contract.Requires(null != type);
            Contract.Requires(0 < id);

            var methodInfo = GetCallerType().GetMethods().First(m => m.Name == nameof(DataServiceContext.Execute) && m.IsGenericMethod && m.GetParameters()[0].Name == "requestUri");

            var genericMethod = methodInfo.MakeGenericMethod(type);
            Contract.Assert(null != genericMethod, "Cannot create generic method.");

            var uri = new Uri(string.Concat(BaseUri.AbsoluteUri.TrimEnd(URI_DELIMITER_CHAR), URI_DELIMITER, type.Name, PLURALISATION_SUFFIX, "(", id, "L)"));

            var result = genericMethod.Invoke(this, new object[] { uri });
            return result;
        }

        #endregion IOdataActionExecutor

        private bool IsBasicAuthentication()
        {
            if (Credentials is NetworkCredential)
            {
                var networkCredentials = (NetworkCredential)Credentials;

                if (!string.IsNullOrEmpty(networkCredentials.UserName) && !string.IsNullOrEmpty(networkCredentials.Password))
                {
                     return Authentication.AUTHORIZATION_BAERER_USER_NAME != networkCredentials.UserName;
                }
            }
            return false;
        }

        private bool IsBearerAuthentication()
        {
            if (Credentials is NetworkCredential)
            {
                var networkCredentials = (NetworkCredential)Credentials;
                if (!string.IsNullOrEmpty(networkCredentials.UserName) && !string.IsNullOrEmpty(networkCredentials.Password))
                {
                    return Authentication.AUTHORIZATION_BAERER_USER_NAME == networkCredentials.UserName;
                }
            }
            return false;
        }

        private bool IsTenantSpecified()
        {
            return !string.IsNullOrEmpty(TenantId);
        }

        private void RegisterSendingRequestEvent()
        {
            if (IsBearerAuthentication() || IsBasicAuthentication() || IsTenantSpecified())
            {
                SendingRequest2 += DataServiceContext_SendingRequest2;
            }
            else
            {
                SendingRequest2 -= DataServiceContext_SendingRequest2;
            }
        }

        public void DataServiceContext_SendingRequest2(object sender, SendingRequest2EventArgs e)
        {
            var authorisationHeaderValue = BuildAuthorizationHeaderValue();
            if (default(string) != authorisationHeaderValue)
            {
                e.RequestMessage.SetHeader(Authentication.Header.AUTHORIZATION, authorisationHeaderValue);
            }

            if (IsTenantSpecified())
            {
                e.RequestMessage.SetHeader(TenantHeaderName, TenantId);
            }
        }

        private string BuildAuthorizationHeaderValue()
        {
            var result = default(string);

            if (IsBearerAuthentication())
            {
                var networkCredentials = (NetworkCredential)Credentials;
                result = string.Format(Authentication.AUTHORIZATION_BEARER_TEMPLATE, networkCredentials.Password);
            }

            if (IsBasicAuthentication())
            {
                var networkCredentials = (NetworkCredential)Credentials;
                var basicAuthString = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{networkCredentials.UserName}:{networkCredentials.Password}"));
                result = string.Format(Authentication.AUTHORIZATION_BASIC_TEMPLATE, basicAuthString);
            }

            return result;
        }
    }
}
