using System;
using System.Linq.Expressions;

namespace LinqExpressionsMapper
{
    public struct ExpressionMappingBuilder<TSource>
    {
        public ExpressionMappingBuilder<TSource, TDest> To<TDest>()
        {
            return new ExpressionMappingBuilder<TSource, TDest>();
        }
    }

    public struct ExpressionMappingBuilder<TSource, TDest>
    {
        public Expression<Func<TSource, TDest>> GetExpression()
        {
            return Mapper.GetExternalExpression<TSource, TDest>();
        }

        public Expression<Func<TSource, TDest>> GetExpression<TParam>(TParam param)
        {
            return Mapper.GetExternalExpression<TSource, TDest, TParam>(param);
        }

        public ExpressionMappingBuilder<TMapper, TSource, TDest> Using<TMapper>() 
            where TMapper : ISelectExpression<TSource, TDest>, new()
        {
            return new ExpressionMappingBuilder<TMapper, TSource, TDest>();
        }

        public static implicit operator Expression<Func<TSource, TDest>>(ExpressionMappingBuilder<TSource, TDest> mappingBuilder)
        {
            return mappingBuilder.GetExpression();
        }
    }

    public struct ExpressionMappingBuilder<TSelect, TSource, TDest> 
        where TSelect : ISelectExpression<TSource, TDest>, new()
    {
        public Expression<Func<TSource, TDest>> GetExpression()
        {
            return Mapper.GetExpression<TSelect, TSource, TDest>();
        }

        public Expression<Func<TSource, TDest>> GetExpression<TParam>(TParam param)
        {
            return Mapper.GetExpression<TSelect, TSource, TDest, TParam>(param);
        }

        public static implicit operator Expression<Func<TSource, TDest>>(ExpressionMappingBuilder<TSelect, TSource, TDest> mappingBuilder)
        {
            return mappingBuilder.GetExpression();
        }
    }
}
