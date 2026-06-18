#if UNITY_EDITOR
using System.Collections.Generic;

namespace YIUIFramework.Editor
{
    internal static class UICreateRedDotShowGet
    {
        private const string EnumContent = @"
        [LabelText(""{0}"")]
        {1} = {2},
";

        public static string Get(List<UIRedDotShowEditorData> dataList)
        {
            var sb = SbPool.Get();
            for (var i = 0; i < dataList.Count; i++)
            {
                var data = dataList[i];
                var des = string.IsNullOrEmpty(data.Des) ? data.EnumName : data.Des;
                sb.AppendFormat(EnumContent, des, data.EnumName, i + 1);
            }

            return SbPool.PutAndToStr(sb);
        }
    }
}
#endif
