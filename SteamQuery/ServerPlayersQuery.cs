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
    /// Allows querying a live server for a list of currently connected players
    /// </summary>
    public class ServerPlayersQuery : ServerQuery<ServerPlayersData>
    {
        /// <summary>
        /// Creates a new instance of the ServerPlayersQuery class
        /// </summary>
        /// <param name="queryAddress">The adddress of the server to query</param>
        /// <param name="queryTimeout">How long to wait for a query response, in milliseconds</param>
        public ServerPlayersQuery(EndPointAddress queryAddress, double queryTimeout = DefaultQueryTimeout)
            : base(queryAddress, ServerQueryType.Players, true, queryTimeout)
        {
        }

        /// <summary>
        /// Returns the byte that identifyies the type of this query to the server
        /// </summary>
        protected override byte GetQueryHeader()
        {
            return 0x55;
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
                if (header != 0x44)
                {
                    FireQueryComplete(queryHandle, ServerQueryResult.UnknownResponseReceived, pingMs);
                    return;
                }

                try
                {
                    byte totalPlayerCount = reader.ReadByte();

                    List<ServerPlayerData> players = new List<ServerPlayerData>();

                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        byte index = reader.ReadByte(); // Always returns 0
                        string name = SerializationHelper.ReadString(stream, response);
                        int score = reader.ReadInt32();
                        float duration = reader.ReadSingle();

                        players.Add(new ServerPlayerData(name, TimeSpan.FromSeconds(duration), score));
                    }

                    
                    var queryResponseData = new ServerPlayersData(totalPlayerCount - players.Count, players);
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
