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

        public IEnumerable<TDest> To<TDest>() 
            where TDest : class, new()
        {
            return _sourceEnumerable.MapSelect<TSource, TDest>();
        }

        public IEnumerable<TDest> To<TDest>(Action<EnumerableMappingBuilder<TSource, TDest>> configure)
            where TDest : class, new()
        {
            var mappingDelegateContainer = new EnumerableFactory<TDest>();
            configure(new EnumerableMappingBuilder<TSource, TDest>(_sourceEnumerable, mappingDelegateContainer));

            return mappingDelegateContainer.Create();
        }
    }

    public struct EnumerableMappingBuilder<TSource, TDest>
        where TSource : class
        where TDest : class, new()
    {
        private readonly IEnumerable<TSource> _sourceEnumerable;
        public EnumerableMappingBuilder(IEnumerable<TSource> sourceEnumerable, EnumerableFactory<TDest> enumerableFactory)
        {
            _sourceEnumerable = sourceEnumerable;
            EnumerableFactory = enumerableFactory;
            EnumerableFactory.Create = GetEnumerable;
        }

        internal EnumerableFactory<TDest> EnumerableFactory { get; private set; }

        public EnumerableMappingBuilder<TSelect, TSource, TDest> Using<TSelect>() 
            where TSelect : IPropertiesMapper<TSource, TDest>, new()
        {
            return new EnumerableMappingBuilder<TSelect, TSource, TDest>(_sourceEnumerable, EnumerableFactory);
        }

        private IEnumerable<TDest> GetEnumerable()
        {
            return _sourceEnumerable.MapSelect<TSource, TDest>();
        }
    }

    public struct EnumerableMappingBuilder<TSelect, TSource, TDest>
        where TSource : class
        where TDest : class, new()
        where TSelect : IPropertiesMapper<TSource, TDest>, new()
    {
        private readonly IEnumerable<TSource> _sourceEnumerable;

        public EnumerableMappingBuilder(IEnumerable<TSource> sourceEnumerable, EnumerableFactory<TDest> enumerableFactory)
        {
            EnumerableFactory = enumerableFactory;
            _sourceEnumerable = sourceEnumerable;
            enumerableFactory.Create = GetEnumerable;
        }

        internal EnumerableFactory<TDest> EnumerableFactory { get; private set; }

        private IEnumerable<TDest> GetEnumerable()
        {
            return _sourceEnumerable.MapSelect<TSelect, TSource, TDest>();
        }
    }

    public class EnumerableFactory<TDest>
    {
        public Func<IEnumerable<TDest>> Create { get; set; }
    }
}
