#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace YIUIFramework.Editor
{
    [HideLabel]
    [HideReferenceObjectPicker]
    internal class UIRedDotShowView : BaseCreateModule
    {
        private const string EnumNameRegex = "^[A-Z][A-Za-z0-9]*$";

        private readonly UIRedDotModule m_Module;
        private readonly RedDotShowAsset m_ShowAsset;

        [TableList(DrawScrollView = true, AlwaysExpanded = true, IsReadOnly = true)]
        [OdinSerialize]
        [BoxGroup("红点动态预制关联", centerLabel: true)]
        [HideLabel]
        [ShowIf(nameof(ShowView))]
        private List<UIRedDotShowEditorData> m_EditorDataList = new();

        private bool m_Dirty;

        public UIRedDotShowView(UIRedDotModule module)
        {
            m_Module = module;
            m_ShowAsset = module.m_RedDotShowAsset;
        }

        public override void Initialize()
        {
            InitEditorDataList();
            m_Module.OnChangeViewSetIndex += OnChangeViewSetIndex;
        }

        public override void OnDestroy()
        {
            m_Module.OnChangeViewSetIndex -= OnChangeViewSetIndex;
            TrySyncDirty();
        }

        [ShowIf(nameof(ShowView))]
        [Button("新增一条", 40, Icon = SdfIconType.PlusCircle)]
        [GUIColor(0.3f, 0.9f, 0.3f)]
        [PropertyOrder(-10)]
        private void AddItem()
        {
            m_EditorDataList.Add(new UIRedDotShowEditorData(this));
            m_Dirty = true;
        }

        [ShowIf(nameof(ShowView))]
        [Button("同步到Asset并生成枚举", 40, Icon = SdfIconType.CloudUpload)]
        [GUIColor(0.4f, 0.8f, 1f)]
        [PropertyOrder(-9)]
        private void SyncAndGenerate()
        {
            if (!ValidateAll(out var error))
            {
                UnityTipsHelper.Show(error);
                return;
            }

            SyncToAsset();
            GenerateEnum();
            YIUIAutoTool.CloseWindowRefresh();
        }

        internal void DeleteEditorData(UIRedDotShowEditorData data)
        {
            m_EditorDataList.Remove(data);
            m_Dirty = true;
        }

        internal void MarkDirty()
        {
            m_Dirty = true;
        }

        private string GetEditorDataError(UIRedDotShowEditorData data)
        {
            if (data == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(data.EnumName))
            {
                return "枚举名称不能为空";
            }

            if (data.EnumName == nameof(ERadDotShowType.None))
            {
                return "None 是保留名称，不可使用";
            }

            if (!Regex.IsMatch(data.EnumName, EnumNameRegex))
            {
                return "枚举名称必须全英文、大驼峰、只允许字母和数字";
            }

            if (string.IsNullOrWhiteSpace(data.Des))
            {
                return "描述不能为空";
            }

            if (string.IsNullOrWhiteSpace(data.ResName))
            {
                return "必须关联一个预制体";
            }

            if (!TryGetUniquePrefabResName(data.Prefab, out _, out var prefabError))
            {
                return prefabError;
            }

            return string.Empty;
        }

        private bool TryGetUniquePrefabResName(GameObject prefab, out string resName, out string error)
        {
            resName = string.Empty;
            error = string.Empty;

            if (prefab == null)
            {
                error = "必须关联一个预制体";
                return false;
            }

            resName = prefab.name;
            var guids = AssetDatabase.FindAssets($"{resName} t:Prefab", null);
            var matchCount = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (obj != null && obj.name == resName)
                {
                    matchCount++;
                }
            }

            if (matchCount != 1)
            {
                error = $"预制体名称 [{resName}] 必须全局唯一，当前匹配数量: {matchCount}";
                return false;
            }

            return true;
        }

        internal GameObject FindPrefabByResName(string resName)
        {
            if (string.IsNullOrEmpty(resName))
            {
                return null;
            }

            var guids = AssetDatabase.FindAssets($"{resName} t:Prefab", null);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (obj != null && obj.name == resName)
                {
                    return obj;
                }
            }

            return null;
        }

        private void InitEditorDataList()
        {
            m_EditorDataList.Clear();
            foreach (var data in m_ShowAsset.AllRedDotShowData)
            {
                var prefab = FindPrefabByResName(data.ResName);
                if (prefab == null)
                {
                    Debug.LogError($"动态红点预制体资源丢失 EnumName = {data.EnumName} ResName = {data.ResName}");
                }

                var editorData = new UIRedDotShowEditorData(this)
                {
                    EnumName = data.EnumName,
                    Des = data.Des,
                    ResName = data.ResName,
                    Prefab = prefab,
                };
                m_EditorDataList.Add(editorData);
            }

            m_Dirty = false;
        }

        private bool ValidateAll(out string error)
        {
            var nameHash = new HashSet<string>();
            foreach (var data in m_EditorDataList)
            {
                error = GetEditorDataError(data);
                if (!string.IsNullOrEmpty(error))
                {
                    error = $"[{data.EnumName}] {error}";
                    return false;
                }

                if (!nameHash.Add(data.EnumName))
                {
                    error = $"枚举名称重复: {data.EnumName}";
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        private void SyncToAsset()
        {
            m_ShowAsset.m_AllRedDotShowData.Clear();
            foreach (var data in m_EditorDataList)
            {
                m_ShowAsset.m_AllRedDotShowData.Add(new RedDotShowData
                {
                    EnumName = data.EnumName,
                    Des = data.Des,
                    ResName = data.ResName,
                });
            }

            EditorUtility.SetDirty(m_ShowAsset);
            AssetDatabase.SaveAssets();
            m_Dirty = false;
        }

        private void GenerateEnum()
        {
            var createData = new UICreateRedDotShowData
            {
                AutoRefresh = true,
                ShowTips = true,
                ClassPath = m_Module.UIRedDotShowClassPath,
                Content = UICreateRedDotShowGet.Get(m_EditorDataList),
            };

            _ = new UICreateRedDotShowCode(out var result, YIUIAutoTool.Author, createData);
            if (!result)
            {
                Debug.LogError("动态红点展示枚举生成失败");
            }
        }

        private bool ShowView()
        {
            return m_Module.CurrentViewType == UIRedDotModule.EUIRedDotViewType.DynamicShow;
        }

        private void OnChangeViewSetIndex(UIRedDotModule.EUIRedDotViewType viewType)
        {
            if (viewType == UIRedDotModule.EUIRedDotViewType.DynamicShow)
            {
                return;
            }

            TrySyncDirty();
        }

        private void TrySyncDirty()
        {
            if (!m_Dirty)
            {
                return;
            }

            UnityTipsHelper.CallBack("动态红点预制关联配置已修改，是否同步到Asset并生成枚举？\n\n取消将丢失本次修改。", () =>
            {
                if (!ValidateAll(out var error))
                {
                    UnityTipsHelper.Show(error);
                    return;
                }

                SyncToAsset();
                GenerateEnum();
            });
        }
    }
}
#endif
