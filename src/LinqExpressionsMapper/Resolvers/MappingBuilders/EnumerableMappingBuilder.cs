using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LinqExpressionsMapper.Resolvers.MappingBuilders
{
    public struct EnumerableMappingBuilder<TSource> where TSource : class
    {
        private readonly IEnumerable<TSource> _sourceEnumerable;

        public EnumerableMappingBuilder(IEnumerable<TSource> sourceEnumerable)
        {
            _sourceEnumerable = sourceEnumerable;
        }

        public EnumerableMappingBuilder<TSource, TDest> To<TDest>() 
            where TDest : class, new()
        {
            return new EnumerableMappingBuilder<TSource, TDest>(_sourceEnumerable);
        }
    }

    public struct EnumerableMappingBuilder<TSource, TDest>
        where TSource : class
        where TDest : class, new()
    {
        private readonly IEnumerable<TSource> _sourceEnumerable;
        private IEnumerable<TDest> _resultEnumerable;

        public EnumerableMappingBuilder(IEnumerable<TSource> sourceEnumerable)
        {
            _sourceEnumerable = sourceEnumerable;
            _resultEnumerable = null;
        }

        public EnumerableMappingBuilder<TSelect, TSource, TDest> Using<TSelect>() 
            where TSelect : IPropertiesMapper<TSource, TDest>, new()
        {
            return new EnumerableMappingBuilder<TSelect, TSource, TDest>(_sourceEnumerable);
        }

        public IEnumerable<TDest> GetEnumerable()
        {
            return _sourceEnumerable.MapSelect<TSource, TDest>();
        }

        public IEnumerable<TDest> Enumerable
        {
            get { return _resultEnumerable ?? (_resultEnumerable = GetEnumerable()); }
        }
    }

    public struct EnumerableMappingBuilder<TSelect, TSource, TDest>
        where TSource : class
        where TDest : class, new()
        where TSelect : IPropertiesMapper<TSource, TDest>, new()
    {
        private readonly IEnumerable<TSource> _sourceEnumerable;
        private IEnumerable<TDest> _resultEnumerable;

        public EnumerableMappingBuilder(IEnumerable<TSource> sourceEnumerable)
        {
            _sourceEnumerable = sourceEnumerable;
            _resultEnumerable = null;
        }

        public IEnumerable<TDest> GetEnumerable()
        {
            return _sourceEnumerable.MapSelect<TSelect, TSource, TDest>();
            ;
        }

        public IEnumerable<TDest> Enumerable
        {
            get { return _resultEnumerable ?? (_resultEnumerable = GetEnumerable()); }
        }
    }
}
