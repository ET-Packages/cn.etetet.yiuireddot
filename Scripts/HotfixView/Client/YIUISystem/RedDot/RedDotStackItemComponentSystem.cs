using System;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    [FriendOf(typeof(RedDotStackItemComponent))]
    public static partial class RedDotStackItemComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this RedDotStackItemComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this RedDotStackItemComponent self)
        {
        }

        public static void RefreshData(this RedDotStackItemComponent self, RedDotStack data, RedDotData infoData)
        {
            self.u_DataId.SetValue(data.Id);
            self.u_DataTime.SetValue(data.GetTime());
            self.u_DataOs.SetValue(data.GetOS(infoData));
            self.u_DataSource.SetValue(data.GetSource());
            self.u_DataShowStack.SetValue(false);
            self.m_RedDotStackData = data;
        }

        #region YIUIEvent开始

        [YIUIInvoke(RedDotStackItemComponent.OnEventShowStackInvoke)]
        private static void OnEventShowStackInvoke(this RedDotStackItemComponent self)
        {
            self.u_ComStackText.text = self.RedDotStackData?.GetStackContent() ?? "";
        }

        #endregion YIUIEvent结束
    }
}
