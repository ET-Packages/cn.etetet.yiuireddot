using System.Collections.Generic;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public partial class RedDotChangeCacheComponent : Entity, IAwake
    {
        public Dictionary<int, int> PendingCounts { get; } = new();
    }
}
