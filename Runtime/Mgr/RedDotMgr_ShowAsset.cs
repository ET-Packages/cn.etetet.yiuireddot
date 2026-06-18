using System.Collections.Generic;
using ET;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace YIUIFramework
{
    public partial class RedDotMgr
    {
        private const string RedDotShowAssetName = "RedDotShowAsset";

        private readonly Dictionary<string, RedDotShowData> m_AllRedDotShowData = new();

        public IReadOnlyDictionary<string, RedDotShowData> AllRedDotShowData => m_AllRedDotShowData;

        private RedDotShowAsset m_RedDotShowAsset;

        private async ETTask<bool> LoadShowAsset()
        {
            var loadResult = await ET.EventSystem.Instance?.YIUIInvokeEntityAsyncSafety<YIUIInvokeEntity_Load, ETTask<UnityObject>>(Entity, new YIUIInvokeEntity_Load
            {
                LoadType = typeof(RedDotShowAsset),
                ResName = RedDotShowAssetName
            });

            if (loadResult == null)
            {
                Debug.LogWarning($"没有加载到动态红点配置 {RedDotShowAssetName}，当前动态红点功能不可用");
                return true;
            }

            m_RedDotShowAsset = (RedDotShowAsset)loadResult;
            InitShowData();

            ET.EventSystem.Instance?.YIUIInvokeEntitySyncSafety(Entity, new YIUIInvokeEntity_Release
            {
                obj = loadResult
            });

            return true;
        }

        private void InitShowData()
        {
            m_AllRedDotShowData.Clear();
            if (m_RedDotShowAsset == null)
            {
                return;
            }

            foreach (var showData in m_RedDotShowAsset.AllRedDotShowData)
            {
                if (showData == null) continue;
                if (string.IsNullOrEmpty(showData.EnumName)) continue;
                m_AllRedDotShowData[showData.EnumName] = showData;
            }
        }
    }
}
