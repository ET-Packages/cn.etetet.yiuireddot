using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace YIUIFramework
{
    [AddComponentMenu("YIUIFramework/红点/红点Text绑定 【RedDotTextBind】")]
    public class RedDotTextBind : RedDotBind
    {
        [PropertyOrder(int.MinValue)]
        [SerializeField]
        [LabelText("文本")]
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        [Required("必须设置红点文本对象！")]
        private Text m_Text;

        protected override void ChangeText()
        {
            if (m_Text != null)
            {
                m_Text.text = Count.ToString();
            }
            else
            {
                Logger.LogErrorContext(gameObject, $"{gameObject.name} 使用了文本显示,但是没有找到文本组件");
            }
        }
    }
}