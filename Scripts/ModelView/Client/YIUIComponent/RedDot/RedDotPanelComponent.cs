using System.Collections.Generic;
using TMPro;
using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// 文档: https://lib9kmxvq7k.feishu.cn/wiki/XzyawmryHitNVNk9QVtcDAftn5O
    /// </summary>
    public partial class RedDotPanelComponent : Entity, IDynamicEvent<OnClickParentListEvent>, IDynamicEvent<OnClickChildListEvent>,
            IDynamicEvent<OnClickItemEvent>
    {
        public YIUILoopScroll<RedDotData>    m_SearchScroll;
        public List<RedDotData>              m_CurrentDataList      = new();
        public Dictionary<int, int>          m_AllDropdownSearchDic = new();
        public List<TMP_Dropdown.OptionData> m_DropdownOptionData   = new();
        public RedDotData                    m_InfoData;
        public YIUILoopScroll<RedDotStack>   m_StackScroll;
    }

    public struct OnClickParentListEvent
    {
        public RedDotData Data;
    }

    public struct OnClickChildListEvent
    {
        public RedDotData Data;
    }

    public struct OnClickItemEvent
    {
        public RedDotData Data;
    }
}