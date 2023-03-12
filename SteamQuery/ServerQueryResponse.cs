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

namespace SteamQuery
{
    /// <summary>
    /// Represents a reponse to a server query
    /// </summary>
    /// <typeparam name="T">The type of the data associated with the response</typeparam>
    public class ServerQueryResponse<T> : EventArgs
    {
        /// <summary>
        /// Gets the handle from the query
        /// </summary>
        public ServerQueryHandle QueryHandle { get; }

        /// <summary>
        /// Gets the result of the query
        /// </summary>
        public ServerQueryResult Result { get; }

        /// <summary>
        /// Gets the query response data, if the query succeeded
        /// </summary>
        public T Data { get; }

        /// <summary>
        /// Gets the server ping resposne time in milliseconds
        /// </summary>
        public float PingMs { get; }

        /// <summary>
        /// Creates a new instance of the ServerQueryResponse class
        /// </summary>
        internal ServerQueryResponse(ServerQueryHandle queryHandle, ServerQueryResult result, float pingMs, T data = default(T))
        {
            QueryHandle = queryHandle;
            Result = result;
            Data = data;
            PingMs = pingMs;
        }
    }

    /// <summary>
    /// Represents the result of a server query
    /// </summary>
    public enum ServerQueryResult
    {
        /// <summary>
        /// The server responded in the expected way. Response data should be valid.
        /// </summary>
        ResponseReceived,
        /// <summary>
        /// The server took too long to respond. No response data received.
        /// </summary>
        QueryTimedOut,
        /// <summary>
        /// The server responded in an unexpected way. Could not parse response data.
        /// </summary>
        UnknownResponseReceived
    }
}
