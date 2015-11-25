using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace System.Linq
{
    /// <summary>
    /// Extensions of IQueryable
    /// </summary>
    public static class QuerableEpressionExtensions
    {
        private const string OrderByMethod = "OrderBy";
        private const string ThenByMethod = "ThenBy";
        private const string OrderByDescMethod = "OrderByDescending";
        private const string ThenByDescMethod = "ThenByDescending";

        /// <summary>
        /// Sorts by member names
        /// </summary>
        /// <param name="queryable">IQueryable</param>
        /// <param name="sorts">Enumeration of sorts</param>
        /// <typeparam name="TItem">Entity Type</typeparam>
        /// <returns>Ordered IQueryable</returns>
        public static IQueryable<TItem> Sort<TItem>(this IQueryable<TItem> queryable, IEnumerable<KeyValuePair<string, ListSortDirection>> sorts)
        {
            return Sort(queryable, sorts, true);
        }

        /// <summary>
        /// Continues sorting by member names
        /// </summary>
        /// <param name="queryable">Ordered IQueryable</param>
        /// <param name="sorts">Enumeration of sorts</param>
        /// <typeparam name="TItem">Entity Type</typeparam>
        /// <returns>Ordered IQueryable</returns>
        public static IQueryable<TItem> ThenSort<TItem>(this IOrderedQueryable<TItem> queryable, IEnumerable<KeyValuePair<string, ListSortDirection>> sorts)
        {
            return Sort(queryable, sorts, false);
        }

        /// <summary>
        /// Sorts by member name
        /// </summary>
        /// <param name="queryable">IQueryable</param>
        /// <param name="sort">Name of property</param>
        /// <param name="direction">Direction of sorting</param>
        /// <typeparam name="TItem">Entity type</typeparam>
        /// <returns>Ordered IQueryable</returns>
        public static IOrderedQueryable<TItem> Sort<TItem>(this IQueryable<TItem> queryable, string sort, ListSortDirection direction)
        {
            return Sort(queryable, sort, direction, false);
        }

        /// <summary>
        /// Continues sorting by member name
        /// </summary>
        /// <param name="queryable">Ordered IQueryable</param>
        /// <param name="sort">Name of property</param>
        /// <param name="direction">Direction of sorting</param>
        /// <typeparam name="TItem">Entity type</typeparam>
        /// <returns>Ordered IQueryable</returns>
        public static IOrderedQueryable<TItem> ThenSort<TItem>(this IOrderedQueryable<TItem> queryable, string sort, ListSortDirection direction)
        {
            return Sort(queryable, sort, direction, true);
        }

        private static IQueryable<TItem> Sort<TItem>(this IQueryable<TItem> queryable, IEnumerable<KeyValuePair<string, ListSortDirection>> sorts, bool replaceSort)
        {
            foreach (KeyValuePair<string, ListSortDirection> sort in sorts)
            {
                queryable = queryable.Sort(sort.Key, sort.Value, replaceSort);
                replaceSort = false;
            }

            return queryable;
        }

        private static IOrderedQueryable<TItem> Sort<TItem>(this IQueryable<TItem> queryable, string sort, ListSortDirection direction, bool continueSort)
        {
            Type itemType = typeof(TItem);

            string orderMethod;
            switch (direction)
            {
                case ListSortDirection.Ascending:
                    orderMethod = !continueSort ? OrderByMethod : ThenByMethod;
                    break;
                case ListSortDirection.Descending:
                    orderMethod = !continueSort ? OrderByDescMethod : ThenByDescMethod;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            LambdaExpression memberExpression = GetMemberLambdaExpression(itemType, sort);

            return (IOrderedQueryable<TItem>)queryable.Provider.CreateQuery(Expression.Call(typeof(Queryable), orderMethod, new[] { itemType, memberExpression.Body.Type }, queryable.Expression, Expression.Quote(memberExpression)));
        }

        private static LambdaExpression GetMemberLambdaExpression(Type itemType, string propertyPath)
        {
            ParameterExpression parameter = Expression.Parameter(itemType, itemType.Name.ToLower());
            Expression expression = parameter;

            string[] names = propertyPath.Split('.');

            foreach (string name in names)
            {
                MemberInfo member = itemType.GetMember(name)[0];

                expression = Expression.MakeMemberAccess(expression, member);

                itemType = expression.Type;
            }

            return Expression.Lambda(expression, parameter);
        }
    }
}
