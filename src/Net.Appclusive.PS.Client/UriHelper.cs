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

using System.Diagnostics.Contracts;

namespace Net.Appclusive.PS.Client
{
    public static class UriHelper
    {
        public const char CHARACTER_TO_TRIM_ON = '/';

        public static string ConcatUri(string baseUri, string uriSuffix)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(baseUri));
            Contract.Requires(!string.IsNullOrWhiteSpace(uriSuffix));

            return string.Concat(baseUri.TrimEnd(CHARACTER_TO_TRIM_ON), CHARACTER_TO_TRIM_ON, uriSuffix.TrimStart(CHARACTER_TO_TRIM_ON).TrimEnd(CHARACTER_TO_TRIM_ON));
        }
    }
}
