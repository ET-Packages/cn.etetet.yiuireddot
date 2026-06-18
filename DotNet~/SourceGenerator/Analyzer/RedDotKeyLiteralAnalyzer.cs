using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace ET
{
    public static class RedDotKeyLiteralAnalyzerRule
    {
        private const string Title = "红点 key 参数必须使用 ERedDotKeyType 常量"; // 诊断标题

        private const string MessageFormat = "红点 API 的 key 参数禁止手写数字，固定 key 必须使用 ERedDotKeyType 常量，当前传入: {0}"; // 诊断消息格式

        private const string Description = "红点 API 禁止直接传入 int 字面量作为 key；固定 key 必须使用 ERedDotKeyType 常量，传递过来的 key 参数可继续透传。"; // 诊断详细描述

        public static readonly DiagnosticDescriptor Rule =
                new(RedDotDiagnosticIds.RedDotKeyLiteralAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    RedDotDiagnosticCategories.RedDot,
                    DiagnosticSeverity.Error,
                    true,
                    Description);
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedDotKeyLiteralAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
                ImmutableArray.Create(RedDotKeyLiteralAnalyzerRule.Rule);

        public override void Initialize(AnalysisContext context)
        {
            if (!AnalyzerGlobalSetting.EnableAnalyzer)
            {
                return;
            }

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocationExpression, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not InvocationExpressionSyntax invocationExpression)
            {
                return;
            }

            if (context.SemanticModel.GetSymbolInfo(invocationExpression, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol)
            {
                return;
            }

            if (!IsTargetMethod(methodSymbol))
            {
                return;
            }

            var keyArgument = GetKeyArgument(context.SemanticModel, invocationExpression);
            if (keyArgument == null)
            {
                return;
            }

            var keyExpression = keyArgument.Value.Syntax;
            if (!ContainsNumberLiteral(keyExpression))
            {
                return;
            }

            var diagnostic = Diagnostic.Create(RedDotKeyLiteralAnalyzerRule.Rule,
                keyExpression.GetLocation(),
                keyExpression.ToString());
            context.ReportDiagnostic(diagnostic);
        }

        private static bool IsTargetMethod(IMethodSymbol methodSymbol)
        {
            return methodSymbol.ContainingType?.ToString() == RedDotAnalyzerDefinitions.RedDotMgrTypeName
                && HasKeyParameter(methodSymbol)
                && IsRedDotKeyApi(methodSymbol.Name);
        }

        private static bool HasKeyParameter(IMethodSymbol methodSymbol)
        {
            foreach (var parameter in methodSymbol.Parameters)
            {
                if (parameter.Name == RedDotAnalyzerDefinitions.KeyParameterName)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsRedDotKeyApi(string methodName)
        {
            return methodName == RedDotAnalyzerDefinitions.CheckRedDotKeyMethodName
                || methodName == RedDotAnalyzerDefinitions.GetDataMethodName
                || methodName == RedDotAnalyzerDefinitions.AddChangedMethodName
                || methodName == RedDotAnalyzerDefinitions.RemoveChangedMethodName
                || methodName == RedDotAnalyzerDefinitions.SetCountMethodName
                || methodName == RedDotAnalyzerDefinitions.GetCountMethodName
                || methodName == RedDotAnalyzerDefinitions.SetTipsMethodName
                || methodName == RedDotAnalyzerDefinitions.DeletePlayerTipsPrefsMethodName
                || methodName == RedDotAnalyzerDefinitions.BindDynamicRedDotByKeyMethodName;
        }

        private static IArgumentOperation? GetKeyArgument(SemanticModel semanticModel, InvocationExpressionSyntax invocationExpression)
        {
            if (semanticModel.GetOperation(invocationExpression) is not IInvocationOperation invocationOperation)
            {
                return null;
            }

            foreach (var argument in invocationOperation.Arguments)
            {
                if (argument.Parameter == null)
                {
                    continue;
                }

                if (argument.Parameter.Name == RedDotAnalyzerDefinitions.KeyParameterName)
                {
                    return argument;
                }
            }

            return null;
        }

        private static bool ContainsNumberLiteral(SyntaxNode expression)
        {
            var valueExpression = UnwrapExpression(expression);
            if (valueExpression.IsKind(SyntaxKind.NumericLiteralExpression))
            {
                return true;
            }

            foreach (var descendantNode in valueExpression.DescendantNodes())
            {
                if (descendantNode.IsKind(SyntaxKind.NumericLiteralExpression))
                {
                    return true;
                }
            }

            return false;
        }

        private static SyntaxNode UnwrapExpression(SyntaxNode expression)
        {
            while (true)
            {
                switch (expression)
                {
                    case ParenthesizedExpressionSyntax parenthesizedExpression:
                        expression = parenthesizedExpression.Expression;
                        continue;
                    case CastExpressionSyntax castExpression:
                        expression = castExpression.Expression;
                        continue;
                    default:
                        return expression;
                }
            }
        }
    }
}
