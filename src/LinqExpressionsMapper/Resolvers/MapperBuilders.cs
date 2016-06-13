namespace LinqExpressionsMapper
{
    public partial class Mapper
    {
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
    }
}
