using System;

namespace LinqExpressionsMapper
{
    /// <summary>
    /// Buider of properties mapping logic.
    /// </summary>
    /// <typeparam name="TSource">Source element type.</typeparam>
    public struct PropertiesMappingBuilder<TSource> where TSource : class
    {
        private readonly TSource _source;

        /// <summary>
        /// Creates instance of PropertiesMappingBuilder.
        /// </summary>
        /// <param name="source">Source element.</param>
        internal PropertiesMappingBuilder(TSource source)
        {
            _source = source;
        }

        /// <summary>
        /// Sets destanation element type.
        /// </summary>
        /// <typeparam name="TDest">Destanation element type.</typeparam>
        /// <returns>Builder of properties mapping logic.</returns>
        public PropertiesMappingBuilder<TSource, TDest> To<TDest>()
            where TDest : class, new()
        {
            return new PropertiesMappingBuilder<TSource, TDest>(_source);
        }

        /// <summary>
        /// Sets destanation element value.
        /// </summary>
        /// <typeparam name="TDest">Destanation element type.</typeparam>
        /// <param name="dest">Destanation element Value.</param>
        /// <returns>Builder of properties mapping logic.</returns>
        public PropertiesMappingBuilderWithDest<TSource, TDest> To<TDest>(TDest dest)
            where TDest : class
        {
            return new PropertiesMappingBuilderWithDest<TSource, TDest>(_source, dest);
        }
    }

    /// <summary>
    /// Buider of properties mapping logic.
    /// </summary>
    /// <typeparam name="TSource">Source element type.</typeparam>
    /// <typeparam name="TDest">Destanation element type.</typeparam>
    public struct PropertiesMappingBuilder<TSource, TDest>
        where TSource : class
        where TDest : class, new()
    {
        private readonly TSource _source;

        internal PropertiesMappingBuilder(TSource source)
        {
            _source = source;
        }

        /// <summary>
        /// Maps properties of source element to properties of destanation element.
        /// </summary>
        /// <returns>Destanation element instance.</returns>
        public TDest Map()
        {
            return Mapper.Map<TSource, TDest>(_source);
        }

        /// <summary>
        /// Sets PropertiesMapper type of source element to destanation element mapping.
        /// </summary>
        /// <typeparam name="TMapper">PropertiesMapper type.</typeparam>
        /// <returns>Builder of mapping logic.</returns>
        public PropertiesMappingBuilder<TMapper, TSource, TDest> Using<TMapper>()
            where TMapper : IPropertiesMapper<TSource, TDest>, new()
        {
            return new PropertiesMappingBuilder<TMapper, TSource, TDest>(_source);
        }

        /// <summary>
        /// Maps properties of source element to properties of destanation element.
        /// </summary>
        /// <param name="mappingBuilder">Mapping logic builder.</param>
        public static implicit operator TDest(PropertiesMappingBuilder<TSource, TDest> mappingBuilder)
        {
            return mappingBuilder.Map();
        }
    }

    /// <summary>
    /// Buider of properties mapping logic.
    /// </summary>
    /// <typeparam name="TSource">Source element type.</typeparam>
    /// <typeparam name="TDest">Destanation element type.</typeparam>
    /// <typeparam name="TMapper">PropertiesMapper type.</typeparam>
    public struct PropertiesMappingBuilder<TMapper, TSource, TDest>
        where TSource : class
        where TDest : class, new()
        where TMapper : IPropertiesMapper<TSource, TDest>, new()
    {
        private readonly TSource _source;

        internal PropertiesMappingBuilder(TSource source)
        {
            _source = source;
        }

        /// <summary>
        /// Maps properties of source element to properties of destanation element.
        /// </summary>
        /// <returns>Destanation element instance.</returns>
        public TDest Map()
        {
            return Mapper.Map<TMapper, TSource, TDest>(_source);
        }

        /// <summary>
        /// Maps properties of source element to properties of destanation element.
        /// </summary>
        /// <param name="mappingBuilder">Mapping logic builder.</param>
        public static implicit operator TDest(PropertiesMappingBuilder<TMapper, TSource, TDest> mappingBuilder)
        {
            return mappingBuilder.Map();
        }
    }

    /// <summary>
    /// Buider of properties mapping logic.
    /// </summary>
    /// <typeparam name="TSource">Source element type.</typeparam>
    /// <typeparam name="TDest">Destanation element type.</typeparam>
    public struct PropertiesMappingBuilderWithDest<TSource, TDest>
        where TSource : class
        where TDest : class
    {
        private readonly TSource _source;
        private readonly TDest _dest;

        internal PropertiesMappingBuilderWithDest(TSource source, TDest dest)
        {
            _source = source;
            _dest = dest;
        }

        /// <summary>
        /// Maps properties of source element to properties of destanation element.
        /// </summary>
        /// <returns>Destanation element instance.</returns>
        public TDest Map()
        {
            return Mapper.Map<TSource, TDest>(_source, _dest);
        }

        /// <summary>
        /// Sets PropertiesMapper type of source element to destanation element mapping.
        /// </summary>
        /// <typeparam name="TMapper">PropertiesMapper type.</typeparam>
        /// <returns>Builder of mapping logic.</returns>
        public PropertiesMappingBuilderWithDest<TMapper, TSource, TDest> Using<TMapper>()
            where TMapper : IPropertiesMapper<TSource, TDest>, new()
        {
            return new PropertiesMappingBuilderWithDest<TMapper, TSource, TDest>(_source, _dest);
        }

        /// <summary>
        /// Maps properties of source element to properties of destanation element.
        /// </summary>
        /// <param name="mappingBuilder">Mapping logic builder.</param>
        public static implicit operator TDest(PropertiesMappingBuilderWithDest<TSource, TDest> mappingBuilder)
        {
            return mappingBuilder.Map();
        }
    }

    /// <summary>
    /// Buider of properties mapping logic.
    /// </summary>
    /// <typeparam name="TSource">Source element type.</typeparam>
    /// <typeparam name="TDest">Destanation element type.</typeparam>
    public struct PropertiesMappingBuilderWithDest<TMapper, TSource, TDest>
        where TSource : class
        where TDest : class
        where TMapper : IPropertiesMapper<TSource, TDest>, new()
    {
        private readonly TSource _source;
        private readonly TDest _dest;

        internal PropertiesMappingBuilderWithDest(TSource source, TDest dest)
        {
            _source = source;
            _dest = dest;
        }

        /// <summary>
        /// Maps properties of source element to properties of destanation element.
        /// </summary>
        /// <returns>Destanation element instance.</returns>
        public TDest Map()
        {
            return Mapper.Map<TMapper, TSource, TDest>(_source, _dest);
        }

        /// <summary>
        /// Maps properties of source element to properties of destanation element.
        /// </summary>
        /// <param name="mappingBuilder">Mapping logic builder.</param>
        public static implicit operator TDest(PropertiesMappingBuilderWithDest<TMapper, TSource, TDest> mappingBuilder)
        {
            return mappingBuilder.Map();
        }
    }
}
