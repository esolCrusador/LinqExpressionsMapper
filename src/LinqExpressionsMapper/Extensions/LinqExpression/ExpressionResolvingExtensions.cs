using System.Collections.Generic;
using System.Linq.Expressions;
using LinqExpressionsMapper.Extensions.LinqExpression.Visitors;

namespace System.Linq
{
    public static class ExpressionResolvingExtensions
    {
        /// <summary>
        /// This method is Expression injection point. When ApplyExpressions() is called expression placeholder is replaced with expression body.
        /// </summary>
        /// <typeparam name="TSource">Source param type.</typeparam>
        /// <typeparam name="TDest">Destanation param type.</typeparam>
        /// <param name="expression">Target expression.</param>
        /// <param name="source">Expression parameter.</param>
        /// <returns>Default destanation.</returns>
        public static TDest Invoke<TSource, TDest>(this Expression<Func<TSource, TDest>> expression, TSource source)
        {
            return default(TDest);
        }

        /// <summary>
        /// This method is Expression injection point for Enumerable. When ApplyExpressions() is called expression placeholder is replaced with expression body.
        /// </summary>
        /// <typeparam name="TSource">Source param type.</typeparam>
        /// <typeparam name="TDest">Destanation param type.</typeparam>
        /// <typeparam name="TSourceEnumerable">Enumerable child type.</typeparam>
        /// <param name="expression">Target expression.</param>
        /// <param name="source">Expression parameter.</param>
        /// <returns>Default destanation enumerable.</returns>
        public static IEnumerable<TDest> InvokeEnumerable<TSource, TDest, TSourceEnumerable>(this Expression<Func<TSource, TDest>> expression, TSourceEnumerable source)
            where TSourceEnumerable : IEnumerable<TSource>
        {
            return default(IEnumerable<TDest>);
        }

        /// <summary>
        /// Replaces Invoke(), InvokeEnumerable() injection points with expressions bodies.
        /// </summary>
        /// <typeparam name="TSource">Source expression param type.</typeparam>
        /// <typeparam name="TDest">Destanation expression param type.</typeparam>
        /// <param name="expression">Target expression.</param>
        /// <returns>Expression with replaced placeholders.</returns>
        public static Expression<Func<TSource, TDest>> ApplyExpressions<TSource, TDest>(this Expression<Func<TSource, TDest>> expression)
        {
            ResolveExpressionRebinder rebiner = new ResolveExpressionRebinder();

            return Expression.Lambda<Func<TSource, TDest>>(rebiner.Visit(expression.Body), expression.Parameters[0]);
        }
    }
}
