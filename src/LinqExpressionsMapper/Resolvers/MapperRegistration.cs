using System;
using System.Linq.Expressions;

namespace LinqExpressionsMapper
{
    public partial class Mapper
    {
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
                SelectResolverWith0Params.Register(selectExpression);
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
                SelectResolverWith0Params.Register(selectDynamicExpression);
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
        /// Registers Select Project Expression with one Param.
        /// </summary>
        /// <typeparam name="TSource">Source param type.</typeparam>
        /// <typeparam name="TDest">Destanation param type.</typeparam>
        /// <typeparam name="TParam1">Parameter param type.</typeparam>
        /// <typeparam name="TParam2">Parameter param type.</typeparam>
        /// <param name="selectExpression">Select Projection Expression factory.</param>
        public static void Register<TSource, TDest, TParam1, TParam2>(ISelectExpression<TSource, TDest, TParam1, TParam2> selectExpression)
        {
            if (!RegisterAllIfMultipleMappings(selectExpression))
                SelectResolverWith2Params.Register(selectExpression);
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
                MappingResolverWith0Params.Register(propertiesMapper);
        }

        /// <summary>
        /// Registers Properties Mapping Delegate.
        /// </summary>
        /// <typeparam name="TSource">Source param type.</typeparam>
        /// <typeparam name="TDest">Destanation param type.</typeparam>
        /// <typeparam name="TParam">Parameter type.</typeparam>
        /// <param name="propertiesMapper">Properties Mapping Delegate factory.</param>
        public static void Register<TSource, TDest, TParam>(IPropertiesMapper<TSource, TDest, TParam> propertiesMapper)
        {
            if (!RegisterAllIfMultipleMappings(propertiesMapper))
                MappingResolverWith1Params.Register(propertiesMapper);
        }

        /// <summary>
        /// Registers Properties Mapping Delegate.
        /// </summary>
        /// <typeparam name="TSource">Source param type.</typeparam>
        /// <typeparam name="TDest">Destanation param type.</typeparam>
        /// <typeparam name="TParam1">Parameter type.</typeparam>
        /// <typeparam name="TParam2"></typeparam>
        /// <param name="propertiesMapper">Properties Mapping Delegate factory.</param>
        public static void Register<TSource, TDest, TParam1, TParam2>(IPropertiesMapper<TSource, TDest, TParam1, TParam2> propertiesMapper)
        {
            if (!RegisterAllIfMultipleMappings(propertiesMapper))
                MappingResolverWith2Params.Register(propertiesMapper);
        }

        /// <summary>
        /// Registers all Properties Mappings & Projection Expressions factory methods of instnce.
        /// </summary>
        /// <param name="mapper">Multiply Mapping factories container.</param>
        public static void RegisterAll(IMultipleMappings mapper)
        {
            var mapperType = mapper.GetType();
            var implementedInterfaces = mapperType.GetInterfaces();

            MappingResolverWith0Params.RegisterAll(mapper, mapperType, implementedInterfaces);
            MappingResolverWith1Params.RegisterAll(mapper, mapperType, implementedInterfaces);
            MappingResolverWith2Params.RegisterAll(mapper, mapperType, implementedInterfaces);

            SelectResolverWith0Params.RegisterAll(mapper, mapperType, implementedInterfaces);
            SelectResolverWith1Params.RegisterAll(mapper, mapperType, implementedInterfaces);
            SelectResolverWith2Params.RegisterAll(mapper, mapperType, implementedInterfaces);
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
    }
}
