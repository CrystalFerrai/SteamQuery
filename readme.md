# SteamQuery

A library for performing Steam game server queries using Valve's server query protocol described [here](https://developer.valvesoftware.com/wiki/Server_queries).

## Features

Implements the following query types:
* Info (A2S_INFO)
* Rules (A2S_RULES)
* Players (A2S_PLAYER)

## Releases

There are no planned releases for this library. You can either clone the repo and built it or use the repo as a submodule.

## How to Use

The repo includes a test console application called `SteamQueryTest`. For example usage see [Program.cs](SteamQueryTest/Program.cs).

# Building

SteamQuery is built in Visual Studio 2022 using .NET 6. It has no other dependencies. You should be able to open the sln and build it, or reference the project from your own project's sln.

# Reporting Issues

If you find any problems with the library, you can [open an issue](https://github.com/CrystalFerrai/SteamQuery/issues). Include as much detail as you can. I will look into reported issues when I find time.
