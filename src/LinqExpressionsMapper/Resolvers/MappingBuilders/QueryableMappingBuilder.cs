using System;
using System.Linq;
using System.Linq.Expressions;

namespace LinqExpressionsMapper.Resolvers.MappingBuilders
{
    public struct QueryableMappingBuilder<TSource>
    {
        private readonly IQueryable<TSource> _sourceQueryable;

        public QueryableMappingBuilder(IQueryable<TSource> sourceQueryable)
        {
            _sourceQueryable = sourceQueryable;
        }

        public IQueryable<TDest> To<TDest>()
        {
            return _sourceQueryable.ResolveSelectExternal<TSource, TDest>();
        }

        public IQueryable<TDest> To<TDest>(Action<QueryableMappingBuilder<TSource, TDest>> configure)
        {
            var factory = new QueryableFactory<TDest>();
            configure(new QueryableMappingBuilder<TSource, TDest>(_sourceQueryable, factory));

            return factory.Create();
        }
    }

    public struct QueryableMappingBuilder<TSource, TDest>
    {
        private readonly IQueryable<TSource> _sourceQueryable;

        public QueryableMappingBuilder(IQueryable<TSource> sourceQueryable, QueryableFactory<TDest> queryableFactory)
        {
            _sourceQueryable = sourceQueryable;
            QueryableFactory = queryableFactory;
            QueryableFactory.Create = GetQueryable;
        }

        internal QueryableFactory<TDest> QueryableFactory { get; private set; }

        private IQueryable<TDest> GetQueryable()
        {
            return _sourceQueryable.ResolveSelectExternal<TSource, TDest>();
        }

        private IQueryable<TDest> GetQueryable<TParam>(TParam param)
        {
            return _sourceQueryable.Select(Mapper.From<TSource>().To<TDest>().GetExpression(param));
        }

        public QueryableMappingBuilder<TSelect, TSource, TDest> Using<TSelect>() 
            where TSelect : ISelectExpression<TSource, TDest>, new()
        {
            return new QueryableMappingBuilder<TSelect, TSource, TDest>(_sourceQueryable, QueryableFactory);
        }

        public void WithParam<TParam>(TParam param)
        {
            Func<TParam, IQueryable<TDest>> getQueryable = GetQueryable;
            QueryableFactory.Create = () => getQueryable(param);
        }
    }

    public struct QueryableMappingBuilder<TSelect, TSource, TDest>
        where TSelect : ISelectExpression<TSource, TDest>, new()
    {
        private readonly IQueryable<TSource> _sourceQueryable;

        public QueryableMappingBuilder(IQueryable<TSource> sourceQueryable, QueryableFactory<TDest> queryableFactory)
        {
            _sourceQueryable = sourceQueryable;
            QueryableFactory = queryableFactory;
            QueryableFactory.Create = GetQueryable;
        }

        internal QueryableFactory<TDest> QueryableFactory { get; private set; }

        private IQueryable<TDest> GetQueryable()
        {
            return _sourceQueryable.ResolveSelect<TSelect, TSource, TDest>();
        }
        private IQueryable<TDest> GetQueryable<TParam>(TParam param)
        {
            return _sourceQueryable.Select(Mapper.From<TSource>().To<TDest>().Using<TSelect>().GetExpression(param));
        }

        public void WithParam<TParam>(TParam param)
        {
            Func<TParam, IQueryable<TDest>> getQueryable = GetQueryable;
            QueryableFactory.Create = () => getQueryable(param);
        }
    }

    public class QueryableFactory<TDest>
    {
       public Func<IQueryable<TDest>> Create { get; set; }
    }
}
