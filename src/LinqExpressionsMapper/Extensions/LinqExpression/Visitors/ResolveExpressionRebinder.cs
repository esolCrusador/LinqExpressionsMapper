using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqExpressionsMapper.Extensions.LinqExpression.Visitors
{
    internal class ResolveExpressionRebinder: ExpressionVisitor
    {
        private static readonly Type ResolverType = typeof (ExpressionResolvingExtensions);
        private static readonly string ResolveMethodName = "InitFrom";
        private static readonly MethodInfo GenericSelectMethodInfo = typeof (Enumerable).GetMethods().First(m => m.Name == "Select" && m.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2);

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == ResolverType && node.Method.Name == ResolveMethodName)
            {
                // node.Arguments[0] is expression which gets expression required for member init.
                Func<LambdaExpression> expressionFactory = Expression.Lambda<Func<LambdaExpression>>(node.Arguments[0]).Compile();
                LambdaExpression selectExpression = expressionFactory();

                Type[] genericArguments = node.Method.GetGenericArguments();
                //IEnumerable
                if (genericArguments.Length == 3)
                {
                    MethodInfo selectMethodInfo = GenericSelectMethodInfo.MakeGenericMethod(genericArguments[0], genericArguments[1]);
                    return Expression.Call(null, selectMethodInfo, node.Arguments[1], selectExpression);
                }
                else
                {
                    return node.Arguments[1].Continue(selectExpression);
                }
            }
            return base.VisitMethodCall(node);
        }
    }
}
