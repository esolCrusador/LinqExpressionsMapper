using System;
using System.Globalization;
using System.Linq.Expressions;

namespace GloryS.Common.Resolvers.SelectsResolver
{
    internal interface ISelectResolver
    {
        void Register<TSource, TDest>(ISelectExpression<TSource, TDest> resolver);

        void Register<TSource, TDest>(ISelectExpressionNonCache<TSource, TDest> resolver);

        void Register<TSource, TDest>(ICultureSelectExpression<TSource, TDest> resolver);

        Expression<Func<TSource, TDest>> GetExternalExpression<TSource, TDest>();

        Expression<Func<TSource, TDest>> GetExternalExpression<TSource, TDest>(Culture culture);

        Expression<Func<TSource, TDest>> GetExpression<TSource, TDest>() where TDest : ISelectExpression<TSource, TDest>, new();

        Expression<Func<TSource, TDest>> GetExpression<TSource, TDest>(Culture culture) where TDest : ICultureSelectExpression<TSource, TDest>, new();
    }
}
