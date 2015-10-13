namespace StackCache.Core.Election
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Leader election strategy, needed to determine who load data in Stores
    /// </summary>
    public interface IElection
    {
        /// <summary>
        /// Check if application identity can be the leader, in the context of a given store
        /// </summary>
        /// <param name="identity">Application identity</param>
        /// <param name="context">Store identity. If null or empty, then leadership is asked for all stores</param>
        /// <param name="leaderAction">Action execute by the leader</param>
        /// <returns>True is current identity/[context] is elected as leader</returns>
        Task<T> ExecuteIfLeader<T>(string identity, string context, Func<CancellationToken, Task<T>> leaderAction);
    }
}