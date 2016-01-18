using System;
using System.Linq.Expressions;

namespace LinqExpressionsMapper
{
    /// <summary>
    /// Builder of Projection logic.
    /// </summary>
    /// <typeparam name="TSource">Source element type.</typeparam>
    public struct ExpressionMappingBuilder<TSource>
    {
        /// <summary>
        /// Defines target element type.
        /// </summary>
        /// <typeparam name="TDest">Destanation element type.</typeparam>
        /// <returns>Builder of projection.</returns>
        public ExpressionMappingBuilder<TSource, TDest> To<TDest>()
        {
            return new ExpressionMappingBuilder<TSource, TDest>();
        }
    }

    /// <summary>
    /// Builder of Projection logic.
    /// </summary>
    /// <typeparam name="TSource">Source element type.</typeparam>
    /// <typeparam name="TDest">Destanation element type.</typeparam>
    public struct ExpressionMappingBuilder<TSource, TDest>
    {
        /// <summary>
        /// Gets Projectection expression from source to destanation. Expression: registered select expression container or TDest as select resolver.
        /// </summary>
        /// <returns>Projection expression.</returns>
        public Expression<Func<TSource, TDest>> GetExpression()
        {
            return Mapper.GetExternalExpression<TSource, TDest>();
        }
        /// <summary>
        /// Gets Parametrised Projection expression from source to destanation passing parameter.  Expression: registered select expression container or TDest as select resolver.
        /// </summary>
        /// <typeparam name="TParam">Parameter type.</typeparam>
        /// <param name="param">Parameter value.</param>
        /// <returns>Projection expression.</returns>
        public Expression<Func<TSource, TDest>> GetExpression<TParam>(TParam param)
        {
            return Mapper.GetExternalExpression<TSource, TDest, TParam>(param);
        }
        /// <summary>
        /// Sets select expression container type. If Projection expression is not registered yet that activated TSelect() will be used for registration.
        /// </summary>
        /// <typeparam name="TSelect">Select expression container type.</typeparam>
        /// <returns>Builder of projection.</returns>
        public ExpressionMappingBuilder<TSelect, TSource, TDest> Using<TSelect>() 
            where TSelect : ISelectExpression<TSource, TDest>, new()
        {
            return new ExpressionMappingBuilder<TSelect, TSource, TDest>();
        }

        /// <summary>
        /// Sets select expression container type. If Projection expression is not registered yet that activated TSelect() will be used for registration.
        /// </summary>
        /// <typeparam name="TSelect">Select expression container type.</typeparam>
        /// <typeparam name="TParam">Parameter type.</typeparam>
        /// <returns>Builder of projection.</returns>
        public ExpressionMappingBuilder<TSelect, TSource, TDest, TParam> Using<TSelect, TParam>()
            where TSelect : ISelectExpression<TSource, TDest, TParam>, new()
        {
            return new ExpressionMappingBuilder<TSelect, TSource, TDest, TParam>();
        }

        /// <summary>
        /// Converts MappingBuilder to result expression.
        /// </summary>
        /// <param name="mappingBuilder">Builder of projection.</param>
        /// <returns>Result Projection expression.</returns>
        public static implicit operator Expression<Func<TSource, TDest>>(ExpressionMappingBuilder<TSource, TDest> mappingBuilder)
        {
            return mappingBuilder.GetExpression();
        }
    }

    /// <summary>
    /// Builder of Projection logic.
    /// </summary>
    /// <typeparam name="TSelect">Projection select expression container.</typeparam>
    /// <typeparam name="TSource">Source element type.</typeparam>
    /// <typeparam name="TDest">Dest element type.</typeparam>
    public struct ExpressionMappingBuilder<TSelect, TSource, TDest> 
        where TSelect : ISelectExpression<TSource, TDest>, new()
    {
        /// <summary>
        /// Gets Projections expression using TSelect if Projection expression container is not registered yet.
        /// </summary>
        /// <returns>Projection expression.</returns>
        public Expression<Func<TSource, TDest>> GetExpression()
        {
            return Mapper.GetExpression<TSelect, TSource, TDest>();
        }

        /// <summary>
        /// Converts MappingBuilder to result expression.
        /// </summary>
        /// <param name="mappingBuilder">Builder of projection.</param>
        /// <returns>Result Projection expression.</returns>
        public static implicit operator Expression<Func<TSource, TDest>>(ExpressionMappingBuilder<TSelect, TSource, TDest> mappingBuilder)
        {
            return mappingBuilder.GetExpression();
        }
    }

    /// <summary>
    /// Builder of Projection logic.
    /// </summary>
    /// <typeparam name="TSelect">Projection select expression container.</typeparam>
    /// <typeparam name="TSource">Source element type.</typeparam>
    /// <typeparam name="TDest">Dest element type.</typeparam>
    /// <typeparam name="TParam">Parameter type.</typeparam>
    public struct ExpressionMappingBuilder<TSelect, TSource, TDest, TParam>
        where TSelect : ISelectExpression<TSource, TDest, TParam>, new()
    {
        /// <summary>
        /// Gets Parametrised Projections expression using TSelect if Projection expression container is not registered yet.
        /// </summary>
        /// <typeparam name="TParam">Projection epxression parameter type.</typeparam>
        /// <param name="param">Projection epxression parameter value.</param>
        /// <returns>Projection expression.</returns>
        public Expression<Func<TSource, TDest>> GetExpression(TParam param)
        {
            return Mapper.GetExpression<TSelect, TSource, TDest, TParam>(param);
        }
    }
}
