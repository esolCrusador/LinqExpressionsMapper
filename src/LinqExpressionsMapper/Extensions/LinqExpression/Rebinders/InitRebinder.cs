using System;
using System.Linq.Expressions;

namespace LinqExpressionsMapper.Extensions.LinqExpression.Rebinders
{
    internal abstract class InitRebinder<TSource, TDest>
    {
        protected InitRebinder()
        {
        }

        public abstract Expression<Func<TSource, TDest>> ExtendInitialization(Expression<Func<TSource, TDest>> initializationExpression); 
    }
}
