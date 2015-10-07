using LinqExpressionsMapper.Extensions.LinqExpression.Visitors;

namespace System.Linq.Expressions
{
    public static class ParameterReplacementExtensions
    {
        public static Expression<Func<TSource, TResult>> ReplaceParameter<TSource, TResult>(this Expression<Func<TSource, TResult>> sourceExpression, ParameterExpression parameter)
        {
            return Expression.Lambda<Func<TSource, TResult>>(ReplaceParameter(sourceExpression.Body, sourceExpression.Parameters[0], parameter), parameter);
        }

        public static Expression<Func<TNewSource, TResult>> CastParameter<TSource, TNewSource, TResult>(this Expression<Func<TSource, TResult>> sourceExpression)
            where TNewSource : TSource
        {
            ParameterExpression sourceParameter = sourceExpression.Parameters[0];
            ParameterExpression parameter = Expression.Parameter(typeof(TNewSource), sourceParameter.Name);

            return Expression.Lambda<Func<TNewSource, TResult>>(ReplaceParameter(sourceExpression.Body, sourceParameter, parameter), parameter);
        }

        public static Expression<Func<TSecondSource, TResult>> ReplaceParameter<TSource, TSecondSource, TResult>(this Expression<Func<TSource, TResult>> sourceExpression, ParameterExpression parameter)
        {
            return Expression.Lambda<Func<TSecondSource, TResult>>(ReplaceParameter(sourceExpression.Body, sourceExpression.Parameters[0], parameter), parameter);
        }

        public static Expression<Func<TSecondSource, TResult>> ReplaceParameter<TSource, TSecondSource, TResult>(this Expression<Func<TSource, TResult>> sourceExpression, Expression<Func<TSecondSource, TSource>> member)
        {
            ExpressionVisitor rebinder = new MemberRebinder(sourceExpression.Parameters[0], (MemberExpression)member.Body);

            Expression resultBody = rebinder.Visit(sourceExpression.Body);

            return Expression.Lambda<Func<TSecondSource, TResult>>(resultBody, member.Parameters[0]);
        }

        internal static Expression ReplaceParameter(this Expression sourceExpression, ParameterExpression replaceParam, ParameterExpression parameter)
        {
            ExpressionVisitor rebinder = new ParameterRebinder(replaceParam, parameter);

            return rebinder.Visit(sourceExpression);
        }
    }
}
