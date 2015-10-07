using System;

namespace LinqExpressionsMapper.Resolvers.MapperResolver
{
   public interface IMappingResolver
   {
       void Register<TSource, TDest>(IPropertiesMapper<TSource, TDest> mapper);

       IPropertiesMapper<TSource, TDest> GetMapper<TSource, TDest>(TSource source, TDest dest);

       bool TryGetMapper<TSource, TDest>(out IPropertiesMapper<TSource, TDest> mapper);
   }
}
