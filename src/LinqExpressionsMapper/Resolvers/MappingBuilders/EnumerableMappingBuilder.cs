using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqExpressionsMapper
{
    /// <summary>
    /// Builder of Enumerable mapping logic.
    /// </summary>
    /// <typeparam name="TSource">Source Enumerable element type.</typeparam>
    public struct EnumerableMappingBuilder<TSource> where TSource : class
    {
        private readonly IEnumerable<TSource> _sourceEnumerable;

        internal EnumerableMappingBuilder(IEnumerable<TSource> sourceEnumerable)
        {
            _sourceEnumerable = sourceEnumerable;
        }

        /// <summary>
        /// Converts source Enumerable to target Enumerable using mapper.
        /// </summary>
        /// <typeparam name="TDest">Destanation model type.</typeparam>
        /// <returns>Enumerable of destanations.</returns>
        public IEnumerable<TDest> To<TDest>()
            where TDest : class, new()
        {
            return _sourceEnumerable.Select(Mapper.MappingResolverWith0Params.GetMapper<TSource, TDest>());
        }

        /// <summary>
        /// Converts source Enumerable to target Enumerable using mapper and options configured in config.
        /// </summary>
        /// <typeparam name="TDest">Destanation model type.</typeparam>
        /// <param name="configure">Configuration action. Here you can set mapping container using <see cref="EnumerableMappingBuilder{TSource,TDest}.Using{TSelect}"/> method.</param>
        /// <returns>Enumerables of destanations.</returns>
        public IEnumerable<TDest> To<TDest>(Action<EnumerableMappingBuilder<TSource, TDest>> configure)
            where TDest : class, new()
        {
            var mappingDelegateContainer = new EnumerableFactory<TDest>();
            configure(new EnumerableMappingBuilder<TSource, TDest>(_sourceEnumerable, mappingDelegateContainer));

            return mappingDelegateContainer.Create();
        }
    }

    /// <summary>
    /// Builder of Enumerable mapping logic.
    /// </summary>
    /// <typeparam name="TSource">Source Enumerable element type.</typeparam>
    /// <typeparam name="TDest">Destanation Enumerable element type.</typeparam>
    public struct EnumerableMappingBuilder<TSource, TDest>
        where TSource : class
        where TDest : class, new()
    {
        private readonly IEnumerable<TSource> _sourceEnumerable;
        internal EnumerableMappingBuilder(IEnumerable<TSource> sourceEnumerable, EnumerableFactory<TDest> enumerableFactory)
        {
            _sourceEnumerable = sourceEnumerable;
            EnumerableFactory = enumerableFactory;
            EnumerableFactory.Create = GetEnumerable;
        }

        internal EnumerableFactory<TDest> EnumerableFactory { get; private set; }

        /// <summary>
        /// Using Mapper type. If mapping from TSource to TDest is not cached then TSelect will be activated and used as properties mapper.
        /// </summary>
        /// <typeparam name="TSelect">Properties Mapper.</typeparam>
        /// <returns>New Builder.</returns>
        public EnumerableMappingBuilder<TSelect, TSource, TDest> Using<TSelect>()
            where TSelect : IPropertiesMapper<TSource, TDest>, new()
        {
            return new EnumerableMappingBuilder<TSelect, TSource, TDest>(_sourceEnumerable, EnumerableFactory);
        }

        private IEnumerable<TDest> GetEnumerable()
        {
            return _sourceEnumerable.Select(Mapper.MappingResolverWith0Params.GetMapper<TSource, TDest>());
        }
    }

    /// <summary>
    /// Builder of Enumerable Mapping logic.
    /// </summary>
    /// <typeparam name="TSelect">Properties Mapper.</typeparam>
    /// <typeparam name="TSource">Source Enumerable element type.</typeparam>
    /// <typeparam name="TDest">Destanation Enumerable element type.</typeparam>
    public struct EnumerableMappingBuilder<TSelect, TSource, TDest>
        where TSource : class
        where TDest : class, new()
        where TSelect : IPropertiesMapper<TSource, TDest>, new()
    {
        private readonly IEnumerable<TSource> _sourceEnumerable;

        internal EnumerableMappingBuilder(IEnumerable<TSource> sourceEnumerable, EnumerableFactory<TDest> enumerableFactory)
        {
            EnumerableFactory = enumerableFactory;
            _sourceEnumerable = sourceEnumerable;
            enumerableFactory.Create = GetEnumerable;
        }

        internal EnumerableFactory<TDest> EnumerableFactory { get; private set; }

        private IEnumerable<TDest> GetEnumerable()
        {
            return _sourceEnumerable.Select(Mapper.MappingResolverWith0Params.GetMapper<TSelect, TSource, TDest>());
        }
    }

    internal class EnumerableFactory<TDest>
    {
        public Func<IEnumerable<TDest>> Create { get; set; }
    }
}
