using System.Collections.Generic;
using LinqExpressionsMapper.Extensions.LinqExpression.Rebinders;

namespace System.Linq.Expressions
{
    public static class ClassBehaviourExpressionExtensions
    {
        /// <summary>
        /// Combines two Projection expressions into one. Projection of child class is extended with Projection of entity (base entity) on base class. Projection of members initialized in baseInit expression and not not initialized in init expression will be copied.
        /// </summary>
        /// <typeparam name="TSource">Source Entity type.</typeparam>
        /// <typeparam name="TResult">Result projection type.</typeparam>
        /// <typeparam name="TBaseSource">Base Source Entity type.</typeparam>
        /// <typeparam name="TBaseResult">Base Result projection type.</typeparam>
        /// <param name="init">Child projection expression.</param>
        /// <param name="baseInit">Base projection expression.</param>
        /// <returns>Extended Projection expression.</returns>
        public static Expression<Func<TSource, TResult>> InheritInit<TSource, TResult, TBaseSource, TBaseResult>(this Expression<Func<TSource, TResult>> init, Expression<Func<TBaseSource, TBaseResult>> baseInit)
            where TSource : TBaseSource
            where TResult : TBaseResult
        {
            var rebinder = new InitInheritanceRebinder<TBaseSource, TBaseResult, TSource, TResult>(baseInit, init);

            return rebinder.ExtendInitialization();
        }

        /// <summary>
        /// Combines two Projection expressions into one in case when result Projection is build from two entity types. Target projection expression is extended with projection of entityMember to baseClass. Projection members initialized in baseInit expression but not initialized in init expression will be copied.
        /// </summary>
        /// <typeparam name="TSource">Source Entity type.</typeparam>
        /// <typeparam name="TResult">Result projection type.</typeparam>
        /// <typeparam name="TBaseSource">Base Entity type.</typeparam>
        /// <typeparam name="TBaseResult">Base Result projection type.</typeparam>
        /// <param name="init">Child projection expression.</param>
        /// <param name="entityMember">Base entity member memberExpression.</param>
        /// <param name="baseInit">Base init expression.</param>
        /// <returns>Extended Projection expression.</returns>
        public static Expression<Func<TSource, TResult>> InheritInit<TSource, TResult, TBaseSource, TBaseResult>(this Expression<Func<TSource, TResult>> init, Expression<Func<TSource, TBaseSource>> entityMember, Expression<Func<TBaseSource, TBaseResult>> baseInit)
            where TResult : TBaseResult
        {
            var rebinder = new InitFromMemberInheritanceRebinder<TBaseSource, TBaseResult, TSource, TResult>(baseInit, entityMember, init);

            return rebinder.ExtendInitialization();
        }

        /// <summary>
        /// Adds Target member initialization from sourceMember using memberInit projection expression.
        /// </summary>
        /// <typeparam name="TSource">Source Entity type.</typeparam>
        /// <typeparam name="TResult">Result projection type.</typeparam>
        /// <typeparam name="TSourceMember">Source member type.</typeparam>
        /// <typeparam name="TMember">Member type.</typeparam>
        /// <param name="init">Projection expression.</param>
        /// <param name="sourceMember">Source member memberExpression.</param>
        /// <param name="member">Target member memberExpression.</param>
        /// <param name="memberInit">Source member to Target member projection expression.</param>
        /// <returns>Projection expression with added Target member initialization.</returns>
        public static Expression<Func<TSource, TResult>> AddMemberInit<TSource, TResult, TSourceMember, TMember>(this Expression<Func<TSource, TResult>> init,
            Expression<Func<TSource, TSourceMember>> sourceMember,
            Expression<Func<TResult, TMember>> member,
            Expression<Func<TSourceMember, TMember>> memberInit)
        {
            var rebinder = new MemberInitRebinder<TSource, TResult, TSourceMember, TMember>(init, sourceMember, member, memberInit);

            return rebinder.ExtendInitialization();
        }
        
        /// <summary>
        /// Adds Target Enumerable member initialization from Enumerable sourceMember using memberInit projection expression.
        /// </summary>
        /// <typeparam name="TSource">Source Entity type.</typeparam>
        /// <typeparam name="TResult">Result Projection type.</typeparam>
        /// <typeparam name="TSourceMember">Source Enumerable member type.</typeparam>
        /// <typeparam name="TMember">Target Enumerable member type.</typeparam>
        /// <param name="init">Projection expression.</param>
        /// <param name="sourceMember">Source Enumerable member memberExpression.</param>
        /// <param name="member">Target Enumerable member memberEpxression.</param>
        /// <param name="memberInit">Source member to Target member projection expression.</param>
        /// <returns>Projection expression with added Target Enumerable member initialization.</returns>
        public static Expression<Func<TSource, TResult>> AddMemberInit<TSource, TResult, TSourceMember, TMember>(this Expression<Func<TSource, TResult>> init,
            Expression<Func<TSource, IEnumerable<TSourceMember>>> sourceMember,
            Expression<Func<TResult, IEnumerable<TMember>>> member,
            Expression<Func<TSourceMember, TMember>> memberInit)
        {
            var rebinder = new EnumerableMemberInitRebinder<TSource, TResult, TSourceMember, TMember>(init, sourceMember, member, memberInit);

            return rebinder.ExtendInitialization();
        }

        /// <summary>
        /// Adds Target member initialization from Entity using memberInit projection expression.
        /// </summary>
        /// <typeparam name="TSource">Source Entity type.</typeparam>
        /// <typeparam name="TResult">Result Projection type.</typeparam>
        /// <typeparam name="TMember">Target member type.</typeparam>
        /// <param name="init">Projection expression.</param>
        /// <param name="member">Target member memberExpression.</param>
        /// <param name="memberInit">Entity to Target member projection expression.</param>
        /// <returns>Perojection expression with added Target member initialization.</returns>
        public static Expression<Func<TSource, TResult>> AddMemberInit<TSource, TResult, TMember>(this Expression<Func<TSource, TResult>> init,
            Expression<Func<TResult, TMember>> member,
            Expression<Func<TSource, TMember>> memberInit)
        {
            var rebinder = new MemberInitRebinder<TSource, TResult, TSource, TMember>(init, null, member, memberInit);

            return rebinder.ExtendInitialization();
        }
    }
}
