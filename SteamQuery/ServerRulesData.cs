// Copyright 2023 Crystal Ferrai
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;

namespace SteamQuery
{
    /// <summary>
    /// Represents data returned from a server rules query
    /// </summary>
    public class ServerRulesData
    {
        /// <summary>
        /// Gets the mapping of rules/settings/values returned by the server, identified by name
        /// </summary>
        public IReadOnlyDictionary<string, string> Rules { get; }

        /// <summary>
        /// Creates a new instance of the ServerRulesData class
        /// </summary>
        internal ServerRulesData(IReadOnlyDictionary<string, string> rules)
        {
            Rules = rules;
        }
    }
}
