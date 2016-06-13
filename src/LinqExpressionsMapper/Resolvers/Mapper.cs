using System;
using System.Linq.Expressions;
using LinqExpressionsMapper.Resolvers.MappingResolver;
using LinqExpressionsMapper.Resolvers.SelectsResolver;

namespace LinqExpressionsMapper
{
    /// <summary>
    /// Mapper
    /// </summary>
    public static partial class Mapper
    {
        internal static readonly SelectResolverWith0Params SelectResolverWith0Params;
        internal static readonly SelectResolverWith1Params SelectResolverWith1Params;
        internal static readonly SelectResolverWith2Params SelectResolverWith2Params;

        internal static readonly MappingResolverWith0Params MappingResolverWith0Params;
        internal static readonly MappingResolverWith1Params MappingResolverWith1Params;
        internal static readonly MappingResolverWith2Params MappingResolverWith2Params;

        static Mapper()
        {
            SelectResolverWith0Params = new SelectResolverWith0Params();
            SelectResolverWith1Params = new SelectResolverWith1Params();
            SelectResolverWith2Params = new SelectResolverWith2Params();

            MappingResolverWith0Params = new MappingResolverWith0Params();
            MappingResolverWith1Params = new MappingResolverWith1Params();
            MappingResolverWith2Params = new MappingResolverWith2Params();
        }
    }
}
