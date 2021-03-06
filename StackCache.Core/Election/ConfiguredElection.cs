﻿namespace StackCache.Core.Election
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Leader is elected by configuration.
    /// </summary>
    /// <remarks>Developer must be aware of this to avoid each node being a master</remarks>
    public class ConfiguredElection : IElection
    {
        private readonly bool _isLeader;

        public ConfiguredElection(bool isLeader)
        {
            this._isLeader = isLeader;
        }

        public async Task ExecuteIfLeader(string identity, string context, Func<Task> leaderAction)
        {
            if (this._isLeader)
                await leaderAction();
        }

        public async Task<T> ExecuteIfLeader<T>(string identity, string context, Func<CancellationToken, Task<T>> leaderAction)
        {
            if (this._isLeader)
                return await leaderAction(CancellationToken.None);
            return default(T);
        }
    }
}