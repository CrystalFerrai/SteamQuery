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

namespace SteamQuery
{
    /// <summary>
    /// Identifies the type of a server query
    /// </summary>
    /// <remarks>
    /// https://developer.valvesoftware.com/wiki/Server_queries
    /// </remarks>
    public enum ServerQueryType
    {
        /// <summary>
        /// A query for basic server information
        /// </summary>
        Info,

        /// <summary>
        /// A query for connected players
        /// </summary>
        Players,

        /// <summary>
        /// A query for server ruels and settings
        /// </summary>
        Rules
    }
}
