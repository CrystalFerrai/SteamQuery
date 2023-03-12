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
	/// Represents data returned from a server info query
	/// </summary>
	public class ServerInfoData
    {
        /// <summary>
        /// Gets the name of the server (session name)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the server build version
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Gets the name of the game the server is associated with
        /// </summary>
        public string GameName { get; }

        /// <summary>
        /// Gets the App ID of the game the server is associated with
        /// </summary>
        public uint GameId { get; }

        /// <summary>
        /// Gets the App ID of the game the server is associated with. (Many games return 0 here - used more by older games.)
        /// </summary>
        public ushort LegacyGameId { get; }

        /// <summary>
        /// Gets the port the server is bound to for incoming game client connections
        /// </summary>
        public ushort GamePort { get; }

        /// <summary>
        /// Gets the name of the map loaded on the server
        /// </summary>
        public string Map { get; }

        /// <summary>
        /// Gets the number of players currently connected to the server
        /// </summary>
        public int PlayerCount { get; }

        /// <summary>
        /// Gets the maximum number of connected players the server will allow
        /// </summary>
        public int MaxPlayers { get; }

        /// <summary>
        /// Gets the type of the server
        /// </summary>
        public ServerType Type { get; }

        /// <summary>
        /// Gets the environment the server is running in
        /// </summary>
        public ServerEnvironment Environment { get; }

        /// <summary>
        /// Gets the visibility of the server
        /// </summary>
        public ServerVisibility Visibility { get; }

        /// <summary>
        /// Gets key words returned by the server. This is usually miscellaneous game-specific data.
        /// </summary>
        public string KeyWords { get; }

        /// <summary>
        /// Gets the number of bots on the server
        /// </summary>
        public byte BotCount { get; }

        /// <summary>
        /// Gets whether Valve Anti-Cheat is enabled on the server
        /// </summary>
        public bool VacEnabled { get; }

        /// <summary>
        /// Gets the steam ID of the server
        /// </summary>
        public ulong SteamId { get; }

        /// <summary>
        /// Gets a game-specific game identifier for the game the server is associated with.
        /// </summary>
        /// <remarks>
        /// This is usually just the game's AppId (low 24 bits), but may contain additional game-specific information in the high 40 bits.
        /// </remarks>
        public ulong ExtendedGameId { get; }

        /// <summary>
        /// Gets the name of the folder containing the game files
        /// </summary>
        public string GameFolder { get; }

        /// <summary>
        /// Gets whether SourceTV is available on the server
        /// </summary>
        public bool SourceTvAvailable { get; }

        /// <summary>
        /// Gets the name of the SourceTV spectator server, if SourceTV is available
        /// </summary>
        public string SourceTvServerName { get; }

        /// <summary>
        /// Gets the SourceTV spectator server port number, if SourceTV is available
        /// </summary>
        public ushort SourceTvPort { get; }

        /// <summary>
        /// Gets the Steam query protocol version in use by the server
        /// </summary>
        public byte ProtocolVersion { get; }

        /// <summary>
        /// Creates a new instance of the ServerInfoData class
        /// </summary>
        internal ServerInfoData(
            string name,
            string version,
            string gameName,
            uint gameId,
            ushort legacyGameId,
            ushort gamePort,
            string map,
            int playerCount,
            int maxPlayers,
            ServerType type,
            ServerEnvironment environment,
            ServerVisibility visibility,
            string keyWords,
            byte botCount,
            bool vacEnabled,
            ulong steamId,
            ulong extendedGameId,
            string gameFolder,
            bool sourceTvAvailable,
            string sourceTvServerName,
            ushort sourceTvPort,
            byte protocolVersion)
        {
            Name = name;
            Version = version;
            GameName = gameName;
            GameId = gameId;
            LegacyGameId = legacyGameId;
            GamePort = gamePort;
            Map = map;
            PlayerCount = playerCount;
            MaxPlayers = maxPlayers;
            Type = type;
            Environment = environment;
            Visibility = visibility;
            KeyWords = keyWords;
            BotCount = botCount;
            VacEnabled = vacEnabled;
            SteamId = steamId;
            ExtendedGameId = extendedGameId;
            GameFolder = gameFolder;
            SourceTvAvailable = sourceTvAvailable;
            SourceTvServerName = sourceTvServerName;
            SourceTvPort = sourceTvPort;
            ProtocolVersion = protocolVersion;
        }

        /// <summary>
        /// Returns a brief summary of the server info
        /// </summary>
        public override string ToString()
        {
            return $"{Name} - (v{Version}, {Map}) Players: {PlayerCount}/{MaxPlayers}";
        }
    }

    /// <summary>
    /// The type of a server
    /// </summary>
    public enum ServerType
    {
        Unknown,
        /// <summary>
        /// A dedicated server
        /// </summary>
        Dedicated,
        /// <summary>
        /// A local / non-dedicated server
        /// </summary>
        Local
    }

    /// <summary>
    /// Gets the environment in which a server is running
    /// </summary>
    public enum ServerEnvironment
    {
        Unknown,
        /// <summary>
        /// Server is running in a Linux-based environment
        /// </summary>
        Linux,
        /// <summary>
        /// Server is running in a Windows-based environment
        /// </summary>
        Windows,
        /// <summary>
        /// Server is running in an OSX-based environment
        /// </summary>
        OSX
    }

    /// <summary>
    /// The visibility of a server
    /// </summary>
    public enum ServerVisibility
    {
        Unknown,
        /// <summary>
        /// Server is open to public and not password protected
        /// </summary>
        Public,
        /// <summary>
        /// Server can only be joined when the correct password is supplied
        /// </summary>
        Private
    }
}
