namespace StackCache.Core.Configuration
{
    /// <summary>
    /// Redis server description
    /// </summary>
    public class RedisServer
    {
        /// <summary>
        /// Network hostname or IP
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Redis instance port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Redis instance password
        /// </summary>
        public string Password { get; set; }
    }
}