using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqExpressionsMapper.Extensions.LinqExpression.Rebinders
{
    internal class MemberInitRebinder<TSource, TResult, TSourceMember, TMember>
        :InitRebinder<TSource, TResult>
    {
        private readonly Expression<Func<TSource, TSourceMember>> _sourceMember;
        private readonly Expression<Func<TResult, TMember>> _member;
        private readonly Expression<Func<TSourceMember, TMember>> _initialization;

        public MemberInitRebinder(Expression<Func<TSource, TResult>> initializationExpression,
                                 Expression<Func<TSource, TSourceMember>> sourceMember,
                                 Expression<Func<TResult, TMember>> member,
                                 Expression<Func<TSourceMember, TMember>> initialization)
            : base(initializationExpression)
        {
            _sourceMember = sourceMember;
            _member = member;
            _initialization = initialization;
        }

        protected virtual Expression GetInitExpression()
        {
            return _sourceMember == null 
                ? _initialization.ReplaceParameter(Parameter).Body 
                : _sourceMember.Continue(_initialization).ReplaceParameter(Parameter).Body;
        }

        public override Expression<Func<TSource, TResult>> ExtendInitialization()
        {
            var memberInitBody = InitializationExpression.Body as MemberInitExpression
                                 ?? Expression.MemberInit((NewExpression) InitializationExpression.Body);

            List<MemberBinding> bindingsList = memberInitBody.Bindings.ToList();

            MemberInfo member = ((MemberExpression)_member.Body).Member;

            Expression memberFromSourceInit = GetInitExpression();

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

            return Expression.Lambda<Func<TSource, TResult>>(Expression.MemberInit(memberInitBody.NewExpression, bindingsList), Parameter);
        }
    }
}
