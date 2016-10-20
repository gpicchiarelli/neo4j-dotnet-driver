// Copyright (c) 2002-2016 "Neo Technology,"
// Network Engine for Objects in Lund AB [http://neotechnology.com]
// 
// This file is part of Neo4j.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using Neo4j.Driver.V1;

namespace Neo4j.Driver.Internal.Routing
{
    internal class ClusterDiscoveryManager
    {
        private readonly IPooledConnection _conn;
        private ILogger logger;
        public IEnumerable<Uri> Readers { get; internal set; } // = new Uri[0];
        public IEnumerable<Uri> Writers { get; internal set; } // = new Uri[0];
        public IEnumerable<Uri> Routers { get; internal set; } // = new Uri[0];

        private const string ProcedureName = "dbms.cluster.routing.getServers";
        public ClusterDiscoveryManager(IPooledConnection connection)
        {
            _conn = connection;
        }

        public void Rediscovery()
        {
            // TODO error handling???
            using (var session = new Session(_conn, logger))
            {
                var result = session.Run($"CALL {ProcedureName}");
                var record = result.Single();
                foreach (var servers in record["servers"].As<IList<IDictionary<string,object>>>())
                {
                    var addresses = servers["addresses"].As<IList<string>>();
                    var role = servers["role"].As<string>();
                    switch (role)
                    {
                        // TODO test 0 size array
                        case "READ":
                            Readers = addresses.Select(address => new Uri(address)).ToArray();
                            break;
                        case "WRITE":
                            Writers = addresses.Select(address => new Uri(address)).ToArray();
                            break;
                        case "ROUTE":
                            Routers = addresses.Select(address => new Uri(address)).ToArray();
                            break;
                    }
                }
            }
        }
    }
}