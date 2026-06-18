#if UNITY_EDITOR
using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace YIUIFramework.Editor
{
    [Serializable]
    [HideLabel]
    [HideReferenceObjectPicker]
    internal class UIRedDotShowEditorData
    {
        [Required("必须设置红点对象！")]
        [VerticalGroup("预制体")]
        [ShowInInspector]
        [AssetsOnly]
        [TableColumnWidth(150, resizable: false)]
        [PreviewField(70, ObjectFieldAlignment.Center)]
        [OnValueChanged(nameof(OnPrefabChanged))]
        [HideLabel]
        public GameObject Prefab;

        [VerticalGroup("预制体")]
        [OdinSerialize]
        [ReadOnly]
        [HideLabel]
        public string ResName;

        [VerticalGroup("信息")]
        [OdinSerialize]
        [HideLabel]
        [OnValueChanged(nameof(OnValueChanged))]
        [LabelText("枚举名称")]
        public string EnumName;

        [VerticalGroup("信息")]
        [OdinSerialize]
        [LabelText("描述")]
        [OnValueChanged(nameof(OnValueChanged))]
        public string Des;

        [VerticalGroup("删")]
        [PropertySpace(22)]
        [TableColumnWidth(70, resizable: false)]
        [Button(70, Icon = SdfIconType.Trash)]
        [GUIColor(1f, 0.4f, 0.4f)]
        private void Delete()
        {
            m_View?.DeleteEditorData(this);
        }

        private readonly UIRedDotShowView m_View;

        public UIRedDotShowEditorData(UIRedDotShowView view)
        {
            m_View = view;
        }

        internal void RefreshPrefabByResName()
        {
            Prefab = m_View?.FindPrefabByResName(ResName);
        }

        private void OnValueChanged()
        {
            m_View?.MarkDirty();
        }

        private void OnPrefabChanged()
        {
            ResName = Prefab != null ? Prefab.name : string.Empty;
            m_View?.MarkDirty();
        }
    }
}
#endif