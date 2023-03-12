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

using System;
using System.Collections.Generic;
using System.IO;

namespace SteamQuery
{
    /// <summary>
    /// Allows querying a live server for a list of rules/settings/values
    /// </summary>
    public class ServerRulesQuery : ServerQuery<ServerRulesData>
    {
        /// <summary>
        /// Creates a new instance of the ServerRulesQuery class
        /// </summary>
        /// <param name="queryAddress">The adddress of the server to query</param>
        /// <param name="queryTimeout">How long to wait for a query response, in milliseconds</param>
        public ServerRulesQuery(EndPointAddress queryAddress, double queryTimeout = DefaultQueryTimeout)
            : base(queryAddress, ServerQueryType.Rules, true, queryTimeout)
        {
        }

        /// <summary>
        /// Returns the byte that identifyies the type of this query to the server
        /// </summary>
        protected override byte GetQueryHeader()
        {
            return 0x56;
        }

        /// <summary>
        /// Called when a response has been received for a pending query
        /// </summary>
        /// <param name="queryHandle">The handle to the query (originally returned from Send)</param>
        /// <param name="result">The result of the query</param>
        /// <param name="response">The response from the server</param>
        protected override void OnQueryComplete(ServerQueryHandle queryHandle, ServerQueryResult result, byte[] response, float pingMs)
        {
            if (result != ServerQueryResult.ResponseReceived)
            {
                FireQueryComplete(queryHandle, result, pingMs);
                return;
            }

            using (MemoryStream stream = new MemoryStream(response))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                byte header = reader.ReadByte();
                if (header != 0x45)
                {
                    FireQueryComplete(queryHandle, ServerQueryResult.UnknownResponseReceived, pingMs);
                    return;
                }

                try
                {
                    ushort numRules = reader.ReadUInt16();
                    Dictionary<string, string> rules = new Dictionary<string, string>(numRules);

                    string name, value;
                    for (int i = 0; i < numRules; ++i)
                    {
                        name = SerializationHelper.ReadString(stream, response);
                        value = SerializationHelper.ReadString(stream, response);
                        rules[name] = value;
                    }
                    
                    var queryResponseData = new ServerRulesData(rules);
                    FireQueryComplete(queryHandle, ServerQueryResult.ResponseReceived, pingMs, queryResponseData);
                }
                catch
                {
                    FireQueryComplete(queryHandle, ServerQueryResult.UnknownResponseReceived, pingMs);
                }
            }
        }
    }
}
