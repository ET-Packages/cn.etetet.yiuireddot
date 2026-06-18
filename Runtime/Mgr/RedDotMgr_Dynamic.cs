using ET;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 动态绑定红点显示
    /// 优点就是可以AI可以介入,不改UI
    /// 缺点没有UI绑定灵活.那个可以随时修改不需要程序接入
    /// </summary>
    public partial class RedDotMgr
    {
        /// <summary>
        /// 常用的直接默认用RedDot
        /// key = ERedDotKeyType 常量
        /// </summary>
        public async ETTask<GameObject> BindDynamicRedDotByKey(Transform parent,
                                                               int key,
                                                               ERadDotShowType showType = ERadDotShowType.RedDot,
                                                               ERedDotAnchor anchor = ERedDotAnchor.RightTop)
        {
            return await BindDynamicRedDotByKey(parent, showType, key, anchor);
        }

        /// <summary>
        /// 根据红点key 自动显示红点信息
        /// key = ERedDotKeyType 常量
        /// </summary>
        public async ETTask<GameObject> BindDynamicRedDotByKey(Transform parent,
                                                               ERadDotShowType showType,
                                                               int key,
                                                               ERedDotAnchor anchor = ERedDotAnchor.RightTop)
        {
            if (key <= 0)
            {
                Debug.LogError($"动态红点绑定失败 key 非法: {key}");
                return null;
            }

            if (!CheckRedDotKey(key))
            {
                Debug.LogError($"动态红点绑定失败 key 非法: {key} 不存在");
                return null;
            }

            return await this.BindDynamicRedDotInternal(parent, showType, anchor, control => control.BindKey(key));
        }

        /// <summary>
        /// 自定义红点数量 count >=1 否则 0 根本不需要显示
        /// 特殊情况下使用的, 常规情况请使用 BindDynamicRedDotByKey
        /// </summary>
        public async ETTask<GameObject> BindDynamicRedDotManual(Transform parent,
                                                                ERadDotShowType showType,
                                                                int count,
                                                                ERedDotAnchor anchor = ERedDotAnchor.RightTop)
        {
            if (count <= 0)
            {
                Debug.LogError($"自定义显示红点必须 ≥ 1, 否则根本没有显示必要, 如果加载完毕要隐藏直接调用 RemoveDynamicRedDot");
                return null;
            }

            return await this.BindDynamicRedDotInternal(parent, showType, anchor, control => control.SetManualCount(count));
        }

        /// <summary>
        /// 要隐藏已加载的对象就用这个
        /// </summary>
        public bool RemoveDynamicRedDot(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return false;
            }

            if (YIUISingletonHelper.IsQuitting)
            {
                return false;
            }

            var control = gameObject.GetComponent<IDynamicRedDotControl>();
            if (control == null)
            {
                Logger.LogErrorContext(gameObject, $"{gameObject.name} 回收的对象不是 红点, 请检查是否传错参数");
            }
            else
            {
                control.ResetState();
            }

            return YIUIGameObjectPool.Inst?.Put(gameObject) == true;
        }

        private string GetDynamicRedDotResName(ERadDotShowType showType)
        {
            if (showType == ERadDotShowType.None)
            {
                return string.Empty;
            }

            var enumName = System.Enum.GetName(typeof(ERadDotShowType), showType);
            if (string.IsNullOrEmpty(enumName))
            {
                return string.Empty;
            }

            if (!m_AllRedDotShowData.TryGetValue(enumName, out var showData) || showData == null)
            {
                return string.Empty;
            }

            return showData.ResName;
        }

        private async ETTask<GameObject> BindDynamicRedDotInternal(Transform parent,
                                                                   ERadDotShowType showType,
                                                                   ERedDotAnchor anchor,
                                                                   System.Action<IDynamicRedDotControl> bindAction)
        {
            if (parent == null)
            {
                Debug.LogError("动态红点绑定失败 parent 为空");
                return null;
            }

            var resName = this.GetDynamicRedDotResName(showType);
            if (string.IsNullOrEmpty(resName))
            {
                Debug.LogError($"动态红点绑定失败 未配置展示类型资源: {showType}");
                return null;
            }

            var gameObject = await YIUIGameObjectPool.Inst.Get(resName, parent);
            if (gameObject == null)
            {
                Debug.LogError($"动态红点创建失败 无法加载资源: {resName}");
                return null;
            }

            if (parent == null)
            {
                if (YIUISingletonHelper.IsQuitting)
                {
                    return null;
                }

                YIUIGameObjectPool.Inst?.Put(gameObject);
                return null;
            }

            ApplyDynamicAnchor(gameObject.GetComponent<RectTransform>(), anchor);

            var control = gameObject.GetComponent<IDynamicRedDotControl>();
            if (control == null)
            {
                Logger.LogErrorContext(gameObject, $"动态红点创建失败 资源未挂载 IDynamicRedDotControl: {resName}");
                YIUIGameObjectPool.Inst?.Put(gameObject);
                return null;
            }

            control.ResetState();
            bindAction?.Invoke(control);
            return gameObject;
        }

        //如果当前几个方式都不合适 可以尝试修改对应的父级 相对位置来达到需求 不要扩展这个方法 不然就无穷无尽了
        private void ApplyDynamicAnchor(RectTransform rectTransform, ERedDotAnchor anchor)
        {
            if (rectTransform == null)
            {
                return;
            }

            var anchorValue = anchor switch
            {
                ERedDotAnchor.Center => new Vector2(0.5f, 0.5f),
                ERedDotAnchor.LeftTop => new Vector2(0f, 1f),
                ERedDotAnchor.Top => new Vector2(0.5f, 1f),
                ERedDotAnchor.RightTop => new Vector2(1f, 1f),
                ERedDotAnchor.Left => new Vector2(0f, 0.5f),
                ERedDotAnchor.Right => new Vector2(1f, 0.5f),
                ERedDotAnchor.LeftBottom => new Vector2(0f, 0f),
                ERedDotAnchor.Bottom => new Vector2(0.5f, 0f),
                ERedDotAnchor.RightBottom => new Vector2(1f, 0f),
                _ => new Vector2(1f, 1f),
            };

            rectTransform.anchorMin = anchorValue;
            rectTransform.anchorMax = anchorValue;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}