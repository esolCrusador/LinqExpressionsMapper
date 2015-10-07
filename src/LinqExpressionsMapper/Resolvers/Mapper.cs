using System.Globalization;
using System.Linq.Expressions;
using GloryS.Common.Resolvers.SelectsResolver;
using LinqExpressionsMapper.Resolvers.MapperResolver;

namespace System
{
    public static class Mapper
    {
        private static readonly ISelectResolver SelectResolver;
        private static readonly IMappingResolver MappingResolver;

        static Mapper()
        {
            SelectResolver = LinqExpressionsMapper.Resolvers.SelectsResolver.SelectResolver.Instance;
            MappingResolver = LinqExpressionsMapper.Resolvers.MapperResolver.MappingResolver.Instance;
        }

        #region Registration
        
        public static void Register<TSource, TDest>(ISelectExpression<TSource, TDest> selectExpression)
        {
            SelectResolver.Register(selectExpression);
        }

        public static void Register<TSource, TDest>(ISelectExpressionNonCache<TSource, TDest> selectExpression)
        {
            SelectResolver.Register(selectExpression);
        }

        public static void Register<TSource, TDest>(ICultureSelectExpression<TSource, TDest> selectExpression)
        {
            SelectResolver.Register(selectExpression);
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

        public static Expression<Func<TSource, TDest>> GetExternalExpression<TSource, TDest>(Culture cultureId)
        {
            return SelectResolver.GetExternalExpression<TSource, TDest>(cultureId);
        }

        public static Expression<Func<TSource, TDist>> GetExpression<TSource, TDist>() where TDist : ISelectExpression<TSource, TDist>, new()
        {
            return SelectResolver.GetExpression<TSource, TDist>();
        }

        public static Expression<Func<TSource, TDest>> GetExpression<TSource, TDest>(Culture cultreId) where TDest : ICultureSelectExpression<TSource, TDest>, new()
        {
            return SelectResolver.GetExpression<TSource, TDest>(cultreId);
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
            where TMapper : class, new()
            where TDest : class
            where TSource : class
        {
            IPropertiesMapper<TSource, TDest> tMapper;
            if (!MappingResolver.TryGetMapper(out tMapper))
            {
                tMapper = (IPropertiesMapper<TSource, TDest>) new TMapper();
                MappingResolver.Register(tMapper);
            }

            tMapper.MapProperties(source, dest);

            return dest;
        }

        #endregion
    }
}
