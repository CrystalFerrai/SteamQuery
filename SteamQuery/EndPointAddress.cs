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
using System.Net;

namespace SteamQuery
{
    /// <summary>
    /// Used by queries to represent the network endpoint to query
    /// </summary>
    public class EndPointAddress
    {
        /// <summary>
        /// The default query port, used when a port is not specified
        /// </summary>
        private const int DefaultPort = 27015;

        /// <summary>
        /// Get the IP address of the end point
        /// </summary>
        public IPAddress Ip { get; }

        /// <summary>
        /// Gets the port of the end point
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Gets the underlying endpoint represented by this object
        /// </summary>
        public IPEndPoint EndPoint { get; }
        
        /// <summary>
        /// Private constructor. Call Create, Parse or TryParse to create an instance
        /// </summary>
        /// <param name="endPoint">The endpoint to be represented by this object</param>
        private EndPointAddress(IPEndPoint endPoint)
        {
            Ip = endPoint.Address;
            Port = endPoint.Port;
            EndPoint = endPoint;
        }

        /// <summary>
        /// Creates an instance of this class using the specified IP address and port
        /// </summary>
        /// <param name="ip">The IP address to use</param>
        /// <param name="port">The port to use</param>
        /// <returns>The newly created end point.</returns>
        /// <exception cref="ArgumentNullException">The passed in IP address is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">The passed in port is out of range.</exception>
        public static EndPointAddress Create(IPAddress ip, int port)
        {
            if (ip == null) throw new ArgumentNullException(nameof(ip));
            if (port < 0 || port > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(port));

            return new EndPointAddress(new IPEndPoint(ip, port));
        }

        /// <summary>
        /// Creates an instance of this class using the specified IP address and the default query port
        /// </summary>
        /// <param name="ip">The IP address to use</param>
        /// <returns>The newly created end point.</returns>
        /// <exception cref="ArgumentNullException">The passed in IP address is null</exception>
        public static EndPointAddress Create(IPAddress ip)
        {
            if (ip == null) throw new ArgumentNullException(nameof(ip));

            return new EndPointAddress(new IPEndPoint(ip, DefaultPort));
        }

        /// <summary>
        /// Creates an instance of this class using the specified IPEndPoint
        /// </summary>
        /// <param name="endPoint">The end point to represent</param>
        /// <returns>The newly created end point.</returns>
        /// <exception cref="ArgumentNullException">The passed in end point is null</exception>
        public static EndPointAddress Create(IPEndPoint endPoint)
        {
            if (endPoint == null) throw new ArgumentNullException(nameof(endPoint));

            return new EndPointAddress(endPoint);
        }

        /// <summary>
        /// Parse a string as an end point.
        /// </summary>
        /// <param name="address">The address to parse</param>
        /// <returns>The newly created end point.</returns>
        /// <exception cref="ArgumentNullException">The passed in address is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">The passed in address contains a port that is out of range.</exception>
        /// <exception cref="ArgumentException">The passed in address could not be parsed as a valid endpoint</exception>
        /// <remarks>Format should be ip:port where ip is anything that can be parsed by IPAddress.Parse and port is a valid port number. If the port is omitted, the default port will be used.</remarks>
        public static EndPointAddress Parse(string address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));

            var badAddressException = new ArgumentException("Invalid address. Address must contain an IP and a port in the format ip:port", nameof(address));

            string[] parts = address.Split(':');
            if (parts.Length < 1 || parts.Length > 2) throw badAddressException;

            IPAddress ip;
            if (!IPAddress.TryParse(parts[0], out ip)) throw badAddressException;

            int port = DefaultPort;
            if (parts.Length > 1)
            {
                if (!int.TryParse(parts[1], out port)) throw badAddressException;
                if (port < 0 || port > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(port));
            }

            return new EndPointAddress(new IPEndPoint(ip, port));
        }

        /// <summary>
        /// Parse a string as an end point.
        /// </summary>
        /// <param name="address">The address to parse</param>
        /// <returns>The newly created end point.</returns>
        /// <exception cref="ArgumentNullException">The passed in address is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">The passed in address contains a port that is out of range.</exception>
        /// <exception cref="ArgumentException">The passed in address could not be parsed as a valid endpoint</exception>
        /// <remarks>Format of ip must be something that can be parsed by IPAddress.Parse and port must be a valid port number.</remarks>
        public static EndPointAddress Parse(string ip, int port)
        {
            if (ip == null) throw new ArgumentNullException(nameof(ip));
            if (port < 0 || port > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(port));

            IPAddress ipAddr;
            if (!IPAddress.TryParse(ip, out ipAddr)) throw new ArgumentException("Invalid IP address", nameof(ip));

            return new EndPointAddress(new IPEndPoint(ipAddr, port));
        }

        /// <summary>
        /// Attempts to parse a string as an end point
        /// </summary>
        /// <param name="address">The address to attempt to parse.</param>
        /// <param name="endPoint">Outputs the newly created end point.</param>
        /// <returns>True if the string was successfully parsed, else false.</returns>
        /// <remarks>Format should be ip:port where ip is anything that can be parsed by IPAddress.Parse and port is a valid port number. If the port is omitted, the default port will be used.</remarks>
        public static bool TryParse(string address, out EndPointAddress endPoint)
        {
            string[] parts = address.Split(':');
            if (parts.Length < 1 || parts.Length > 2)
            {
                endPoint = null;
                return false;
            }

            IPAddress ipAddr;
            if (!IPAddress.TryParse(parts[0], out ipAddr))
            {
                endPoint = null;
                return false;
            }

            int port = DefaultPort;
            if (parts.Length > 1)
            {
                if (!int.TryParse(parts[1], out port))
                {
                    endPoint = null;
                    return false;
                }
            }

            endPoint = new EndPointAddress(new IPEndPoint(ipAddr, port));
            return true;
        }

        /// <summary>
        /// Attempts to parse a string as an end point
        /// </summary>
        /// <param name="ip">The IP address to attempt to parse.</param>
        /// <param name="port">The port to use in the created end point.</param>
        /// <param name="endPoint">Outputs the newly created end point.</param>
        /// <returns>True if the string was successfully parsed, else false.</returns>
        /// <remarks>Format of ip should be something that can be parsed by IPAddress.Parse and port should be a valid port number.</remarks>
        public static bool TryParse(string ip, int port, out EndPointAddress endPoint)
        {
            IPAddress ipAddr;
            if (!IPAddress.TryParse(ip, out ipAddr))
            {
                endPoint = null;
                return false;
            }

            if (port < 0 || port > ushort.MaxValue)
            {
                endPoint = null;
                return false;
            }

            endPoint = new EndPointAddress(new IPEndPoint(ipAddr, port));
            return true;
        }

        /// <summary>
        /// Returns a string representation of this end point
        /// </summary>
        public override string ToString()
        {
            return $"{Ip}:{Port}";
        }

        /// <summary>
        /// Returns a hash code for this object
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                return Ip.GetHashCode() * 17 + Port.GetHashCode() * 13;
            }
        }

        /// <summary>
        /// Returns whether the passed in object is equal to this end point
        /// </summary>
        /// <param name="obj">The object to compare</param>
        public override bool Equals(object obj)
        {
            EndPointAddress other = obj as EndPointAddress;
            if (other == null) return false;
            return Ip.Equals(other.Ip) && Port == other.Port;
        }

        /// <summary>
        /// Compares two end points for equality
        /// </summary>
        public static bool operator ==(EndPointAddress a, EndPointAddress b)
        {
            return ReferenceEquals(a, b) || !ReferenceEquals(a, null) && a.Equals(b);
        }

        /// <summary>
        /// Compares two end points for inequality
        /// </summary>
        public static bool operator !=(EndPointAddress a, EndPointAddress b)
        {
            return !(ReferenceEquals(a, b) || !ReferenceEquals(a, null) && a.Equals(b));
        }
    }
}
