#if UNITY_EDITOR
namespace YIUIFramework.Editor
{
    internal class UICreateRedDotShowCode : BaseTemplate
    {
        public override string EventName => "红点系统 动态预制枚举自动生成";

        private readonly bool m_AutoRefresh;
        public override bool AutoRefresh => m_AutoRefresh;

        private readonly bool m_ShowTips;
        public override bool ShowTips => m_ShowTips;

        public UICreateRedDotShowCode(out bool result, string authorName, UICreateRedDotShowData codeData) : base(authorName)
        {
            var path = $"../{codeData.ClassPath}";
            var templatePath = "Assets/../Packages/cn.etetet.yiuireddot/Editor/RedDot/Template";
            var template = $"{templatePath}/UICreateRedDotShowTemplate.txt";
            CreateVo = new CreateVo(template, path);

            m_AutoRefresh = codeData.AutoRefresh;
            m_ShowTips = codeData.ShowTips;
            ValueDic["Content"] = codeData.Content;

            result = CreateNewFile();
        }
    }
}
#endif
