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

        public QueryableMappingBuilder<TSource, TDest> To<TDest>()
        {
            return new QueryableMappingBuilder<TSource, TDest>(_sourceQueryable);
        }
    }

    public struct QueryableMappingBuilder<TSource, TDest>
    {
        private readonly IQueryable<TSource> _sourceQueryable;
        private IQueryable<TDest> _resultQuery;

        public QueryableMappingBuilder(IQueryable<TSource> sourceQueryable)
        {
            _sourceQueryable = sourceQueryable;
            _resultQuery = null;
        }

        public IQueryable<TDest> GetQueryable()
        {
            return _sourceQueryable.ResolveSelectExternal<TSource, TDest>();
        }

        public IQueryable<TDest> Querable
        {
            get { return _resultQuery ?? (_resultQuery = GetQueryable()); }
        }

        public IQueryable<TDest> GetQueryable<TParam>(TParam param)
        {
            return _sourceQueryable.Select(Mapper.From<TSource>().To<TDest>().GetExpression(param));
        }

        public QueryableMappingBuilder<TSelect, TSource, TDest> Using<TSelect>() 
            where TSelect : ISelectExpression<TSource, TDest>, new()
        {
            return new QueryableMappingBuilder<TSelect, TSource, TDest>(_sourceQueryable);
        }
    }

    public struct QueryableMappingBuilder<TSelect, TSource, TDest>
        where TSelect : ISelectExpression<TSource, TDest>, new()
    {
        private readonly IQueryable<TSource> _sourceQueryable;
        private IQueryable<TDest> _resultQuery;

        public QueryableMappingBuilder(IQueryable<TSource> sourceQueryable)
        {
            _sourceQueryable = sourceQueryable;
            _resultQuery = null;
        }

        public IQueryable<TDest> Queryable
        {
            get { return _resultQuery ?? (_resultQuery = GetQueryable()); }
        }

        public IQueryable<TDest> GetQueryable()
        {
            return _sourceQueryable.ResolveSelect<TSelect, TSource, TDest>();
        }
        public IQueryable<TDest> GetQueryable<TParam>(TParam param)
        {
            return _sourceQueryable.Select(Mapper.From<TSource>().To<TDest>().Using<TSelect>().GetExpression(param));
        }
    }
}
