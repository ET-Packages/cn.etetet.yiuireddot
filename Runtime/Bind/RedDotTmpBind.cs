using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace YIUIFramework
{
    public class RedDotTmpBind : MonoBehaviour
    {
        [SerializeField]
        [LabelText("文本")]
        private TextMeshProUGUI m_Text;

        [SerializeField]
        [LabelText("红点枚举")]
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        #if UNITY_EDITOR
        [ValueDropdown("GetKeyTypesSelectList", DoubleClickToConfirm = true)]
        #endif
        private int m_Key;

        #if UNITY_EDITOR
        private IEnumerable GetKeyTypesSelectList()
        {
            var showValues = new ValueDropdownList<int>();
            var keys       = RedDotKeyHelper.GetKeys();

            foreach (var key in keys)
            {
                showValues.Add(RedDotKeyHelper.GetDisplayDesc(key), key);
            }

            return RedDotKeyHelper.SubDisplayValueDropdownList(showValues);
        }
        #endif

        public int Key => m_Key;

        [ShowInInspector]
        [ReadOnly]
        [LabelText("显影")]
        public bool Show { get; private set; }

        [ShowInInspector]
        [ReadOnly]
        [LabelText("数量")]
        public int Count { get; private set; }

        private void Awake()
        {
            if (Key <= 0) return;
            RedDotMgr.Inst?.AddChanged(Key, OnRedDotChangeHandler);
        }

        private void OnDestroy()
        {
            if (Key <= 0) return;
            if (YIUISingletonHelper.Disposing)
                return;
            RedDotMgr.Inst?.RemoveChanged(Key, OnRedDotChangeHandler);
        }

        private void OnRedDotChangeHandler(int count)
        {
            Show  = count >= 1;
            Count = count;
            Refresh();
        }

        private void Refresh()
        {
            gameObject.SetActive(Show);
            if (m_Text != null)
                m_Text.text = Count.ToString();
        }
    }
}