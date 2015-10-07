namespace System.Linq.Expressions
{
    public static class FilterExpressionExtensions
    {
        public static Expression<Func<TEntity, bool>> GetFilter<TEntity>(this IFilterExpression<TEntity> filter)
        {
            return filter.GetFilterExpressions().DefaultIfEmpty(GetDefaultExpression<TEntity>()).Combine(Expression.AndAlso);
        }

        public static Expression<Func<TEntity, bool>> GetAltFilter<TEntity>(this IAltFilterExpression<TEntity> filter)
        {
            return filter.GetFilterExpressions().DefaultIfEmpty(GetDefaultExpression<TEntity>()).Combine(Expression.AndAlso);
        }

        private static Expression<Func<TEntity, bool>> GetDefaultExpression<TEntity>()
        {
            Type entityType = typeof (TEntity);

            return Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(true), Expression.Parameter(entityType, entityType.Name));
        }
    }
}
