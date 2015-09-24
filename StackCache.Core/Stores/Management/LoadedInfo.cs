namespace StackCache.Core
{
    using System;
    using ProtoBuf;

    [ProtoContract]
    public class LoadedInfo : LoadingInfo
    {
        public LoadedInfo()
        {
        }

        public LoadedInfo(LoadingInfo loading, DateTime loadFinished, int loadCount)
            : base(loading.Prefix, loading.MachineName, loading.LoadStart)
        {
            this.LoadFinished = loadFinished;
            this.LoadCount = loadCount;
        }

        [ProtoMember(4)]
        public DateTime LoadFinished { get; set; }

        [ProtoMember(5)]
        public int LoadCount { get; set; }
    }
}