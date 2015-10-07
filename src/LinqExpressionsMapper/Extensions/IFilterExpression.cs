using System.Collections.Generic;

namespace System.Linq.Expressions
{
    public interface IFilterExpression<TEntity>
    {
        IEnumerable<Expression<Func<TEntity, bool>>> GetFilterExpressions();
    }

    public interface IAltFilterExpression<TEntity>
    {
        IEnumerable<Expression<Func<TEntity, bool>>> GetFilterExpressions();
    }
}
