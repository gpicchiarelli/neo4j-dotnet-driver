﻿// Copyright (c) "Neo4j"
// Neo4j Sweden AB [https://neo4j.com]
// 
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Neo4j.Driver;

/// <summary>Provides basic information of the server where the cypher query was executed.</summary>
public interface IServerInfo
{
    /// <summary>Get the address of the server</summary>
    string Address { get; }

    /// <summary>Get the protocol version</summary>
    string ProtocolVersion { get; }

    /// <summary>Get the entire server agent string</summary>
    string Agent { get; }
}
