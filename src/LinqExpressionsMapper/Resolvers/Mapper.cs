using System;
using System.Linq.Expressions;
using LinqExpressionsMapper.Resolvers.MapperResolver;
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
        
        public static void Register<TSource, TDest>(ISelectExpression<TSource, TDest> selectExpression)
        {
            SelectResolver.Register(selectExpression);
        }

        public static void Register<TSource, TDest>(ISelectDynamicExpression<TSource, TDest> selectDynamicExpression)
        {
            SelectResolver.Register(selectDynamicExpression);
        }

        public static void Register<TSource, TDest, TParam>(ISelectExpression<TSource, TDest, TParam> selectExpression)
        {
            SelectResolverWith1Params.Register(selectExpression);
        }

        public static void Register<TSource, TDest>(IPropertiesMapper<TSource, TDest> propertiesMapper)
        {
            MappingResolver.Register(propertiesMapper);
        }

        #endregion

        #region Select Resolver

        public static Expression<Func<TSource, TDist>> GetExternalExpression<TSource, TDist>()
        {
            return SelectResolver.GetExternalExpression<TSource, TDist>();
        }

        public static Expression<Func<TSource, TDest>> GetExternalExpression<TSource, TDest, TParam>(TParam param)
        {
            return SelectResolverWith1Params.GetExternalExpression<TSource, TDest, TParam>(param);
        }

        public static Expression<Func<TSource, TDest>> GetExpression<TSource, TDest>() where TDest : ISelectExpression<TSource, TDest>, new()
        {
            return SelectResolver.GetExpression<TSource, TDest>();
        }

        public static Expression<Func<TSource, TDest>> GetExpression<TSource, TDest, TParam>(TParam param) where TDest : ISelectExpression<TSource, TDest, TParam>, new()
        {
            return SelectResolverWith1Params.GetExpression<TSource, TDest, TParam>(param);
        }

        public static Expression<Func<TSource, TDest>> GetExpression<TSelect, TSource, TDest>()
            where TSelect : ISelectExpression<TSource, TDest>, new()
        {
            Expression<Func<TSource, TDest>> result;

            if (!SelectResolver.TryGetFromCache(out result))
            {
                var resolver = new TSelect();
                SelectResolver.Register(resolver);

                result = SelectResolver.GetExternalExpression<TSource, TDest>();
            }

            return result;
        }

        #endregion

        #region Mapping Resolver

        public static TDest Map<TSource, TDest>(TSource source)
            where TDest : class, new()
            where TSource : class
        {
            TDest dest = new TDest();

            IPropertiesMapper<TSource, TDest> mapper = MappingResolver.GetMapper(source, dest);

            mapper.MapProperties(source, dest);

            return dest;
        }

        public static TDest Map<TSource, TDest>(TSource source, TDest dest)
            where TDest : class
            where TSource : class
        {
            IPropertiesMapper<TSource, TDest> mapper = MappingResolver.GetMapper(source, dest);

            mapper.MapProperties(source, dest);

            return dest;
        }

        public static TDest Map<TMapper, TSource, TDest>(TSource source, TDest dest)
            where TMapper : IPropertiesMapper<TSource, TDest>, new()
            where TDest : class
            where TSource : class
        {
            IPropertiesMapper<TSource, TDest> tMapper;
            if (!MappingResolver.TryGetMapper(out tMapper))
            {
                tMapper = new TMapper();
                MappingResolver.Register(tMapper);
            }

            tMapper.MapProperties(source, dest);

            return dest;
        }

        public static TDest Map<TMapper, TSource, TDest>(TSource source)
            where TMapper : IPropertiesMapper<TSource, TDest>, new()
            where TDest : class, new()
            where TSource : class
        {
            var dest = new TDest();

            return Map<TMapper, TSource, TDest>(source, dest);
        }

        #endregion
    }
}
