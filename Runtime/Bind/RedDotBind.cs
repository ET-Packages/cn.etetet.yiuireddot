using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YIUIFramework
{
    [AddComponentMenu("YIUIFramework/红点/红点通用绑定 【RedDotBind】")]
    public class RedDotBind : MonoBehaviour, IDynamicRedDotControl
    {
        [LabelText("红点显隐对象")]
        [Required("必须设置红点显隐对象！")]
        public GameObject m_TargetGameObject;

        [SerializeField]
        [LabelText("红点枚举")]
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        #if UNITY_EDITOR
        [ValueDropdown("GetKeyTypesSelectList", DoubleClickToConfirm = true)]
        [OnValueChanged("OnKeyValueChanged")]
        #endif
        private int m_Key;

        #if UNITY_EDITOR
        private IEnumerable GetKeyTypesSelectList()
        {
            var showValues = new ValueDropdownList<int>();
            var keys = RedDotKeyHelper.GetKeys();

            foreach (var key in keys)
            {
                showValues.Add(RedDotKeyHelper.GetDisplayDesc(key), key);
            }

            return RedDotKeyHelper.SubDisplayValueDropdownList(showValues);
        }

        [ShowInInspector]
        [NonSerialized]
        [Delayed]
        [OnValueChanged("OnInputKeyValue")]
        [HideInPlayMode]
        [LabelText("手动选择")]
        public int InputChangeKey; //当枚举多了 下拉菜单不好用的时候/或不想下拉菜单选择的时候可手动输入

        private void OnInputKeyValue()
        {
            if (!RedDotKeyHelper.ContainsKey(InputChangeKey))
            {
                Logger.LogErrorContext(this.gameObject, $"不存在这个Key 请检查 {InputChangeKey}");
                m_Key = 0;
                InputChangeKey = 0;
                return;
            }

            m_Key = InputChangeKey;
        }

        private void OnKeyValueChanged()
        {
            InputChangeKey = m_Key;
        }

        private void OnValidate()
        {
            m_TargetGameObject ??= this.gameObject;
        }

        #endif

        public int Key => m_Key;

        [ShowInInspector]
        [ReadOnly]
        [LabelText("显隐")]
        public bool Show { get; private set; }

        [ShowInInspector]
        [ReadOnly]
        [LabelText("数量")]
        public int Count { get; private set; }

        private void Awake()
        {
            if (m_Key <= 0)
            {
                ResetState();
                return;
            }

            BindRedDotKey(m_Key);
        }

        private void OnDestroy()
        {
            if (m_Key <= 0) return;
            RemoveKeyListener(m_Key);
        }

        private void BindRedDotKey(int key)
        {
            RemoveKeyListener(m_Key);
            if (key <= 0)
            {
                m_Key = 0;
                Refresh(0);
                return;
            }

            m_Key = key;
            RedDotMgr.Inst?.AddChanged(m_Key, Refresh);
        }

        //刷新可视化数据
        private void Refresh(int count)
        {
            Count = count > 0 ? count : 0;
            Show = Count >= 1;
            this.m_TargetGameObject.SetActive(Show);
            if (Show) //如果不显示会跳过修改文本,可能会出现隐藏的时候文本不是0的情况.属于正常 都隐藏了没必要去修改文本 浪费
            {
                ChangeText();
            }
        }

        //子类实现
        protected virtual void ChangeText()
        {
        }

        //需要动态改变的,可以预制时,选无
        //然后调用这个方法动态的修改
        public void ChangeBind(int key, bool force = false)
        {
            if (key <= 0)
            {
                Logger.LogErrorContext(this.gameObject, $"修改为 {key} ,尝试修改的Key 错误 不可能 <= 0  {(m_Key > 0 ? $"如果就是想 {m_Key} 改成 0 请调用 ResetState" : "")}");
                return;
            }

            if (!force && m_Key > 0)
            {
                //原则上已经有监听了不允许修改 要修改必须强制修改
                Logger.LogErrorContext(this.gameObject, $"已经有监听的红点 {m_Key} ,想修改为 {key} ,但是当前是非强制修改 所以禁止修改 请检查原因");
                return;
            }

            BindRedDotKey(key);
        }

        //接口的强制绑定
        public void BindKey(int key)
        {
            ChangeBind(key, true);
        }

        //跳过红点枚举,直接设定红点显示. 自定义显示数量.
        //特殊情况下使用
        public void SetManualCount(int count)
        {
            RemoveKeyListener(m_Key);
            Refresh(count);
        }

        //重置状态
        public void ResetState()
        {
            RemoveKeyListener(m_Key);
            Refresh(0);
            m_Key = 0;
        }

        //给接口用的 不是显隐对象是挂载脚本对象
        public GameObject GetOwnerGameObject()
        {
            return gameObject;
        }

        //移除监听
        private void RemoveKeyListener(int key)
        {
            if (key <= 0) return;
            if (YIUISingletonHelper.Disposing)
            {
                return;
            }

            RedDotMgr.Inst?.RemoveChanged(key, Refresh);
        }
    }
}