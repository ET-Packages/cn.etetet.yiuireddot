#if YIUI
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YIUIFramework.Editor.MCP
{
    public static class YIUIMCPRedDotDebugMode
    {
        public const int Basic = 0; // 红点调试基础模式，只返回当前节点基础状态。
        public const int Full = 1; // 红点调试完整模式，额外返回堆栈明细。
    }

    [HideLabel]
    [HideReferenceObjectPicker]
    public class GetRedDotDebugInfoParams : YIUIMCPBaseParams
    {
        [LabelText("红点Key")]
        public int key;

        [LabelText("调试模式")]
        [InfoBox("0 = Basic, 1 = Full")]
        public int mode = YIUIMCPRedDotDebugMode.Basic;
    }

    [HideLabel]
    [HideReferenceObjectPicker]
    public class SetRedDotCountParams : YIUIMCPBaseParams
    {
        [LabelText("红点Key")]
        public int key;

        [LabelText("红点数量")]
        public int count;
    }

    [YIUIMCPTools("GetRedDotDebugInfo", "获取红点调试信息")]
    public class YIUIMCPTools_GetRedDotDebugInfo : YIUIMCPBaseExecutor<GetRedDotDebugInfoParams>
    {
        protected override Task<YIUIMCPResult> Run(GetRedDotDebugInfoParams data)
        {
            if (!YIUIMCPRedDotToolHelper.TryGetPlayableRedDotData(data.key, out var redDotMgr, out var redDotData, out var failureResult))
            {
                return Task.FromResult(failureResult);
            }

            if (data.mode != YIUIMCPRedDotDebugMode.Basic && data.mode != YIUIMCPRedDotDebugMode.Full)
            {
                return Task.FromResult(YIUIMCPResult.FailureLog($"GetRedDotDebugInfo, 参数错误: mode={data.mode} 仅支持 0(Basic) 或 1(Full)"));
            }

            var result = YIUIMCPRedDotToolHelper.BuildBaseDebugInfo(redDotMgr, redDotData);
            result["mode"] = data.mode;

            if (data.mode == YIUIMCPRedDotDebugMode.Full)
            {
                var stacks = new List<Dictionary<string, object>>();
                foreach (var stack in redDotData.StackList)
                {
                    if (stack == null)
                    {
                        continue;
                    }

                    stacks.Add(new Dictionary<string, object>
                    {
                        ["id"] = stack.Id,
                        ["time"] = stack.GetTime(),
                        ["os"] = stack.GetOS(redDotData),
                        ["source"] = stack.GetSource(),
                        ["content"] = stack.GetStackContent(),
                    });
                }

                result["stacks"] = stacks;
            }

            return Task.FromResult(YIUIMCPResult.Success(JsonConvert.SerializeObject(result, Formatting.Indented)));
        }
    }

    [YIUIMCPTools("SetRedDotCount", "设置红点数量")]
    public class YIUIMCPTools_SetRedDotCount : YIUIMCPBaseExecutor<SetRedDotCountParams>
    {
        protected override Task<YIUIMCPResult> Run(SetRedDotCountParams data)
        {
            if (!YIUIMCPRedDotToolHelper.TryGetPlayableRedDotData(data.key, out var redDotMgr, out var redDotData, out var failureResult))
            {
                return Task.FromResult(failureResult);
            }

            var beforeCount = redDotData.Count;
            var beforeRealCount = redDotData.RealCount;
            var setResult = redDotMgr.SetCount(data.key, data.count);
            redDotData = redDotMgr.GetData(data.key);
            if (redDotData == null)
            {
                return Task.FromResult(YIUIMCPResult.FailureLog($"SetRedDotCount, 设置后重新获取红点失败: key={data.key}"));
            }

            var result = new Dictionary<string, object>
            {
                ["key"] = redDotData.Key,
                ["desc"] = redDotMgr.GetKeyDes(redDotData.Key),
                ["beforeCount"] = beforeCount,
                ["beforeRealCount"] = beforeRealCount,
                ["afterCount"] = redDotData.Count,
                ["afterRealCount"] = redDotData.RealCount,
                ["tips"] = redDotData.Tips,
                ["switchTips"] = redDotData.Config?.SwitchTips ?? false,
                ["setResult"] = setResult,
            };

            return Task.FromResult(YIUIMCPResult.Success(JsonConvert.SerializeObject(result, Formatting.Indented)));
        }
    }

    internal static class YIUIMCPRedDotToolHelper
    {
        internal static bool TryGetPlayableRedDotData(int key,
                                                      out RedDotMgr redDotMgr,
                                                      out RedDotData redDotData,
                                                      out YIUIMCPResult failureResult)
        {
            redDotMgr = null;
            redDotData = null;
            failureResult = default;

            if (!Application.isPlaying)
            {
                failureResult = YIUIMCPResult.FailureLog("红点调试工具仅允许在 PlayMode 下执行");
                return false;
            }

            if (key <= 0)
            {
                failureResult = YIUIMCPResult.FailureLog($"红点Key无效: {key}");
                return false;
            }

            if (!RedDotMgr.Exist)
            {
                failureResult = YIUIMCPResult.FailureLog("RedDotMgr 未初始化，当前无法获取红点调试信息");
                return false;
            }

            redDotMgr = RedDotMgr.Inst;
            if (redDotMgr == null || redDotMgr.Disposed)
            {
                failureResult = YIUIMCPResult.FailureLog("RedDotMgr 已释放或不可用");
                return false;
            }

            if (!redDotMgr.InitedSucceed)
            {
                failureResult = YIUIMCPResult.FailureLog("RedDotMgr 尚未完成初始化");
                return false;
            }

            if (!redDotMgr.CheckRedDotKey(key))
            {
                failureResult = YIUIMCPResult.FailureLog($"红点Key不存在: {key}");
                return false;
            }

            redDotData = redDotMgr.GetData(key);
            if (redDotData == null)
            {
                failureResult = YIUIMCPResult.FailureLog($"获取红点数据失败: key={key}");
                return false;
            }

            return true;
        }

        internal static Dictionary<string, object> BuildBaseDebugInfo(RedDotMgr redDotMgr, RedDotData redDotData)
        {
            var parentKeys = new List<int>();
            foreach (var parentData in redDotData.ParentList)
            {
                if (parentData == null)
                {
                    continue;
                }

                parentKeys.Add(parentData.Key);
            }

            var childKeys = new List<int>();
            foreach (var childData in redDotData.ChildList)
            {
                if (childData == null)
                {
                    continue;
                }

                childKeys.Add(childData.Key);
            }

            return new Dictionary<string, object>
            {
                ["key"] = redDotData.Key,
                ["desc"] = redDotMgr.GetKeyDes(redDotData.Key),
                ["exists"] = true,
                ["count"] = redDotData.Count,
                ["realCount"] = redDotData.RealCount,
                ["tips"] = redDotData.Tips,
                ["switchTips"] = redDotData.Config?.SwitchTips ?? false,
                ["parentKeys"] = parentKeys,
                ["childKeys"] = childKeys,
                ["parentCount"] = parentKeys.Count,
                ["childCount"] = childKeys.Count,
                ["stackCount"] = redDotData.StackList?.Count ?? 0,
            };
        }
    }
}
#endif
