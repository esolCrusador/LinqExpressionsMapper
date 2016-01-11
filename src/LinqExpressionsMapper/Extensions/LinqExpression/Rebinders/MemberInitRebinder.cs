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

        public MemberInitRebinder(Expression<Func<TSource, TSourceMember>> sourceMember,
                                 Expression<Func<TResult, TMember>> member,
                                 Expression<Func<TSourceMember, TMember>> initialization)
            : base()
        {
            _sourceMember = sourceMember;
            _member = member;
            _initialization = initialization;
        }

        protected virtual Expression GetInitExpression(ParameterExpression parameter)
        {
            return _sourceMember == null 
                ? _initialization.ReplaceParameter(parameter).Body 
                : _sourceMember.Continue(_initialization).ReplaceParameter(parameter).Body;
        }

        public override Expression<Func<TSource, TResult>> ExtendInitialization(Expression<Func<TSource, TResult>>  initializationExpression)
        {
            ParameterExpression parameter = initializationExpression.Parameters[0];
            var memberInitBody = initializationExpression.Body as MemberInitExpression
                                 ?? Expression.MemberInit((NewExpression)initializationExpression.Body);

            List<MemberBinding> bindingsList = memberInitBody.Bindings.ToList();

            MemberInfo member = ((MemberExpression)_member.Body).Member;

            Expression memberFromSourceInit = GetInitExpression(parameter);

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

            return Expression.Lambda<Func<TSource, TResult>>(Expression.MemberInit(memberInitBody.NewExpression, bindingsList), parameter);
        }
    }
}
