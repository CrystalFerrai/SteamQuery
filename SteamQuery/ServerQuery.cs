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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace SteamQuery
{
    /// <summary>
    /// Base class for server query implementations
    /// </summary>
    /// <remarks>
    /// https://developer.valvesoftware.com/wiki/Server_queries
    /// </remarks>
    public abstract class ServerQuery
    {
        /// <summary>
        /// Default value for QueryTimeout
        /// </summary>
        protected const double DefaultQueryTimeout = 15000.0;

        /// <summary>
        /// Whether a challange id needs to be requested fromt he server for this query
        /// </summary>
        private readonly bool mChallengeRequired;
        
        /// <summary>
        /// Gets the address of the server associated with this query
        /// </summary>
        public EndPointAddress QueryAddress { get; }

        /// <summary>
        /// Gets the type of query represented by this instance
        /// </summary>
        public ServerQueryType QueryType { get; }

        /// <summary>
        /// Gets how long to wait for a query response from the server, in milliseconds
        /// </summary>
        public double QueryTimeout { get; }

        /// <summary>
        /// Return true after a query response if a query should be sent again using updated data
        /// </summary>
        protected virtual bool ShouldQueryAgain => false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryAddress">The server address to query</param>
        /// <param name="queryType">The type of query</param>
        /// <param name="challengeRequired">Whether a challenge id needs to be requested</param>
        /// <param name="queryTimeout">How long to wait for a query response, in milliseconds</param>
        protected ServerQuery(EndPointAddress queryAddress, ServerQueryType queryType, bool challengeRequired, double queryTimeout)
        {
            mChallengeRequired = challengeRequired;
            QueryAddress = queryAddress;
            QueryType = queryType;
            QueryTimeout = queryTimeout;
        }
        
        /// <summary>
        /// Asynchronously sends this query to the server
        /// </summary>
        /// <returns>A handle to the query that can be used to identify the response</returns>
        public ServerQueryHandle Send()
        {
            UdpClient client = new UdpClient();
            client.Connect(QueryAddress.EndPoint);
            return InternalSend(client);
        }


        /// <summary>
        /// Returns the byte that identifyies the type of this query to the server
        /// </summary>
        protected abstract byte GetQueryHeader();

        /// <summary>
        /// Returns the data to use as the packet content when sending this query
        /// </summary>
        protected virtual byte[] GetQueryDatagram()
        {
            return new byte[0];
        }

        /// <summary>
        /// Called when a response has been received for a pending query
        /// </summary>
        /// <param name="queryHandle">The handle to the query (originally returned from Send)</param>
        /// <param name="result">The result of the query</param>
        /// <param name="response">The response from the server</param>
        /// <param name="pingMs">Server ping resposne time in milliseconds</param>
        protected abstract void OnQueryComplete(ServerQueryHandle queryHandle, ServerQueryResult result, byte[] response, float pingMs);
        
        /// <summary>
        /// Utility method to check whether a flag is set
        /// </summary>
        /// <param name="data">The data to check</param>
        /// <param name="flag">The flag to check for</param>
        protected bool CheckFlag(byte data, byte flag)
        {
            return (data & flag) != 0;
        }

        /// <summary>
        /// Send a request using the specified client. Useful for queries that need to do multiple sends.
        /// </summary>
        private ServerQueryHandle InternalSend(UdpClient client)
        {
            ServerQueryHandle queryHandle = new ServerQueryHandle();

            Timer timeoutTimer = new Timer(QueryTimeout);
            timeoutTimer.Elapsed += (s, e) => Timouet_Elapsed(queryHandle, timeoutTimer, client);
            timeoutTimer.Start();

            var response = new ResponseAggregator(client, new Stopwatch(), timeoutTimer, queryHandle);

            if (mChallengeRequired)
            {
                // Request a challenge ID from the server
                byte[] datagram =
                {
                    0xff, 0xff, 0xff, 0xff,
                    GetQueryHeader(),
                    0xff, 0xff, 0xff, 0xff
                };

                client.BeginSend(datagram, datagram.Length, ChallengeRequestCallback, response);
            }
            else
            {
                // No challenge required, just send the query
                byte[] queryData = GetQueryDatagram();
                byte[] datagram = new byte[5 + queryData.Length];
                datagram[0] = datagram[1] = datagram[2] = datagram[3] = 0xff;
                datagram[4] = GetQueryHeader();
                queryData.CopyTo(datagram, 5);

                response.ResponseTimer.Start();
                client.BeginSend(datagram, datagram.Length, SendCallback, response);
            }

            return queryHandle;
        }
        /// <summary>
        /// Challenge ID request has been sent (or interrupted)
        /// </summary>
        private void ChallengeRequestCallback(IAsyncResult result)
        {
            ResponseAggregator response = (ResponseAggregator)result.AsyncState;

            lock (response.Client)
            {
                // NOTE: If the query timed out, we will get called because the client receive was canceled by the timeout handler.
                if (response.Client.Client == null)
                {
                    // Client has been closed
                    return;
                }

                response.Client.EndSend(result);
                response.Client.BeginReceive(ChallengeReceiveCallback, response);
            }
        }

        /// <summary>
        /// Received a response (or timeout) from a challenge ID request
        /// </summary>
        private void ChallengeReceiveCallback(IAsyncResult result)
        {
            ResponseAggregator response = (ResponseAggregator)result.AsyncState;

            lock (response.Client)
            {
                // NOTE: If the query timed out, we will get called because the client receive was canceled by the timeout handler.
                if (response.Client.Client == null)
                {
                    // Client has been closed
                    return;
                }

                IPEndPoint endPoint = QueryAddress.EndPoint;
                byte[] responseData = response.Client.EndReceive(result, ref endPoint);

                if (responseData.Length != 9 || responseData[4] != 0x41)
                {
                    NotifyQueryComplete(response, ServerQueryResult.UnknownResponseReceived);
                    return;
                }

                byte[] queryData = GetQueryDatagram();
                if (queryData.Length == 0)
                {
                    // Can just swap the header byte from the response (which contains the challenge ID) and send it back as a request
                    responseData[4] = GetQueryHeader();
                }
                else
                {
                    // At the time of creating this, none of the existing queries had a requirement for this. Can add it later if it is ever needed.
                    throw new NotImplementedException("Have not implemented support for a query that has header content and requires a challenge ID");
                }

                response.ResponseTimer.Start();
                response.Client.BeginSend(responseData, responseData.Length, SendCallback, response);
            }
        }

        /// <summary>
        /// Query packet has been sent to the server
        /// </summary>
        private void SendCallback(IAsyncResult result)
        {
            ResponseAggregator response = (ResponseAggregator)result.AsyncState;

            lock (response.Client)
            {
                // NOTE: If the query timed out, we will get called because the client receive was canceled by the timeout handler.
                if (response.Client.Client == null)
                {
                    // Client has been closed
                    return;
                }

                response.Client.EndSend(result);
                response.Client.BeginReceive(ReceiveCallback, response);
            }
        }

        /// <summary>
        /// Timout period has elapsed for a query
        /// </summary>
        private void Timouet_Elapsed(ServerQueryHandle queryHandle, Timer timer, UdpClient client)
        {
            lock (client)
            {
                // NOTE: It is possible to get here and also receive a response from the server if the response comes in close enough to the timout tick.
                // However, the resonse handler will have closed the client, so we can abort this method if that happens.
                if (client.Client == null)
                {
                    // Client has been closed
                    return;
                }
                timer.Dispose();
                client.Close();
                OnQueryComplete(queryHandle, ServerQueryResult.QueryTimedOut, null, 0.0f);
            }
        }

        /// <summary>
        /// Server has responded to a query (or it has been canceled)
        /// </summary>
        private void ReceiveCallback(IAsyncResult result)
        {
            ResponseAggregator response = (ResponseAggregator)result.AsyncState;
			response.ResponseTimer.Stop();

			lock (response.Client)
            {
                // NOTE: If the query timed out, we will get called because the client receive was canceled by the timeout handler.
                if (response.Client.Client == null)
                {
                    // Client has been closed
                    return;
                }

                IPEndPoint endPoint = QueryAddress.EndPoint;
                byte[] responseData = response.Client.EndReceive(result, ref endPoint);

                if (mChallengeRequired && responseData.Length == 9 && responseData[4] == 0x41)
                {
                    // Got a new challenge ID. This is unusual and seems to only happen on heavily loaded servers.
                    // We will try sending the query again with the new ID.
                    responseData[4] = GetQueryHeader();
                    response.ResponseTimer.Restart();
                    response.Client.BeginSend(responseData, responseData.Length, SendCallback, response);
                    return;
                }

                try
                {
                    using (var stream = new MemoryStream(responseData))
                    using (var reader = new BinaryReader(stream))
                    {
                        int header = reader.ReadInt32();
                        if (header == -1)
                        {
                            // Packet is not split
                            NotifyQueryComplete(response, ServerQueryResult.ResponseReceived, responseData.SkipItems(4));
                        }
                        else if (header == -2)
                        {
#if DEBUG
                            // WARNING: Untested code. Have not been able to get this case to happen
                            if (System.Diagnostics.Debugger.IsAttached)
                            {
                                System.Diagnostics.Debugger.Break();
                            }
#endif

                            // Packet is split
                            int id = reader.ReadInt32();
                            byte totalPackets = reader.ReadByte();
                            byte packetNumber = reader.ReadByte();

                            // Sanity checks
                            if (totalPackets < 1 || totalPackets > 15 || packetNumber >= totalPackets)
                            {
                                NotifyQueryComplete(response, ServerQueryResult.UnknownResponseReceived);
                                return;
                            }

                            if (response.Responses == null)
                            {
                                // First received packet (not necessarily packet 0)
                                response.Responses = new byte[totalPackets][];
                            }

                            if (packetNumber == 0 && (id & 0x80000000) != 0)
                            {
                                // Stream is compressed
                                throw new NotImplementedException("Received a compressed response from the server. Support for this has not been implemented.");
                            }

                            response.Responses[packetNumber] = responseData.SkipItems((int)reader.BaseStream.Position);

                            if (response.Responses.Any(d => d == null))
                            {
                                // Still waiting on more responses
                                response.Client.BeginReceive(ReceiveCallback, response);
                            }
                            else
                            {
                                // Received all responses, combine response packet contents
                                NotifyQueryComplete(response, ServerQueryResult.ResponseReceived, response.Responses.SelectMany(d => d).ToArray().SkipItems(4));
                            }
                        }
                        else
                        {
                            NotifyQueryComplete(response, ServerQueryResult.UnknownResponseReceived);
                        }
                    }
                }
                catch
                {
                    NotifyQueryComplete(response, ServerQueryResult.UnknownResponseReceived);
                }
            }
        }

        /// <summary>
        /// Helper method to notify derived classes of query completion
        /// </summary>
        private void NotifyQueryComplete(ResponseAggregator response, ServerQueryResult result, byte[] data = null)
        {
            response.TimeoutTimer.Dispose();

            float pingMs = (float)((double)response.ResponseTimer.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0);
            OnQueryComplete(response.QueryHandle, result, data, pingMs);

            if (ShouldQueryAgain)
			{
                InternalSend(response.Client);
			}
            else
			{
                response.Client.Close();
            }
        }

        /// <summary>
        /// Utility for handling multi-packet query responses
        /// </summary>
        private class ResponseAggregator
        {
            public UdpClient Client { get; }

            public Timer TimeoutTimer { get; }

            public Stopwatch ResponseTimer { get; }

            public ServerQueryHandle QueryHandle { get; }

            public byte[][] Responses { get; set; }

            public ResponseAggregator(UdpClient client, Stopwatch responseTimer, Timer timeoutTimer, ServerQueryHandle queryHandle)
            {
                Client = client;
                TimeoutTimer = timeoutTimer;
                ResponseTimer = responseTimer;
                QueryHandle = queryHandle;
            }
        }
    }

    /// <summary>
    /// Base class for query implementations with a provided response type
    /// </summary>
    /// <typeparam name="ResponseType">The data type used for the QueryComplete event</typeparam>
	public abstract class ServerQuery<ResponseType> : ServerQuery
	{
        /// <summary>
        /// Event fired when a query has completed.
        /// </summary>
        public virtual event EventHandler<ServerQueryResponse<ResponseType>> QueryComplete;

        protected ServerQuery(EndPointAddress queryAddress, ServerQueryType queryType, bool challengeRequired, double queryTimeout)
            : base(queryAddress, queryType, challengeRequired, queryTimeout)
		{
		}

        protected void FireQueryComplete(ServerQueryHandle queryHandle, ServerQueryResult result, float pingMs, ResponseType data = default(ResponseType))
		{
            QueryComplete?.Invoke(this, new ServerQueryResponse<ResponseType>(queryHandle, result, pingMs, data));
		}
	}

	/// <summary>
	/// Identifies a specific server query. Can be used to match responses to requests.
	/// </summary>
	public class ServerQueryHandle
    {
        // Intentionally empty, only used to compare instances
    }
}
