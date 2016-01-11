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

        public InitInheritanceRebinder(Expression<Func<TBaseSource, TBaseDist>> baseExpr)
            : base()
        {
            BaseInitExpr = baseExpr;
        }

        public override Expression<Func<TSource, TDest>> ExtendInitialization(Expression<Func<TSource, TDest>> initializationExpression)
        {
            ParameterExpression parameter = initializationExpression.Parameters[0];
            List<MemberBinding> baseBindings = GetBaseInitExpressionBody(parameter).Bindings.ToList();
            List<MemberBinding> bindings;

            NewExpression newExpression = initializationExpression.Body as NewExpression;
            if (newExpression == null)
            {
                var inheritedInitExpr = (MemberInitExpression)initializationExpression.Body;

                newExpression = inheritedInitExpr.NewExpression;
                bindings = inheritedInitExpr.Bindings.ToList();
            }
            else
            {
                bindings = new List<MemberBinding>(0);
            }


            bindings.AddRange(baseBindings.Where(baseBinding => bindings.All(b => b.Member.Name != baseBinding.Member.Name)));

            return Expression.Lambda<Func<TSource, TDest>>(Expression.MemberInit(newExpression, bindings), parameter);
        }

        protected virtual MemberInitExpression GetBaseInitExpressionBody(ParameterExpression parameter)
        {
            Expression<Func<TSource, TBaseDist>> newExpression = BaseInitExpr.ReplaceParameter<TBaseSource, TSource, TBaseDist>(parameter);

            return (MemberInitExpression) newExpression.Body;
        }
    }
}
