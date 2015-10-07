using System;
using System.Linq.Expressions;

namespace LinqExpressionsMapper.Extensions.LinqExpression.Rebinders
{
    internal class InitFromMemberInheritanceRebinder<TBaseSource, TBaseDest, TSource, TDest> : InitInheritanceRebinder<TBaseSource, TBaseDest, TSource, TDest>
        where TDest : TBaseDest
    {
        private readonly Expression<Func<TSource, TBaseSource>> _entityMember;

        public InitFromMemberInheritanceRebinder(Expression<Func<TBaseSource, TBaseDest>> baseExpr, Expression<Func<TSource, TBaseSource>> entityMember, Expression<Func<TSource, TDest>> initializationExpression)
            : base(baseExpr, initializationExpression)
        {
            _entityMember = entityMember;
        }

        protected override MemberInitExpression GetBaseInitExpressionBody()
        {
            Expression<Func<TSource, TBaseDest>> replacedInit = BaseInitExpr.ReplaceParameter(_entityMember);

            return (MemberInitExpression)replacedInit.ReplaceParameter(Parameter).Body;
        }
    }
}
