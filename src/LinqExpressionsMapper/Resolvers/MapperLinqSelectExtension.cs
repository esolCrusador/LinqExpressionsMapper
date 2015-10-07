using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace System.Linq
{
    public static class MapperLinqSelectExtension
    {
        public static IQueryable<TDest> ResolveSelectExternal<TSource, TDest>(this IQueryable<TSource> queryable)
        {
            return queryable.Select(Mapper.GetExternalExpression<TSource, TDest>());
        }

        public static IQueryable<TDest> ResolveSelect<TSource, TDest>(this IQueryable<TSource> queryable)
            where TDest : ISelectExpression<TSource, TDest>, new()
        {
            return queryable.Select(Mapper.GetExpression<TSource, TDest>());
        }

        public static IQueryable<TDest> ResolveSelectExternal<TSource, TDest>(this IQueryable<TSource> queryable, Culture cultureId)
        {
            return queryable.Select(Mapper.GetExternalExpression<TSource, TDest>(cultureId));
        }

        public static IQueryable<TDest> ResolveSelect<TSource, TDest>(this IQueryable<TSource> queryable, Culture cultureId)
            where TDest : ICultureSelectExpression<TSource, TDest>, new()
        {
            return queryable.Select(Mapper.GetExpression<TSource, TDest>(cultureId));
        }

        public static IEnumerable<TDest> MapSelect<TSource, TDest>(this IEnumerable<TSource> enumerable)
            where TSource:class 
            where TDest: class , new()
        {
            return enumerable.Select(Mapper.Map<TSource, TDest>);
        }
    }
}
