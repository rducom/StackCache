namespace StackCache.Core.Helpers
{
    using System;

    /// <summary>
    /// General application node info
    /// </summary>
    public class ApplicationNode
    {
        private static readonly Lazy<ApplicationNode> _lazy = new Lazy<ApplicationNode>(() => new ApplicationNode());
        private readonly string _appIdentifier;

        private ApplicationNode()
        {
            this._appIdentifier = AppDomain.CurrentDomain.ApplicationIdentity + Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Get application Identifier
        /// </summary>
        public static string Identifier => _lazy.Value._appIdentifier;
    }
}