using System.Globalization;

namespace System.Linq.Expressions
{
    public interface ICultureSelectExpression<TSource, TDest>
    {
        Expression<Func<TSource, TDest>> GetSelectExpression(Culture cultureId);
    }
}
