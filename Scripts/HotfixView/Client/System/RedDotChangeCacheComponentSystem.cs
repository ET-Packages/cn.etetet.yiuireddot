using YIUIFramework;

namespace ET.Client
{
    [EntitySystemOf(typeof(RedDotChangeCacheComponent))]
    [FriendOf(typeof(RedDotChangeCacheComponent))]
    public static partial class RedDotChangeCacheComponentSystem
    {
        [EntitySystem]
        private static void Awake(this RedDotChangeCacheComponent self)
        {
        }

        public static void Cache(this RedDotChangeCacheComponent self, int redDotId, int count)
        {
            if (self == null)
            {
                return;
            }

            self.PendingCounts[redDotId] = count;
        }

        public static void FlushIfReady(this RedDotChangeCacheComponent self)
        {
            if (self == null || self.PendingCounts.Count == 0 || !IsRedDotMgrReady())
            {
                return;
            }

            var redDotMgr = RedDotMgr.Inst;
            if (redDotMgr == null)
            {
                return;
            }

            foreach (var pair in self.PendingCounts)
            {
                redDotMgr.SetCount(pair.Key, pair.Value);
            }

            self.PendingCounts.Clear();
        }

        private static bool IsRedDotMgrReady()
        {
            if (!RedDotMgr.Exist)
            {
                return false;
            }

            var redDotMgr = RedDotMgr.Inst;
            return redDotMgr != null && redDotMgr.InitedSucceed && redDotMgr.Enabled;
        }
    }
}
