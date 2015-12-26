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

    public class EnumerableMappingBuilder<TSource, TDest> : IEnumerable<TDest>
        where TSource : class
        where TDest : class, new()
    {
        private readonly IEnumerable<TSource> _sourceEnumerable;
        private IEnumerable<TDest> _resultEnumerable;

        public EnumerableMappingBuilder(IEnumerable<TSource> sourceEnumerable)
        {
            _sourceEnumerable = sourceEnumerable;
        }

        public EnumerableMappingBuilder<TSelect, TSource, TDest> Using<TSelect>() 
            where TSelect : IPropertiesMapper<TSource, TDest>, new()
        {
            return new EnumerableMappingBuilder<TSelect, TSource, TDest>(_sourceEnumerable);
        }

        public IEnumerable<TDest> Select()
        {
            return ResultEnumerable;
        }

        protected virtual IEnumerable<TDest> Resolve(IEnumerable<TSource> source)
        {
            return source.MapSelect<TSource, TDest>();
        }

        protected IEnumerable<TDest> ResultEnumerable
        {
            get { return _resultEnumerable ?? (_resultEnumerable = Resolve(_sourceEnumerable)); }
        }

        #region IEnumerable<TDest>

        public IEnumerator<TDest> GetEnumerator()
        {
            return ResultEnumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    public class EnumerableMappingBuilder<TSelect, TSource, TDest> : EnumerableMappingBuilder<TSource, TDest>
       where TSource : class
       where TDest : class, new() 
        where TSelect : IPropertiesMapper<TSource, TDest>, new()
    {
        public EnumerableMappingBuilder(IEnumerable<TSource> sourceEnumerable) : base(sourceEnumerable)
        {
        }

        protected override IEnumerable<TDest> Resolve(IEnumerable<TSource> source)
        {
            return source.MapSelect<TSelect, TSource, TDest>();
        }
    }
}
