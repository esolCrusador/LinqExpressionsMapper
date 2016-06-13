using System;
using System.Linq;
using System.Linq.Expressions;

namespace LinqExpressionsMapper.Resolvers.MappingBuilders
{
    /// <summary>
    /// Builder of query projection logic.
    /// </summary>
    /// <typeparam name="TSource">Source element type.</typeparam>
    public struct QueryableMappingBuilder<TSource>
    {
        private readonly IQueryable<TSource> _sourceQueryable;

        internal QueryableMappingBuilder(IQueryable<TSource> sourceQueryable)
        {
            _sourceQueryable = sourceQueryable;
        }

        /// <summary>
        /// Projects query to destanation query.
        /// </summary>
        /// <typeparam name="TDest">Destanation query element type.</typeparam>
        /// <returns>Query of destanation elements.</returns>
        public IQueryable<TDest> To<TDest>()
        {
            return _sourceQueryable.Select(Mapper.SelectResolverWith0Params.GetExternalExpression<TSource, TDest>());
        }

        /// <summary>
        /// Projects query to destanation query using projection configuration.
        /// </summary>
        /// <typeparam name="TDest">Destanation element type.</typeparam>
        /// <param name="configure">Projection configuration action.</param>
        /// <returns>Query of destanation elements.</returns>
        public IQueryable<TDest> To<TDest>(Action<QueryableMappingBuilder<TSource, TDest>> configure)
        {
            var factory = new QueryableFactory<TDest>();
            configure(new QueryableMappingBuilder<TSource, TDest>(_sourceQueryable, factory));

            return factory.Create();
        }
    }

    /// <summary>
    /// Builder of query projection logic.
    /// </summary>
    /// <typeparam name="TSource">Source element type.</typeparam>
    /// <typeparam name="TDest">Destanation element type.</typeparam>
    public struct QueryableMappingBuilder<TSource, TDest>
    {
        private readonly IQueryable<TSource> _sourceQueryable;

        internal QueryableMappingBuilder(IQueryable<TSource> sourceQueryable, QueryableFactory<TDest> queryableFactory)
        {
            _sourceQueryable = sourceQueryable;
            QueryableFactory = queryableFactory;

            QueryableFactory.Create = GetQueryable;
        }

        internal QueryableFactory<TDest> QueryableFactory { get; private set; }

        private IQueryable<TDest> GetQueryable()
        {
            return _sourceQueryable.Select(Mapper.SelectResolverWith0Params.GetExternalExpression<TSource, TDest>());
        }

        /// <summary>
        /// Sets select expression container type. If Projection expression is not registered yet that activated TSelect() will be used for registration. 
        /// </summary>
        /// <typeparam name="TSelect">Projection select expression container.</typeparam>
        /// <returns>Builder of projection.</returns>
        public QueryableMappingBuilder<TSelect, TSource, TDest> Using<TSelect>() 
            where TSelect : ISelectExpression<TSource, TDest>, new()
        {
            return new QueryableMappingBuilder<TSelect, TSource, TDest>(_sourceQueryable, QueryableFactory);
        }

        /// <summary>
        /// Sets param of parametrised projection expression factory.
        /// </summary>
        /// <typeparam name="TParam">Param type.</typeparam>
        /// <param name="param">Param value.</param>
        public QueryableWithParamMappingBuilder<TSource, TDest, TParam> WithParam<TParam>(TParam param)
        {
            return new QueryableWithParamMappingBuilder<TSource, TDest, TParam>(_sourceQueryable, QueryableFactory, param);
        }
    }

    /// <summary>
    /// Builder of query projection logic.
    /// </summary>
    /// <typeparam name="TSource">Source element type.</typeparam>
    /// <typeparam name="TDest">Destanation element type.</typeparam>
    /// <typeparam name="TSelect">Project expression container type.</typeparam>
    public struct QueryableMappingBuilder<TSelect, TSource, TDest>
        where TSelect : ISelectExpression<TSource, TDest>, new()
    {
        private readonly IQueryable<TSource> _sourceQueryable;

        internal QueryableMappingBuilder(IQueryable<TSource> sourceQueryable, QueryableFactory<TDest> queryableFactory)
        {
            _sourceQueryable = sourceQueryable;
            QueryableFactory = queryableFactory;

            QueryableFactory.Create = GetQueryable;
        }

        internal QueryableFactory<TDest> QueryableFactory { get; private set; }

        private IQueryable<TDest> GetQueryable()
        {
            return _sourceQueryable.Select(Mapper.SelectResolverWith0Params.GetExpression<TSelect, TSource, TDest>());
        }
    }

    /// <summary>
    /// Builder of query projection logic.
    /// </summary>
    /// <typeparam name="TSource">Source element type.</typeparam>
    /// <typeparam name="TDest">Destanation element type.</typeparam>
    /// <typeparam name="TParam">Parameter type.</typeparam>
    public struct QueryableWithParamMappingBuilder<TSource, TDest, TParam>
    {
        private readonly IQueryable<TSource> _sourceQueryable;
        private readonly TParam _param;

        internal QueryableWithParamMappingBuilder(IQueryable<TSource> sourceQueryable, QueryableFactory<TDest> queryableFactory, TParam param)
        {
            _sourceQueryable = sourceQueryable;
            QueryableFactory = queryableFactory;
            _param = param;

            QueryableFactory.Create = GetQueryable;
        }

        internal QueryableFactory<TDest> QueryableFactory { get; private set; }

        private IQueryable<TDest> GetQueryable()
        {
            return _sourceQueryable.Select(Mapper.SelectResolverWith1Params.GetExternalExpression<TSource, TDest, TParam>(_param));
        }

        /// <summary>
        /// Sets select expression container type. If Projection expression is not registered yet that activated TSelect() will be used for registration. 
        /// </summary>
        /// <typeparam name="TSelect">Projection select expression container.</typeparam>
        /// <returns>Builder of projection.</returns>
        public QueryableWithParamMappingBuilder<TSelect, TSource, TDest, TParam> Using<TSelect>()
            where TSelect : ISelectExpression<TSource, TDest, TParam>, new()
        {
            return new QueryableWithParamMappingBuilder<TSelect, TSource, TDest, TParam>(_sourceQueryable, QueryableFactory, _param);
        }
    }

    /// <summary>
    /// Builder of query projection logic.
    /// </summary>
    /// <typeparam name="TSource">Source element type.</typeparam>
    /// <typeparam name="TDest">Destanation element type.</typeparam>
    /// <typeparam name="TParam">Parameter type.</typeparam>
    /// <typeparam name="TSelect">Project expression container type.</typeparam>
    public struct QueryableWithParamMappingBuilder<TSelect, TSource, TDest, TParam>
        where TSelect: ISelectExpression<TSource, TDest, TParam>, new()
    {
        private readonly IQueryable<TSource> _sourceQueryable;
        private readonly TParam _param;

        internal QueryableWithParamMappingBuilder(IQueryable<TSource> sourceQueryable, QueryableFactory<TDest> queryableFactory, TParam param)
        {
            _sourceQueryable = sourceQueryable;
            QueryableFactory = queryableFactory;
            _param = param;

            QueryableFactory.Create = GetQueryable;
        }

        internal QueryableFactory<TDest> QueryableFactory { get; private set; }

        private IQueryable<TDest> GetQueryable()
        {
            return _sourceQueryable.Select(Mapper.SelectResolverWith1Params.GetExpression<TSelect, TSource, TDest, TParam>(_param));
        }
    }

    internal class QueryableFactory<TDest>
    {
       public Func<IQueryable<TDest>> Create { get; set; }
    }
}
