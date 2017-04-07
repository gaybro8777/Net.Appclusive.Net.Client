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
using System.Data.Services.Client;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Dynamic;
using Net.Appclusive.Api.Constants;
using Net.Appclusive.Public.Domain.Identity;

namespace Net.Appclusive.Api
{
    public static class DataServiceQueryExtensions
    {
        public static DataServiceQuery<T> Filter<T>(this DataServiceQuery<T> dataServiceQuery, object filterValue)
        {
            Contract.Requires(null != filterValue);

            return dataServiceQuery.AddQueryOption(DataService.QueryOption.FILTER, filterValue);
        }

        public static T Id<T>(this DataServiceQuery<T> dataServiceQuery, long id)
        {
            Contract.Requires(0 < id);

            return dataServiceQuery.Where("Id == @0", id).FirstOrDefault();
        }

        public static Tenant Id(this DataServiceQuery<Tenant> dataServiceQuery, Guid id)
        {
            Contract.Requires(default(Guid) != id);

            return dataServiceQuery.FirstOrDefault(entity => entity.Id == id);
        }
    }
}
