using LinqExpressionsMapper.Extensions.LinqExpression.Visitors;

namespace System.Linq.Expressions
{
    public static class ContinuationsExpressionExtensions
    {
        public static Expression<Func<TSource, TResult>> Continue<TSource, TItem, TResult>(this Expression<Func<TSource, TItem>> sourceExpression, Expression<Func<TItem, TResult>> continueExpression)
        {
            ExpressionVisitor rebinder;

            switch (sourceExpression.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    {
                        rebinder = new MemberRebinder(continueExpression.Parameters[0], (MemberExpression)sourceExpression.Body);
                        break;
                    }
                case ExpressionType.Parameter:
                    {
                        rebinder = new ParameterRebinder(continueExpression.Parameters[0], (ParameterExpression)sourceExpression.Body);
                        break;
                    }
                default:
                    rebinder = new ExpressionRebinder(continueExpression.Parameters[0], sourceExpression.Body);
                    break;
            }

            Expression resultBody = rebinder.Visit(continueExpression.Body);

            return Expression.Lambda<Func<TSource, TResult>>(resultBody, sourceExpression.Parameters);
        }

        public static Expression Continue(this Expression sourceBody, Expression continueBody, ParameterExpression continueParameter)
        {
            var rebinder = new ExpressionRebinder(continueParameter, sourceBody);

            return rebinder.Visit(continueBody);
        }

        public static Expression Continue(this Expression sourceBody, LambdaExpression continueExpression)
        {
            return Continue(sourceBody, continueExpression.Body, continueExpression.Parameters[0]);
        }

        public static Expression<Func<TSource, bool>> Not<TSource>(this Expression<Func<TSource, bool>> sourceExpression)
        {
            return Expression.Lambda<Func<TSource, bool>>(Expression.Not(sourceExpression.Body), sourceExpression.Parameters);
        }

        public static Expression<Func<TModel, bool>> EqualsExpression<TModel, TField>(this Expression<Func<TModel, TField>> memberExpression, TField value)
        {
            return Expression.Lambda<Func<TModel, bool>>(Expression.Equal(memberExpression.Body, Expression.Constant(value)), memberExpression.Parameters);
        }

        public static Expression<Func<TModel, TValue>> ConvertExpression<TModel, TField, TValue>(this Expression<Func<TModel, TField>> memberExpression)
        {
            return Expression.Lambda<Func<TModel, TValue>>(Expression.Convert(memberExpression.Body, typeof(TValue)), memberExpression.Parameters);
        }
    }
}
