using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

namespace YIUIFramework
{
    public static class RedDotKeyHelper
    {
        private static List<int> m_AllRedDotKey;

        private static List<int> AllRedDotKey
        {
            get
            {
                if (m_AllRedDotKey == null)
                {
                    GetKeys();
                }

                return m_AllRedDotKey;
            }
        }

        private static HashSet<int> m_HashAllRedDotKey;

        /// <summary>
        /// 检查这个Key 是否存在
        /// </summary>
        public static bool ContainsKey(int key)
        {
            if (m_HashAllRedDotKey == null)
            {
                m_HashAllRedDotKey = new();
                m_HashAllRedDotKey.AddRange(AllRedDotKey);
            }

            return m_HashAllRedDotKey.Contains(key);
        }

        /// <summary>
        /// 反射获取枚举列表
        /// 注意: 运行时使用的是ET编译过的dll
        /// 如果新增了没有编译 则无法获取到最新数据 请注意一定要编译后运行
        /// </summary>
        public static List<int> GetKeys(bool force = false)
        {
            if (m_AllRedDotKey != null && !force)
            {
                return m_AllRedDotKey;
            }

            m_AllRedDotKey     = new();
            m_HashAllRedDotKey = null;
            #if UNITY_EDITOR
            m_RedDotKeyDesc = new();
            #endif

            var assembly = AssemblyHelper.GetAssembly("ET.Model");
            if (assembly == null)
            {
                Logger.LogError($"没有找到ET.Model程序集");
                return m_AllRedDotKey;
            }

            Type redDotKeyType = assembly.GetType("ET.ERedDotKeyType");
            if (redDotKeyType == null)
            {
                Logger.LogError($"没有找到ET.ERedDotKeyType类型");
                return m_AllRedDotKey;
            }

            FieldInfo[] fields = redDotKeyType.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (FieldInfo field in fields)
            {
                if (field.IsLiteral && field.FieldType == typeof(int))
                {
                    int key = (int)field.GetValue(null);
                    m_AllRedDotKey.Add(key);

                    #if UNITY_EDITOR
                    var keyDesc            = "";
                    var labelTextAttribute = field.GetCustomAttribute<LabelTextAttribute>();
                    if (labelTextAttribute != null)
                    {
                        keyDesc = labelTextAttribute.Text;
                    }

                    m_RedDotKeyDesc[key] = keyDesc;
                    #endif
                }
            }

            return m_AllRedDotKey;
        }

        #if UNITY_EDITOR
        private static Dictionary<int, string> m_RedDotKeyDesc;

        // 获取红点描述
        // Editor 时可用
        internal static string GetDesc(int key)
        {
            return m_RedDotKeyDesc?.GetValueOrDefault(key, "");
        }

        // 统一显示红点描述
        internal static string GetDisplayDesc(int key)
        {
            return $"{key}_{m_RedDotKeyDesc?.GetValueOrDefault(key, "")}";
        }

        private const int MAX_GROUP_SIZE = 10;

        // 显示红点描述的下拉列表
        // 新ID规则：
        // 1        = 根节点
        // 1001     = 二级节点（预留3位）
        // 100101   = 三级节点（每级追加2位）
        // 10010101 = 四级节点（每级追加2位）
        // 因此下拉菜单需要按树形层级分组，而不是按简单递增范围分组。
        internal static ValueDropdownList<int> SubDisplayValueDropdownList(ValueDropdownList<int> keys)
        {
            keys.Sort((a, b) =>
            {
                var aId = a.Value;
                var bId = b.Value;
                return aId.CompareTo(bId);
            });

            var result = new ValueDropdownList<int>();
            var keyMap = new Dictionary<int, ValueDropdownItem<int>>();
            var childMap = new Dictionary<int, List<ValueDropdownItem<int>>>();

            foreach (var item in keys)
            {
                if (item.Value <= 0)
                {
                    continue;
                }

                keyMap[item.Value] = item;
            }

            foreach (var item in keys)
            {
                if (item.Value <= 0)
                {
                    continue;
                }

                var parentKey = GetDisplayParentKey(item.Value, keyMap);
                if (!childMap.TryGetValue(parentKey, out var childList))
                {
                    childList = new List<ValueDropdownItem<int>>();
                    childMap[parentKey] = childList;
                }

                childList.Add(item);
            }

            if (keyMap.TryGetValue(1, out var rootItem))
            {
                result.Add(rootItem.Text, rootItem.Value);
            }

            AppendDropdownItems(result, childMap, 1, "", new HashSet<int> { 1 });

            return result;
        }

        private static void AppendDropdownItems(
                ValueDropdownList<int> result,
                Dictionary<int, List<ValueDropdownItem<int>>> childMap,
                int parentKey,
                string prefix,
                HashSet<int> parentChain)
        {
            if (!childMap.TryGetValue(parentKey, out var children) || children.Count == 0)
            {
                return;
            }

            children.Sort((a, b) => a.Value.CompareTo(b.Value));

            for (var startIndex = 0; startIndex < children.Count; startIndex += MAX_GROUP_SIZE)
            {
                var count = Math.Min(MAX_GROUP_SIZE, children.Count - startIndex);
                var groupPrefix = prefix;

                if (children.Count > MAX_GROUP_SIZE)
                {
                    var startSegment = GetLocalSegmentText(children[startIndex].Value);
                    var endSegment = GetLocalSegmentText(children[startIndex + count - 1].Value);
                    groupPrefix = CombinePath(prefix, $"{startSegment}-{endSegment}");
                }

                for (var index = startIndex; index < startIndex + count; index++)
                {
                    var child = children[index];
                    if (child.Value <= 0)
                    {
                        continue;
                    }

                    var itemPath = CombinePath(groupPrefix, child.Text);
                    result.Add(itemPath, child.Value);
                    if (parentChain.Contains(child.Value))
                    {
                        continue;
                    }

                    var childChain = new HashSet<int>(parentChain) { child.Value };
                    AppendDropdownItems(result, childMap, child.Value, itemPath, childChain);
                }
            }
        }

        private static int GetDisplayParentKey(int key, Dictionary<int, ValueDropdownItem<int>> keyMap)
        {
            if (key == 1)
            {
                return 0;
            }

            if (!TryGetParentKey(key, out var parentKey))
            {
                return 1;
            }

            if (parentKey == 1 || keyMap.ContainsKey(parentKey))
            {
                return parentKey;
            }

            return 1;
        }

        private static bool TryGetParentKey(int key, out int parentKey)
        {
            parentKey = 0;

            if (key <= 1)
            {
                return false;
            }

            var keyText = key.ToString();
            if (keyText.Length <= 4)
            {
                parentKey = 1;
                return true;
            }

            parentKey = int.Parse(keyText.Substring(0, keyText.Length - 2));
            return true;
        }

        private static string GetLocalSegmentText(int key)
        {
            var keyText = key.ToString();
            if (keyText.Length <= 4)
            {
                return keyText.Substring(1).PadLeft(3, '0');
            }

            return keyText.Substring(keyText.Length - 2, 2);
        }

        private static string CombinePath(string prefix, string segment)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return segment;
            }

            return $"{prefix}/{segment}";
        }

        #endif
    }
}
