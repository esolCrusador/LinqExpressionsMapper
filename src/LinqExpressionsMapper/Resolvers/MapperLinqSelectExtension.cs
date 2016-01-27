using System.Collections.Generic;
using LinqExpressionsMapper;
using LinqExpressionsMapper.Resolvers.MappingBuilders;

namespace System.Linq
{
    public static class MapperLinqSelectExtension
    {
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
