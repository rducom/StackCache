namespace StackCache.Core
{
    using System;
    using CacheKeys;
    using ProtoBuf;

    // 1 - take v�rou => cl� de cache contien le Region
    // 2 - Load data
    // 3 - Put data en 2nd level
    // 4 - Put RegionInfo => la region existe + date chargement
    // 5 - Notify Region change
    // 6 - Lache v�rou


    // concurent :
    // 1 - Take verou => FAIL
    // 2 - Attendre la notification PUB/SUB

    [ProtoContract]
    public class LoadingInfo
    {
        public LoadingInfo()
        {
        }

        public LoadingInfo(KeyPrefix prefix, string machineName, DateTime loadStart)
        {
            this.Prefix = prefix;
            this.MachineName = machineName;
            this.LoadStart = loadStart;
        }

        [ProtoMember(1)]
        public KeyPrefix Prefix { get; set; }

        [ProtoMember(2)]
        public string MachineName { get; set; }

        [ProtoMember(3)]
        public DateTime LoadStart { get; set; }
    }
}