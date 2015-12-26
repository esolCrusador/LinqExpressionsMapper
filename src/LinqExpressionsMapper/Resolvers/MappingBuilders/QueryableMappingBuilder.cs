﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

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

    public class QueryableMappingBuilder<TSource, TDest> : IQueryable<TDest>
    {
        protected readonly IQueryable<TSource> _sourceQueryable;
        protected IQueryable<TDest> ResultQuery;

        public QueryableMappingBuilder(IQueryable<TSource> sourceQueryable)
        {
            _sourceQueryable = sourceQueryable;
        }

        protected IQueryable<TDest> ResultQueryable
        {
            get { return ResultQuery ?? (ResultQuery = Resolve(_sourceQueryable)); }
        }

        public IQueryable<TDest> Select()
        {
            return Resolve(_sourceQueryable);
        }

        public virtual IQueryable<TDest> Select<TParam>(TParam param)
        {
            return _sourceQueryable.Select(Mapper.From<TSource>().To<TDest>().GetExpression(param));
        }

        public QueryableMappingBuilder<TSelect, TSource, TDest> Using<TSelect>() 
            where TSelect : ISelectExpression<TSource, TDest>, new()
        {
            return new QueryableMappingBuilder<TSelect, TSource, TDest>(_sourceQueryable);
        }

        protected virtual IQueryable<TDest> Resolve(IQueryable<TSource> sourceQueryable )
        {
            return sourceQueryable.ResolveSelectExternal<TSource, TDest>();
        }

        #region IQueryable<TDest>

        public IEnumerator<TDest> GetEnumerator()
        {
            return ResultQueryable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression
        {
            get { return ResultQueryable.Expression; }
        }

        public Type ElementType
        {
            get { return ResultQueryable.ElementType; }
        }

        public IQueryProvider Provider
        {
            get { return ResultQueryable.Provider; }
        }

        #endregion
    }

    public class QueryableMappingBuilder<TSelect, TSource, TDest> : QueryableMappingBuilder<TSource, TDest> 
        where TSelect : ISelectExpression<TSource, TDest>, new()
    {
        public QueryableMappingBuilder(IQueryable<TSource> sourceQueryable) : base(sourceQueryable)
        {
        }

        protected override IQueryable<TDest> Resolve(IQueryable<TSource> sourceQueryable)
        {
            return sourceQueryable.ResolveSelect<TSelect, TSource, TDest>();
        }

        public override IQueryable<TDest> Select<TParam>(TParam param)
        {
            return _sourceQueryable.Select(Mapper.From<TSource>().To<TDest>().Using<TSelect>().GetExpression(param));
        }
    }
}
