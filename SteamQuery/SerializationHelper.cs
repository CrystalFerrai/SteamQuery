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
    /// Helper class for working with stream data serialization
    /// </summary>
    internal static class SerializationHelper
    {
        /// <summary>
        /// Returns a new array that is the passed in array with the specified number of items skipped at the beginning.
        /// </summary>
        /// <param name="array">The array to skip items on</param>
        /// <param name="skip">The number of items to skip</param>
        public static T[] SkipItems<T>(this T[] array, int skip)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (skip > array.Length) throw new ArgumentOutOfRangeException(nameof(skip));

            T[] val = new T[array.Length - skip];
            Array.Copy(array, skip, val, 0, val.Length);
            return val;
        }

        /// <summary>
        /// Reads a null-terminated ASCII string from a stream
        /// </summary>
        /// <param name="stream">The stream from which to read the string</param>
        /// <param name="raw">The raw data the stream is associated with</param>
        public static string ReadString(Stream stream, byte[] raw)
        {
            long startPos = stream.Position;
            while (stream.ReadByte() > 0) { }
            int range = (int)(stream.Position - startPos);
            return Encoding.ASCII.GetString(raw, (int)startPos, range - 1);
        }
    }
}
