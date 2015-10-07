using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LinqExpressionsMapper.Extensions.LinqExpression.Rebinders
{
    internal class InitInheritanceRebinder<TBaseSource, TBaseDist, TSource, TDest> : InitRebinder<TSource, TDest>
        where TDest : TBaseDist
    {
        protected readonly Expression<Func<TBaseSource, TBaseDist>> BaseInitExpr;

        public InitInheritanceRebinder(Expression<Func<TBaseSource, TBaseDist>> baseExpr, Expression<Func<TSource, TDest>> initializationExpression)
            : base(initializationExpression)
        {
            BaseInitExpr = baseExpr;
        }

        public override Expression<Func<TSource, TDest>> ExtendInitialization()
        {
            List<MemberBinding> baseBindings = GetBaseInitExpressionBody().Bindings.ToList();
            List<MemberBinding> bindings;

            NewExpression newExpression = InitializationExpression.Body as NewExpression;
            if (newExpression == null)
            {
                var inheritedInitExpr = (MemberInitExpression) InitializationExpression.Body;

                newExpression = inheritedInitExpr.NewExpression;
                bindings = inheritedInitExpr.Bindings.ToList();
            }
            else
            {
                bindings = new List<MemberBinding>(0);
            }


            bindings.AddRange(baseBindings.Where(baseBinding => bindings.All(b => b.Member.Name != baseBinding.Member.Name)));

            return Expression.Lambda<Func<TSource, TDest>>(Expression.MemberInit(newExpression, bindings), Parameter);
        }

        protected virtual MemberInitExpression GetBaseInitExpressionBody()
        {
            Expression<Func<TSource, TBaseDist>> newExpression = BaseInitExpr.ReplaceParameter<TBaseSource, TSource, TBaseDist>(Parameter);

            return (MemberInitExpression) newExpression.Body;
        }
    }
}
