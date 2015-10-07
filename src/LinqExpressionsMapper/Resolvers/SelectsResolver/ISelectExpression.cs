namespace System.Linq.Expressions
{
    public interface ISelectExpression<TSource, TDest>
    {
        Expression<Func<TSource, TDest>> GetSelectExpression();
    }
}
