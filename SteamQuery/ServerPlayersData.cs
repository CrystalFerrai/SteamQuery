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

namespace SteamQuery
{
    /// <summary>
    /// Represents data returned from a server players query
    /// </summary>
    public class ServerPlayersData
    {
        /// <summary>
        /// Gets the number of identified players connected to the server
        /// </summary>
        public int ActivePlayerCount { get; }

        /// <summary>
        /// Gets a count of players in an unknown state. In some games, these are players that are currently connecting.
        /// </summary>
        public int UnknownPlayerCount { get; }

        /// <summary>
        /// Gets the collection of players currently connected to the server
        /// </summary>
        public IReadOnlyCollection<ServerPlayerData> ActivePlayers { get; }

        /// <summary>
        /// Creates a new instance of the ServerPlayersData class
        /// </summary>
        internal ServerPlayersData(int unknownPlayerCount, IReadOnlyCollection<ServerPlayerData> activePlayers)
        {
            UnknownPlayerCount = unknownPlayerCount;
            ActivePlayerCount = activePlayers.Count;
            ActivePlayers = activePlayers;
        }

        /// <summary>
        /// Returns a brief overview of the data
        /// </summary>
        public override string ToString()
        {
            return $"{ActivePlayerCount} active, {UnknownPlayerCount} unknown";
        }
    }

    /// <summary>
    /// Represents a player connected to a server
    /// </summary>
    public class ServerPlayerData
    {
        /// <summary>
        /// Gets the name of the player
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the amount of time the player has been connected to the server
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Gets the player's score. In many games, this is always 0.
        /// </summary>
        public int Score { get; }

        /// <summary>
        /// Creates a new instance of the ServerPlayerData class
        /// </summary>
        internal ServerPlayerData(string name, TimeSpan duration, int score)
        {
            Name = name;
            Duration = duration;
            Score = score;
        }

        /// <summary>
        /// Returns a brief summary of the player
        /// </summary>
        public override string ToString()
        {
            return $"{Name} [Duration {Duration}]";
        }
    }
}
