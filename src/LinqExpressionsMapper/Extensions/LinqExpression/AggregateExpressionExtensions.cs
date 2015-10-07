using System.Collections.Generic;
using LinqExpressionsMapper.Extensions.LinqExpression.Visitors;

namespace System.Linq.Expressions
{
    public static class AggregateExpressionExtensions
    {
        public static IEnumerable<Expression<Func<TSource, TResult>>> Concat<TSource, TAltSource, TResult>(this IEnumerable<Expression<Func<TSource, TResult>>> expressions, IEnumerable<Expression<Func<TAltSource, TResult>>> altSource)
            where TSource : TAltSource
        {
            if (typeof(TAltSource) == typeof(TSource))
            {
                return Enumerable.Concat(expressions, altSource.Cast<Expression<Func<TSource, TResult>>>());
            }
            return Enumerable.Concat(expressions, altSource.Select(exp => exp.CastParameter<TAltSource, TSource, TResult>()));
        }

        public static Expression<Func<TSource, TResult>> Combine<TSource, TResult>(this IEnumerable<Expression<Func<TSource, TResult>>> expresions, Func<Expression, Expression, Expression> combineOperator)
        {
            ParameterExpression parameter = null;
            Expression resultBody = null;
            foreach (var expression in expresions)
            {
                if (resultBody == null)
                {
                    resultBody = expression.Body;
                    parameter = expression.Parameters[0];
                }
                else
                {
                    Expression expressionBody = expression.Body;

                    ParameterExpression expressionParameter = expression.Parameters[0];

                    var rebinder = new ParameterRebinder(expressionParameter, parameter);
                    expressionBody = rebinder.Visit(expressionBody);

                    resultBody = combineOperator(resultBody, expressionBody);
                }
            }

            if (resultBody == null)
            {
                throw new ArgumentException("Expressions Enumerable is empty", "expresions");
            }

            return Expression.Lambda<Func<TSource, TResult>>(resultBody, parameter);
        }

        public static Expression<Func<TSource, TResult>> Combine<TSource, TResult>(this IEnumerable<Expression<Func<TSource, TResult>>> expresions, Expression<Func<TResult, TResult, TResult>> combineExpression)
        {
            if (!(combineExpression.Body is BinaryExpression))
            {
                throw new ArgumentException("Combine Expression is not binary expression", "combineExpression");
            }

            var operatorType = combineExpression.Body.NodeType;

            Func<Expression, Expression, BinaryExpression> combineOperator =
                (left, right) =>
                    Expression.MakeBinary(operatorType, left, right);

            return Combine(expresions, combineOperator);
        }

        public static Expression<Func<TSource, TResult>> Join<TSource, TResult>(this IEnumerable<Expression<Func<TSource, TResult>>> expresions, Expression<Func<TResult, TResult, TResult>> combineExpression)
        {
            if (!(combineExpression.Body is MethodCallExpression))
            {
                throw new ArgumentException("Combine Expression is not binary expression", "combineExpression");
            }

            var methodCall = (MethodCallExpression) combineExpression.Body;

            Func<Expression, Expression, MethodCallExpression> combineOperator =
                (left, right) => Expression.Call(methodCall.Method, left, right);

            return Combine(expresions, combineOperator);
        }
    }
}
