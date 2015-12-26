using System;
using System.Runtime.Remoting.Messaging;

namespace LinqExpressionsMapper
{
    public struct PropertiesMappingBuilder<TSource> where TSource : class
    {
        private readonly TSource _source;

        /// <summary>
        /// Creates instance of PropertiesMappingBuilder
        /// </summary>
        /// <param name="source">Source Model</param>
        public PropertiesMappingBuilder(TSource source)
        {
            _source = source;
        }

        public PropertiesMappingBuilder<TSource, TDest> To<TDest>()
            where TDest : class, new()
        {
            return new PropertiesMappingBuilder<TSource, TDest>(_source);
        }

        public PropertiesMappingBuilderWithDest<TSource, TDest> To<TDest>(TDest dest)
            where TDest : class
        {
            return new PropertiesMappingBuilderWithDest<TSource, TDest>(_source, dest);
        }
    }

    public struct PropertiesMappingBuilder<TSource, TDest>
        where TSource : class
        where TDest : class, new()
    {
        private readonly TSource _source;

        public PropertiesMappingBuilder(TSource source)
        {
            _source = source;
        }

        public TDest Map()
        {
            return Mapper.Map<TSource, TDest>(_source);
        }

        public PropertiesMappingBuilder<TMapper, TSource, TDest> Using<TMapper>()
            where TMapper : IPropertiesMapper<TSource, TDest>, new()
        {
            return new PropertiesMappingBuilder<TMapper, TSource, TDest>(_source);
        }

        public static implicit operator TDest(PropertiesMappingBuilder<TSource, TDest> mappingBuilder)
        {
            return mappingBuilder.Map();
        }
    }

    public struct PropertiesMappingBuilder<TMapper, TSource, TDest>
        where TSource : class
        where TDest : class, new()
        where TMapper : IPropertiesMapper<TSource, TDest>, new()
    {
        private readonly TSource _source;

        public PropertiesMappingBuilder(TSource source)
        {
            _source = source;
        }

        public TDest Map()
        {
            return Mapper.Map<TMapper, TSource, TDest>(_source);
        }

        public static implicit operator TDest(PropertiesMappingBuilder<TMapper, TSource, TDest> mappingBuilder)
        {
            return mappingBuilder.Map();
        }
    }

    public struct PropertiesMappingBuilderWithDest<TSource, TDest>
    where TSource : class
    where TDest : class
    {
        private readonly TSource _source;
        private readonly TDest _dest;

        public PropertiesMappingBuilderWithDest(TSource source, TDest dest)
        {
            _source = source;
            _dest = dest;
        }

        public TDest Map()
        {
            return Mapper.Map<TSource, TDest>(_source, _dest);
        }

        public PropertiesMappingBuilderWithDest<TMapper, TSource, TDest> Using<TMapper>()
            where TMapper : IPropertiesMapper<TSource, TDest>, new()
        {
            return new PropertiesMappingBuilderWithDest<TMapper, TSource, TDest>(_source, _dest);
        }

        public static implicit operator TDest(PropertiesMappingBuilderWithDest<TSource, TDest> mappingBuilder)
        {
            return mappingBuilder.Map();
        }
    }

    public struct PropertiesMappingBuilderWithDest<TMapper, TSource, TDest>
        where TSource : class
        where TDest : class
        where TMapper : IPropertiesMapper<TSource, TDest>, new()
    {
        private TSource _source;
        private readonly TDest _dest;

        public PropertiesMappingBuilderWithDest(TSource source, TDest dest)
        {
            _source = source;
            _dest = dest;
        }

        public TDest Map()
        {
            return Mapper.Map<TMapper, TSource, TDest>(_source, _dest);
        }

        public static implicit operator TDest(PropertiesMappingBuilderWithDest<TMapper, TSource, TDest> mappingBuilder)
        {
            return mappingBuilder.Map();
        }
    }
}
