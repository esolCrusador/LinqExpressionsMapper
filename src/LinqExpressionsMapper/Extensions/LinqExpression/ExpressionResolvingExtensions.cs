using System.Collections.Generic;
using System.Linq.Expressions;
using LinqExpressionsMapper.Extensions.LinqExpression.Visitors;

namespace System.Linq
{
    public static class ExpressionResolvingExtensions
    {
        public static TDest Invoke<TSource, TDest>(this Expression<Func<TSource, TDest>> expression, TSource source)
        {
            return default(TDest);
        }

        public static IEnumerable<TDest> InvokeEnumerable<TSource, TDest, TSourceEnumerable>(this Expression<Func<TSource, TDest>> expression, TSourceEnumerable source)
            where TSourceEnumerable : IEnumerable<TSource>
        {
            return default(IEnumerable<TDest>);
        }

        public static Expression<Func<TSource, TDest>> ApplyExpressions<TSource, TDest>(this Expression<Func<TSource, TDest>> expression)
        {
            ResolveExpressionRebinder rebiner = new ResolveExpressionRebinder();

            return Expression.Lambda<Func<TSource, TDest>>(rebiner.Visit(expression.Body), expression.Parameters[0]);
        }
    }
}
