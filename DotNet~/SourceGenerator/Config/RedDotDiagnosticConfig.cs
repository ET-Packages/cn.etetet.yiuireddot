namespace ET
{
    public static class RedDotDiagnosticCategories
    {
        public const string RedDot = "YIUIRedDotAnalyzers"; // 红点分析器诊断分类名称
    }

    public static class RedDotDiagnosticIds
    {
        public const string RedDotKeyLiteralAnalyzerRuleId = "YIUIRD0001"; // 红点 key 参数数字字面量分析器规则 ID
    }

    public static class RedDotAnalyzerDefinitions
    {
        public const string RedDotMgrTypeName = "YIUIFramework.RedDotMgr"; // 红点管理器完整类型名

        public const string CheckRedDotKeyMethodName = "CheckRedDotKey"; // 检查红点 key 是否存在的方法名

        public const string GetDataMethodName = "GetData"; // 获取红点数据的方法名

        public const string AddChangedMethodName = "AddChanged"; // 添加红点变化监听的方法名

        public const string RemoveChangedMethodName = "RemoveChanged"; // 移除红点变化监听的方法名

        public const string SetCountMethodName = "SetCount"; // 设置红点数量的方法名

        public const string GetCountMethodName = "GetCount"; // 获取红点数量的方法名

        public const string SetTipsMethodName = "SetTips"; // 设置红点提示开关的方法名

        public const string DeletePlayerTipsPrefsMethodName = "DeletePlayerTipsPrefs"; // 删除玩家红点提示偏好方法名

        public const string BindDynamicRedDotByKeyMethodName = "BindDynamicRedDotByKey"; // 按 key 动态绑定红点的方法名

        public const string KeyParameterName = "key"; // 红点 API 的 key 参数名称
    }
}
