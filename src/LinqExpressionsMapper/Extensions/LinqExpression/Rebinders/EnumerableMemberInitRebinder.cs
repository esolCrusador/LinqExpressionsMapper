using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqExpressionsMapper.Extensions.LinqExpression.Rebinders
{
    internal class EnumerableMemberInitRebinder<TSource, TResult, TSourceMember, TMember>
        : InitRebinder<TSource, TResult>
    {
        private static readonly MethodInfo SelectMethodInfo;

        private delegate IEnumerable<TMember> EnumerableSelect(IEnumerable<TSourceMember> enumerable, Func<TSourceMember, TMember> select);

        static EnumerableMemberInitRebinder ()
        {
            EnumerableSelect selectDelegate = Enumerable.Select;

            SelectMethodInfo = selectDelegate.Method;
        }

        private readonly Expression<Func<TSource, IEnumerable<TSourceMember>>> _sourceMember;
        private readonly Expression<Func<TResult, IEnumerable<TMember>>> _member;
        private readonly Expression<Func<TSourceMember, TMember>> _initialization;

        public EnumerableMemberInitRebinder(Expression<Func<TSource, IEnumerable<TSourceMember>>> sourceMember,
                                     Expression<Func<TResult, IEnumerable<TMember>>> member,
                                     Expression<Func<TSourceMember, TMember>> initialization)
        {
            _sourceMember = sourceMember;
            _member = member;
            _initialization = initialization;
        }

        protected internal virtual Expression GetInitExpression(ParameterExpression parameter)
        {
            return Expression.Call(SelectMethodInfo, _sourceMember.Body, _initialization).ReplaceParameter(_sourceMember.Parameters[0], parameter);
        }

        public override Expression<Func<TSource, TResult>> ExtendInitialization(Expression<Func<TSource, TResult>> initializationExpression)
        {
            ParameterExpression paramter = initializationExpression.Parameters[0];
            var memberInitBody = (MemberInitExpression)initializationExpression.Body;
            List<MemberBinding> bindingsList = memberInitBody.Bindings.ToList();

            MemberInfo member = ((MemberExpression)_member.Body).Member;

            Expression memberFromSourceInit = GetInitExpression(paramter);

            var memberAssigment = (MemberAssignment)bindingsList.FirstOrDefault(m => m.Member.Name == member.Name);
            if (memberAssigment == null)
            {
                memberAssigment = Expression.Bind(member, memberFromSourceInit);
            }
            else
            {
                bindingsList.Remove(memberAssigment);
                memberAssigment = memberAssigment.Update(memberFromSourceInit);
            }
            bindingsList.Add(memberAssigment);

            return Expression.Lambda<Func<TSource, TResult>>(Expression.MemberInit(memberInitBody.NewExpression, bindingsList), paramter);
        }
    }
}
