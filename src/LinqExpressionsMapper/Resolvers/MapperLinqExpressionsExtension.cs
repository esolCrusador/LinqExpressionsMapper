using System.Collections.Generic;
using System.Globalization;

namespace System.Linq.Expressions
{
    public static class MapperLinqExpressionsExtension
    {
        public static Expression<Func<TSource, TResult>> ResolveMemberInit<TSource, TResult, TSourceMember, TMember>(this Expression<Func<TSource, TResult>> init,
            Expression<Func<TSource, TSourceMember>> sourceMember,
            Expression<Func<TResult, TMember>> member)
            where TMember : ISelectExpression<TSourceMember, TMember>, new()
        {
            return init.AddMemberInit(sourceMember, member, Mapper.GetExpression<TSourceMember, TMember>());
        }

        public static Expression<Func<TSource, TResult>> ResolveMemberInit<TSource, TResult, TSourceMember, TMember>(this Expression<Func<TSource, TResult>> init,
            Expression<Func<TSource, IEnumerable<TSourceMember>>> sourceMember,
            Expression<Func<TResult, IEnumerable<TMember>>> member)
            where TMember : ISelectExpression<TSourceMember, TMember>, new()
        {
            return init.AddMemberInit(sourceMember, member, Mapper.GetExpression<TSourceMember, TMember>());
        }

        public static Expression<Func<TSource, TResult>> ResolveMemberInit<TSource, TResult, TSourceMember, TMember>(this Expression<Func<TSource, TResult>> init,
            Expression<Func<TSource, ICollection<TSourceMember>>> sourceMember,
            Expression<Func<TResult, IEnumerable<TMember>>> member)
            where TMember : ISelectExpression<TSourceMember, TMember>, new()
        {
            return init.AddMemberInit(sourceMember.Continue(source => source.AsEnumerable()), member, Mapper.GetExpression<TSourceMember, TMember>());
        }

        public static Expression<Func<TSource, TResult>> ResolveMemberInit<TSource, TResult, TSourceMember, TMember>(this Expression<Func<TSource, TResult>> init,
            Expression<Func<TSource, TSourceMember>> sourceMember,
            Expression<Func<TResult, TMember>> member,
            Culture cultureId)
            where TMember : ICultureSelectExpression<TSourceMember, TMember>, new()
        {
            return init.AddMemberInit(sourceMember, member, Mapper.GetExpression<TSourceMember, TMember>(cultureId));
        }

        public static Expression<Func<TSource, TResult>> ResolveMemberInit<TSource, TResult, TSourceMember, TMember>(this Expression<Func<TSource, TResult>> init,
            Expression<Func<TSource, IEnumerable<TSourceMember>>> sourceMember,
            Expression<Func<TResult, IEnumerable<TMember>>> member,
            Culture cultureId)
            where TMember : ICultureSelectExpression<TSourceMember, TMember>, new()
        {
            return init.AddMemberInit(sourceMember, member, Mapper.GetExpression<TSourceMember, TMember>(cultureId));
        }

        public static Expression<Func<TSource, TResult>> ResolveMemberInit<TSource, TResult, TSourceMember, TMember>(this Expression<Func<TSource, TResult>> init,
            Expression<Func<TSource, ICollection<TSourceMember>>> sourceMember,
            Expression<Func<TResult, IEnumerable<TMember>>> member,
            Culture cultureId)
            where TMember : ISelectExpression<TSourceMember, TMember>, new()
        {
            return init.AddMemberInit(sourceMember.Continue(source => source.AsEnumerable()), member, Mapper.GetExpression<TSourceMember, TMember>());
        }
    }
}
