using System;
using System.Linq.Expressions;
using LinqExpressionsMapper.Resolvers.MappingResolver;
using LinqExpressionsMapper.Resolvers.SelectsResolver;

namespace LinqExpressionsMapper
{
    public static class Mapper
    {
        private static readonly SelectResolverWith0Params SelectResolver;
        private static readonly SelectResolverWith1Params SelectResolverWith1Params;
        private static readonly MappingResolver MappingResolver;

        static Mapper()
        {
            SelectResolver = new SelectResolverWith0Params();
            SelectResolverWith1Params = new SelectResolverWith1Params();
            MappingResolver = new MappingResolver();
        }

        #region Registration

        /// <summary>
        /// Registers Select Project Expression.
        /// </summary>
        /// <typeparam name="TSource">Source param type.</typeparam>
        /// <typeparam name="TDest">Destanation param type.</typeparam>
        /// <param name="selectExpression">Select Projection Expression factory.</param>
        public static void Register<TSource, TDest>(ISelectExpression<TSource, TDest> selectExpression)
        {
            if (!RegisterAllIfMultipleMappings(selectExpression))
                SelectResolver.Register(selectExpression);
        }

        /// <summary>
        /// Registers Select Projection Expression Factory.
        /// </summary>
        /// <typeparam name="TSource">Source param type.</typeparam>
        /// <typeparam name="TDest">Destanation param type.</typeparam>
        /// <param name="selectDynamicExpression">Select Dynamic Projection Expression factory.</param>
        public static void Register<TSource, TDest>(ISelectDynamicExpression<TSource, TDest> selectDynamicExpression)
        {
            if (!RegisterAllIfMultipleMappings(selectDynamicExpression))
                SelectResolver.Register(selectDynamicExpression);
        }

        /// <summary>
        /// Registers Select Project Expression with one Param.
        /// </summary>
        /// <typeparam name="TSource">Source param type.</typeparam>
        /// <typeparam name="TDest">Destanation param type.</typeparam>
        /// <typeparam name="TParam">Parameter param type.</typeparam>
        /// <param name="selectExpression">Select Projection Expression factory.</param>
        public static void Register<TSource, TDest, TParam>(ISelectExpression<TSource, TDest, TParam> selectExpression)
        {
            if (!RegisterAllIfMultipleMappings(selectExpression))
                SelectResolverWith1Params.Register(selectExpression);
        }

        /// <summary>
        /// Registers Properties Mapping Delegate.
        /// </summary>
        /// <typeparam name="TSource">Source param type.</typeparam>
        /// <typeparam name="TDest">Destanation param type.</typeparam>
        /// <param name="propertiesMapper">Properties Mapping Delegate factory.</param>
        public static void Register<TSource, TDest>(IPropertiesMapper<TSource, TDest> propertiesMapper)
        {
            if (!RegisterAllIfMultipleMappings(propertiesMapper))
                MappingResolver.Register(propertiesMapper);
        }

        /// <summary>
        /// Registers all Properties Mappings & Projection Expressions factory methods of instnce.
        /// </summary>
        /// <param name="mapper">Multiply Mapping factories container.</param>
        public static void RegisterAll(IMultipleMappings mapper)
        {
            var mapperType = mapper.GetType();
            var implementedInterfaces = mapperType.GetInterfaces();

            MappingResolver.RegisterAll(mapper, mapperType, implementedInterfaces);
            SelectResolver.RegisterAll(mapper, mapperType, implementedInterfaces);
            SelectResolverWith1Params.RegisterAll(mapper, mapperType, implementedInterfaces);
        }

        private static bool RegisterAllIfMultipleMappings(object mapper)
        {
            if (mapper is IMultipleMappings)
            {
                RegisterAll((IMultipleMappings)mapper);
                return true;
            }
            return false;
        }

        #endregion

        #region Builders

        /// <summary>
        /// Creates Properties Mapping logic builder.
        /// </summary>
        /// <typeparam name="TSource">Source element type.</typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static PropertiesMappingBuilder<TSource> From<TSource>(TSource source)
            where TSource : class
        {
            return new PropertiesMappingBuilder<TSource>(source);
        }

        /// <summary>
        /// Create Projection Expression logic builder.
        /// </summary>
        /// <typeparam name="TSource">Source element type.</typeparam>
        /// <returns>Projection Expression logic builder.</returns>
        public static ExpressionMappingBuilder<TSource> From<TSource>()
        {
            return new ExpressionMappingBuilder<TSource>();
        }

        #endregion

        #region SelectWith Resolver

        internal static Expression<Func<TSource, TDest>> GetExternalExpression<TSource, TDest>()
        {
            return SelectResolver.GetExternalExpression<TSource, TDest>();
        }

        internal static Expression<Func<TSource, TDest>> GetExternalExpression<TSource, TDest, TParam>(TParam param)
        {
            return SelectResolverWith1Params.GetExternalExpression<TSource, TDest, TParam>(param);
        }

        internal static Expression<Func<TSource, TDest>> GetExpression<TSource, TDest>() where TDest : ISelectExpression<TSource, TDest>, new()
        {
            return SelectResolver.GetExpression<TSource, TDest>();
        }

        internal static Expression<Func<TSource, TDest>> GetExpression<TSource, TDest, TParam>(TParam param) where TDest : ISelectExpression<TSource, TDest, TParam>, new()
        {
            return SelectResolverWith1Params.GetExpression<TSource, TDest, TParam>(param);
        }

        internal static Expression<Func<TSource, TDest>> GetExpression<TSelect, TSource, TDest>()
            where TSelect : ISelectExpression<TSource, TDest>, new()
        {
            Expression<Func<TSource, TDest>> result;

            if (!SelectResolver.TryGetFromCache(out result))
            {
                var resolver = new TSelect();
                Register(resolver);

                result = SelectResolver.GetExternalExpression<TSource, TDest>();
            }

            return result;
        }

        internal static Expression<Func<TSource, TDest>> GetExpression<TSelect, TSource, TDest, TParam>(TParam param)
            where TSelect : ISelectExpression<TSource, TDest, TParam>, new()
        {
            Expression<Func<TSource, TDest>> result;

            if (!SelectResolverWith1Params.TryGetFromCache(param, out result))
            {
                var resolver = new TSelect();
                Register(resolver);

                result = SelectResolverWith1Params.GetExternalExpression<TSource, TDest, TParam>(param);
            }

            return result;
        }

        #endregion

        #region Mapping Resolver

        internal static TDest Map<TSource, TDest>(TSource source)
            where TDest : class, new()
            where TSource : class
        {
            TDest dest = new TDest();

            Action<TSource, TDest> mappingAction = MappingResolver.GetMapper(source, dest);

            mappingAction(source, dest);

            return dest;
        }

        internal static TDest Map<TSource, TDest>(TSource source, TDest dest)
            where TDest : class
            where TSource : class
        {
            Action<TSource, TDest> mappingAction = MappingResolver.GetMapper(source, dest);

            mappingAction(source, dest);

            return dest;
        }

        internal static TDest Map<TMapper, TSource, TDest>(TSource source, TDest dest)
            where TMapper : IPropertiesMapper<TSource, TDest>, new()
            where TDest : class
            where TSource : class
        {
            Action<TSource, TDest> mappingAction;
            if (!MappingResolver.TryGetMapperFromCache(out mappingAction))
            {
                var mapper = new TMapper();
                mappingAction = mapper.MapProperties;
                Register(mapper);
            }

            mappingAction(source, dest);

            return dest;
        }

        internal static TDest Map<TMapper, TSource, TDest>(TSource source)
            where TMapper : IPropertiesMapper<TSource, TDest>, new()
            where TDest : class, new()
            where TSource : class
        {
            var dest = new TDest();

            return Map<TMapper, TSource, TDest>(source, dest);
        }

        internal static Func<TSource, TDest> GetMapper<TSource, TDest>()
            where TDest : new()
        {
            return MappingResolver.GetMapper<TSource, TDest>();
        }

        internal static Func<TSource, TDest> GetMapper<TMapper, TSource, TDest>()
            where TMapper : IPropertiesMapper<TSource, TDest>, new()
            where TDest : new()
        {
            Func<TSource, TDest> mappingFunc;
            if (!MappingResolver.TryGetMapperFromCache(out mappingFunc))
            {
                Register(new TMapper());
                mappingFunc = MappingResolver.GetMapper<TSource, TDest>();
            }

            return mappingFunc;
        }

        #endregion
    }
}
