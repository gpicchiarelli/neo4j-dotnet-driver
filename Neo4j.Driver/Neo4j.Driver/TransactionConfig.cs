// Copyright (c) "Neo4j"
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

using System;
using System.Collections.Generic;
using Neo4j.Driver.Internal;
using Neo4j.Driver.Internal.IO;

namespace Neo4j.Driver;

/// <summary>
/// Configuration object containing settings for explicit and auto-commit transactions. Leave the fields unmodified to use
/// server side transaction configurations.
/// <para/>
/// For example, the following code starts a transaction using server default transaction configurations.
/// <code>
/// session.BeginTransaction(b=>{});
/// </code>
/// </summary>
public sealed class TransactionConfig
{
    internal static readonly TransactionConfig Default = new();
    private IDictionary<string, object> _metadata;
    private TimeSpan? _timeout;

    internal TransactionConfig()
    {
        _timeout = null;
        _metadata = PackStream.EmptyDictionary;
    }

    internal TransactionConfig(IDictionary<string, object> metadata, TimeSpan? timeout)
    {
        _metadata = metadata;
        _timeout = timeout;
    }

    /// <summary>
    /// Transaction timeout. Transactions that execute longer than the configured timeout will be terminated
    /// by the database. This functionality allows user code to limit query/transaction execution time. The specified timeout
    /// overrides the default timeout configured in the database using the <code>db.transaction.timeout</code> setting 
    /// (<code>dbms.transaction.timeout</code> before Neo4j 5.0). Values higher than <code>db.transaction.timeout</code> will be
    /// ignored and will fall back to the default for server versions between 4.2 and 5.2 (inclusive). Leave this field unmodified or set it
    /// to <code>null</code> to use the default timeout configured on the server. A timeout of zero will make the transaction
    /// execute indefinitely.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If the value given to transaction timeout in milliseconds is less than zero.
    /// </exception>
    public TimeSpan? Timeout
    {
        get => _timeout;
        internal set
        {
            if ((value ?? TimeSpan.Zero) < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "Transaction timeout should not be negative.");
            }

            _timeout = value;
        }
    }

    /// <summary>
    /// The transaction metadata. Specified metadata will be attached to the executing transaction and visible in the
    /// output of <code>dbms.listQueries</code> and <code>dbms.listTransactions</code> procedures. It will also get logged to
    /// the <code>query.log</code>. Transactions starting with this <see cref="TransactionConfig"/> This functionality makes it
    /// easier to tag transactions and is equivalent to <code>dbms.setTXMetaData</code> procedure. Leave this field unmodified
    /// to use default timeout configured on database.
    /// </summary>
    public IDictionary<string, object> Metadata
    {
        get => _metadata;
        internal set => _metadata =
            value ?? throw new ArgumentNullException(nameof(value), "Transaction metadata should not be null");
    }

    /// <summary>Returns the config content in a nice string representation.</summary>
    /// <returns>The content of the transaction config in a string.</returns>
    public override string ToString()
    {
        return $"{GetType().Name}{{{nameof(Metadata)}={Metadata.ToContentString()}, {nameof(Timeout)}={Timeout}}}";
    }
}

/// <summary>The builder to create a <see cref="TransactionConfig"/></summary>
public sealed class TransactionConfigBuilder
{
    private readonly ILogger _logger;
    private readonly TransactionConfig _config;

    internal TransactionConfigBuilder(
        ILogger logger,
        TransactionConfig config)
    {
        _logger = logger;
        _config = config;
    }

    /// <summary>
    /// Sets the transaction timeout. Transactions that execute longer than the configured timeout will be terminated
    /// by the database. This functionality allows user code to limit query/transaction execution time. The specified timeout
    /// overrides the default timeout configured in the database using the <code>db.transaction.timeout</code> setting 
    /// (<code>dbms.transaction.timeout</code> before Neo4j 5.0). Values higher than <code>db.transaction.timeout</code> will be
    /// ignored and will fall back to default for server versions between 4.2 and 5.2 (inclusive). Leave this field unmodified or
    /// set it to <code>null</code> to use the default timeout configured on the server. A timeout of zero will make the transaction
    /// execute indefinitely.
    /// <para/>
    /// If the timeout is not an exact number of milliseconds, it will be rounded up to the next millisecond.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If the value given to transaction timeout in milliseconds is less than zero.
    /// </exception>
    /// <param name="timeout">The new timeout.</param>
    /// <returns>this <see cref="TransactionConfigBuilder"/> instance.</returns>
    public TransactionConfigBuilder WithTimeout(TimeSpan? timeout)
    {
        _config.Timeout = FixSubmilliseconds(timeout);
        return this;
    }

    private TimeSpan? FixSubmilliseconds(TimeSpan? timeout)
    {
        if(timeout == null || timeout == TimeSpan.MaxValue)
        {
            return timeout;
        }

        if (timeout.Value.Ticks % TimeSpan.TicksPerMillisecond == 0)
        {
            return timeout;
        }

        var result = TimeSpan.FromMilliseconds(Math.Ceiling(timeout.Value.TotalMilliseconds));
        _logger.Info(
            $"Transaction timeout {timeout} contains sub-millisecond precision and will be rounded up to {result}.");

        return result;
    }

    /// <summary>
    /// The transaction metadata. Specified metadata will be attached to the executing transaction and visible in the
    /// output of <code>dbms.listQueries</code> and <code>dbms.listTransactions</code> procedures. It will also get logged to
    /// the <code>query.log</code>. Transactions starting with this <see cref="TransactionConfig"/> This functionality makes it
    /// easier to tag transactions and is equivalent to <code>dbms.setTXMetaData</code> procedure. Leave this field unmodified
    /// to use default timeout configured on database.
    /// </summary>
    /// <param name="metadata">the metadata to set on transaction</param>
    /// <returns>this <see cref="TransactionConfigBuilder"/> instance</returns>
    public TransactionConfigBuilder WithMetadata(IDictionary<string, object> metadata)
    {
        _config.Metadata = metadata;
        return this;
    }

    internal TransactionConfig Build()
    {
        return _config;
    }
}
