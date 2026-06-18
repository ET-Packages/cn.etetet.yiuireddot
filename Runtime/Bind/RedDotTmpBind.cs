#if TextMeshPro
using TMPro;
using UnityEngine;
using Sirenix.OdinInspector;

namespace YIUIFramework
{
    [AddComponentMenu("YIUIFramework/红点/红点TMP绑定 【RedDotTextBind】")]
    public class RedDotTmpBind : RedDotBind
    {
        [PropertyOrder(int.MinValue)]
        [SerializeField]
        [LabelText("文本")]
        [EnableIf("@UIOperationHelper.CommonShowIf()")]
        [Required("必须设置红点文本对象！")]
        private TextMeshProUGUI m_Text;

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
#endif