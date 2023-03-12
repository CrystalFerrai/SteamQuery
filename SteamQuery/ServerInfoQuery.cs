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
using System.IO;
using System.Text;

namespace SteamQuery
{
    /// <summary>
    /// Allows querying a live server for basic information
    /// </summary>
    public class ServerInfoQuery : ServerQuery<ServerInfoData>
    {
        private byte[] mChallengeRequest;

        /// <summary>
        /// If the query returned a challenge, we need to query again with response included
        /// </summary>
		protected override bool ShouldQueryAgain => mChallengeRequest != null;

		/// <summary>
		/// Creates a new instance of the ServerInfoQuery class
		/// </summary>
		/// <param name="queryAddress">The adddress of the server to query</param>
		/// <param name="queryTimeout">How long to wait for a query response, in milliseconds</param>
		public ServerInfoQuery(EndPointAddress queryAddress, double queryTimeout = DefaultQueryTimeout)
            : base(queryAddress, ServerQueryType.Info, false, queryTimeout)
        {
            mChallengeRequest = null;
        }

        /// <summary>
        /// Returns the byte that identifyies the type of this query to the server
        /// </summary>
        protected override byte GetQueryHeader()
        {
            return 0x54;
        }

        /// <summary>
        /// Returns the data to use as the packet content when sending this query
        /// </summary>
        protected override byte[] GetQueryDatagram()
        {
            const string query = "Source Engine Query";
            byte[] data;

            if (mChallengeRequest != null)
            {
                data = new byte[query.Length + 1 + mChallengeRequest.Length];
                Array.Copy(mChallengeRequest, 0, data, query.Length + 1, mChallengeRequest.Length);
                mChallengeRequest = null;
            }
            else
            {
                data = new byte[query.Length + 1];
            }

            Encoding.ASCII.GetBytes(query, 0, query.Length, data, 0);
            data[query.Length] = 0;
            return data;
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

            if (response.Length > 0 && response[0] == 0x41)
			{
                mChallengeRequest = new byte[response.Length - 1];
                Array.Copy(response, 1, mChallengeRequest, 0, response.Length - 1);
                return;
			}

            using (MemoryStream stream = new MemoryStream(response))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                byte header = reader.ReadByte();
                if (header != 0x49)
                {
                    FireQueryComplete(queryHandle, ServerQueryResult.UnknownResponseReceived, pingMs);
                    return;
                }

                try
                {
                    byte protocolVersion = reader.ReadByte();
                    string serverName = SerializationHelper.ReadString(stream, response);
                    string mapName = SerializationHelper.ReadString(stream, response);
                    string gameFolder = SerializationHelper.ReadString(stream, response);
                    string gameName = SerializationHelper.ReadString(stream, response);
                    ushort legacyAppId = reader.ReadUInt16();
                    byte playerCount = reader.ReadByte();
                    byte maxPlayers = reader.ReadByte();
                    byte botCount = reader.ReadByte();
                    char serverType = (char)reader.ReadByte();
                    char serverEnvironment = (char)reader.ReadByte();
                    byte serverVisibility = reader.ReadByte();
                    byte serverVac = reader.ReadByte();
                    string gameVersion = SerializationHelper.ReadString(stream, response);

                    ushort gamePort = 0;
                    ulong steamId = 0;
                    string serverKeywords = string.Empty;
                    ulong gameId = 0;
                    uint realAppId = 0;
                    bool sourceTvAvailable = false;
                    string sourceTvServerName = string.Empty;
                    ushort sourceTvPort = 0;
                    if (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        byte edf = reader.ReadByte();
                        if (CheckFlag(edf, 0x80))
                        {
                            gamePort = reader.ReadUInt16();
                        }
                        if (CheckFlag(edf, 0x10))
                        {
                            steamId = reader.ReadUInt64();
                        }
                        if (CheckFlag(edf, 0x40))
                        {
                            sourceTvAvailable = true;
                            sourceTvPort = reader.ReadUInt16();
                            sourceTvServerName = SerializationHelper.ReadString(stream, response);
                        }
                        if (CheckFlag(edf, 0x20))
                        {
                            serverKeywords = SerializationHelper.ReadString(stream, response);
                        }
                        if (CheckFlag(edf, 0x01))
                        {
                            gameId = reader.ReadUInt64();
                            realAppId = (uint)(gameId & 0xffffffu);
                        }
                    }

                    ServerType type = ServerType.Unknown;
                    switch (serverType)
                    {
                        case 'd':
                            type = ServerType.Dedicated;
                            break;
                        case 'l':
                            type = ServerType.Local;
                            break;
                    }

                    ServerEnvironment environment = ServerEnvironment.Unknown;
                    switch (serverEnvironment)
                    {
                        case 'l':
                            environment = ServerEnvironment.Linux;
                            break;
                        case 'w':
                            environment = ServerEnvironment.Windows;
                            break;
                        case 'm':
                        case 'o':
                            environment = ServerEnvironment.OSX;
                            break;
                    }

                    ServerVisibility visibility = ServerVisibility.Unknown;
                    switch (serverVisibility)
                    {
                        case 0:
                            visibility = ServerVisibility.Public;
                            break;
                        case 1:
                            visibility = ServerVisibility.Private;
                            break;
                    }


                    var queryResponseData = new ServerInfoData(
                        serverName,
                        gameVersion,
                        gameName,
                        realAppId,
                        legacyAppId,
                        gamePort,
                        mapName,
                        playerCount,
                        maxPlayers,
                        type,
                        environment,
                        visibility,
                        serverKeywords,
                        botCount,
                        serverVac == 1,
                        steamId,
                        gameId,
                        gameFolder,
                        sourceTvAvailable,
                        sourceTvServerName,
                        sourceTvPort,
                        protocolVersion);

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
