using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace YIUIFramework
{
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class RedDotShowData
    {
        [LabelText("枚举名")]
        [ShowInInspector]
        [ReadOnly]
        [OdinSerialize]
        public string EnumName { get; internal set; }

        [LabelText("描述")]
        [ShowInInspector]
        [ReadOnly]
        [OdinSerialize]
        public string Des { get; internal set; }

        [LabelText("预制体名称")]
        [ShowInInspector]
        [ReadOnly]
        [OdinSerialize]
        public string ResName { get; internal set; }
    }
}
