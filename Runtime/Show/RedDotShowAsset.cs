using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace YIUIFramework
{
    [LabelText("红点动态预制关联资源")]
    public class RedDotShowAsset : SerializedScriptableObject
    {
        [NonSerialized]
        [OdinSerialize]
        [ReadOnly]
        [ShowInInspector]
        public List<RedDotShowData> m_AllRedDotShowData = new();

        public IReadOnlyList<RedDotShowData> AllRedDotShowData => m_AllRedDotShowData;
    }
}