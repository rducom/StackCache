namespace StackCache.Core.Configuration
{
    using System;
    using System.Collections.Generic;

    public class RedisSetting : Setting
    {
        public String ServerName
        {
            get; set;
        }

        public IEnumerable<RedisServer> RedisInstances
        {
            get; set;
        }


    }
}