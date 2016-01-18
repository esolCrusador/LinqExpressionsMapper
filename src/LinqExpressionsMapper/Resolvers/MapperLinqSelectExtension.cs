﻿using System.Collections.Generic;
using System.Linq.Expressions;
using LinqExpressionsMapper;
using LinqExpressionsMapper.Resolvers.MappingBuilders;

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

        public static IQueryable<TDest> ResolveSelect<TSelect, TSource, TDest>(this IQueryable<TSource> queryable)
            where TSelect : ISelectExpression<TSource, TDest>, new()
        {
            return queryable.Select(Mapper.GetExpression<TSelect, TSource, TDest>());
        }

        public static IQueryable<TDest> ResolveSelectExternal<TSource, TDest, TParam>(this IQueryable<TSource> queryable, TParam param)
        {
            return queryable.Select(Mapper.GetExternalExpression<TSource, TDest, TParam>(param));
        }

        public static IQueryable<TDest> ResolveSelect<TSource, TDest, TParam>(this IQueryable<TSource> queryable, TParam param)
            where TDest : ISelectExpression<TSource, TDest, TParam>, new()
        {
            return queryable.Select(Mapper.GetExpression<TSource, TDest, TParam>(param));
        }

        public static IEnumerable<TDest> MapSelect<TSource, TDest>(this IEnumerable<TSource> enumerable)
            where TSource:class 
            where TDest: class , new()
        {
            return enumerable.Select(Mapper.Map<TSource, TDest>);
        }

        public static IEnumerable<TDest> MapSelect<TMapper, TSource, TDest>(this IEnumerable<TSource> enumerable)
            where TSource : class
            where TDest : class, new()
            where TMapper : IPropertiesMapper<TSource, TDest>, new()
        {
            return enumerable.Select(Mapper.Map<TMapper, TSource, TDest>);
        }

        /// <summary>
        /// Creates Enumerable mapping logic builder.
        /// </summary>
        /// <typeparam name="TSource">Source element type.</typeparam>
        /// <param name="enumerable">Enumerable of source elements.</param>
        /// <returns>Enumerable mapping logic builder.</returns>
        public static EnumerableMappingBuilder<TSource> Map<TSource>(this IEnumerable<TSource> enumerable)
            where TSource : class
        {
            return new EnumerableMappingBuilder<TSource>(enumerable);
        }

        /// <summary>
        /// Creates Queryable projection logic builder.
        /// </summary>
        /// <typeparam name="TSource">Source element type.</typeparam>
        /// <param name="queryable">Queryable of source elements.</param>
        /// <returns>Queryable projection logic builder.</returns>
        public static QueryableMappingBuilder<TSource> Project<TSource>(this IQueryable<TSource> queryable)
        {
            return new QueryableMappingBuilder<TSource>(queryable);
        }
    }
}
