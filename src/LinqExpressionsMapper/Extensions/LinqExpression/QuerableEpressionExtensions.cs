using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace System.Linq
{
    public static class QuerableEpressionExtensions
    {
        private const string OrderByMethod = "OrderBy";
        private const string ThenByMethod = "ThenBy";
        private const string OrderByDescMethod = "OrderByDescending";
        private const string ThenByDescMethod = "ThenByDescending";

        public static IOrderedQueryable<TItem> Sort<TItem>(this IQueryable<TItem> queryable, string sort, ListSortDirection direction, bool continueSort = false)
        {
            Type itemType = typeof (TItem);

            ParameterExpression parameter = Expression.Parameter(itemType, itemType.Name.ToLower());

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


            MemberInfo member = typeof (TItem).GetMember(sort)[0];

            LambdaExpression memberExpression = Expression.Lambda(Expression.MakeMemberAccess(parameter, member), parameter);

            return (IOrderedQueryable<TItem>) queryable.Provider.CreateQuery(Expression.Call(typeof (Queryable), orderMethod, new[] {itemType, memberExpression.Body.Type}, queryable.Expression, Expression.Quote(memberExpression)));
        }

        public static IQueryable<TItem> Sort<TItem>(this IQueryable<TItem> queryable, IEnumerable<KeyValuePair<string, ListSortDirection>> sorts)
        {
            bool replaceSort = false;
            foreach (var sort in sorts)
            {
                queryable = queryable.Sort(sort.Key, sort.Value, replaceSort);
                replaceSort = true;
            }

            return queryable;
        }
    }
}
