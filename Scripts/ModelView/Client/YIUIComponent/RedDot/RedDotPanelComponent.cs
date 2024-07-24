using System.Collections.Generic;
using TMPro;
using YIUIFramework;

namespace ET.Client
{
    public partial class RedDotPanelComponent : Entity, IDynamicEvent<OnClickParentListEvent>, IDynamicEvent<OnClickChildListEvent>,
            IDynamicEvent<OnClickItemEvent>
    {
        public YIUILoopScroll<RedDotData, RedDotDataItemComponent>   m_SearchScroll;
        public List<RedDotData>                                      m_CurrentDataList      = new();
        public Dictionary<int, int>                                  m_AllDropdownSearchDic = new();
        public List<TMP_Dropdown.OptionData>                         m_DropdownOptionData   = new();
        public RedDotData                                            m_InfoData;
        public YIUILoopScroll<RedDotStack, RedDotStackItemComponent> m_StackScroll;
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