﻿// Copyright 2023 Crystal Ferrai
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

using SteamQuery;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace SteamQueryTest
{
    /// <summary>
    /// Simple console app that performs queries and prints responses to test the SteamQuery library
    /// </summary>
	internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Out.WriteLine("Usage: SteamQueryTest serverIp:port");
                return 0;
            }

            EndPointAddress server;
            if (!EndPointAddress.TryParse(args[0], out server))
            {
                Console.Error.WriteLine("{0} could not be parsed as a valid end point address", args[0]);
            }

            PerformQuery<ServerInfoQuery, ServerInfoData>("Info Query", new ServerInfoQuery(server));
            PerformQuery<ServerRulesQuery, ServerRulesData>("Rules Query", new ServerRulesQuery(server));
            PerformQuery<ServerPlayersQuery, ServerPlayersData>("Players Query", new ServerPlayersQuery(server));

            Console.Out.WriteLine("Press any key to exit...");
            Console.ReadKey();

            return 0;
        }

        private static void PerformQuery<QueryType, ResponseType>(string name, QueryType query) where QueryType : ServerQuery<ResponseType>
        {
            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

            Console.Out.WriteLine(name);
            Console.Out.WriteLine("----------------------------------------");

            query.QueryComplete += (sender, response) =>
            {
                Console.Out.WriteLine(HandleResponse(response));
                waitHandle.Set();
            };

            query.Send();

            if (!waitHandle.WaitOne(5000))
            {
                Console.Out.WriteLine("No response.");
            }

            Console.Out.WriteLine();
        }

        private static string HandleResponse<T>(ServerQueryResponse<T> response)
        {
            StringBuilder responseOutput = new();

            switch (response.Result)
            {
                case ServerQueryResult.QueryTimedOut:
                    responseOutput.AppendLine("Query timed out.");
                    break;
                case ServerQueryResult.UnknownResponseReceived:
                    responseOutput.AppendLine("Query returned an unrecognized response.");
                    break;
                case ServerQueryResult.ResponseReceived:
                    foreach (PropertyInfo prop in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                        {
                            IEnumerable propValue = (IEnumerable)prop.GetValue(response.Data);
                            responseOutput.AppendLine($"{prop.Name} [{propValue.Cast<object>().Count()}]");
                            foreach (var item in propValue)
                            {
                                responseOutput.AppendLine($"  {item}");
                            }
                        }
                        else
                        {
                            responseOutput.AppendLine($"{prop.Name}: {prop.GetValue(response.Data)}");
                        }
                    }
                    break;
            }

            return responseOutput.ToString();
        }
    }
}
