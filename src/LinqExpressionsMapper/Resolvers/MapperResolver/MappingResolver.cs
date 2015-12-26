using System;
using System.Collections.Generic;
using System.Linq;
using LinqExpressionsMapper.Models;

namespace LinqExpressionsMapper.Resolvers.MapperResolver
{
    internal class MappingResolver
    {
        #region Private Fields

        private readonly object _sync = new object();
        private readonly Dictionary<PairId, object> _cache = new Dictionary<PairId, object>();

        #endregion

        #region Implemetation Of IMappingResolver

        public void Register<TSource, TDest>(IPropertiesMapper<TSource, TDest> mapper)
        {
            lock (_sync)
            {
                PairId id = PairId.GetId<TSource, TDest>();
                if (_cache.ContainsKey(id))
                {
                    _cache[id] = mapper;
                }
                else
                {
                    _cache.Add(PairId.GetId<TSource, TDest>(), mapper);
                }
            }
        }

        public void RegisterAll(object mapper, Type interfaces, Type[] implementedInterfaces)
        {
            lock (_sync)
            {
                Type propertiesMapperType = typeof (IPropertiesMapper<,>);
                foreach (Type mapperType in implementedInterfaces.Where(i=>i.GUID==propertiesMapperType.GUID))
                {
                    Type[] genericArgs = mapperType.GetGenericArguments();

                    var pairId = PairId.GetId(genericArgs[0], genericArgs[1]);

                    if (_cache.ContainsKey(pairId))
                    {
                        _cache[pairId] = mapper;
                    }
                    else
                    {
                        _cache.Add(pairId, mapper);
                    }
                }
            }
        }

        public IPropertiesMapper<TSource, TDest> GetMapper<TSource, TDest>(TSource source, TDest dest)
        {
            PairId pairId = PairId.GetId<TSource, TDest>();

            object mapperObject;

            if (_cache.TryGetValue(pairId, out mapperObject))
            {
                return (IPropertiesMapper<TSource, TDest>) mapperObject;
            }

            lock (_sync)
            {
                IPropertiesMapper<TSource, TDest> mapper;

                if (_cache.TryGetValue(pairId, out mapperObject))
                {
                    mapper = (IPropertiesMapper<TSource, TDest>) mapperObject;
                }
                else
                {
                    if ((mapper = dest as IPropertiesMapper<TSource, TDest>) != null)
                    {
                        _cache.Add(pairId, mapper);
                    }
                    else
                    {
                        if ((mapper = source as IPropertiesMapper<TSource, TDest>) != null)
                        {
                            _cache.Add(pairId, mapper);
                        }
                        else
                        {
                            throw new NotSupportedException(
                                String.Format("Convert expression for {0}->{1} does not exist.",
                                    typeof (TSource).FullName, typeof (TDest).FullName));
                        }
                    }
                }

                return mapper;
            }
        }

        public bool TryGetMapper<TSource, TDest>(out IPropertiesMapper<TSource, TDest> mapper)
        {
            PairId pairId = PairId.GetId<TSource, TDest>();
            object mapperObject;

            if (_cache.TryGetValue(pairId, out mapperObject))
            {
                mapper = (IPropertiesMapper<TSource, TDest>) mapperObject;
                return true;
            }
            else
            {
                mapper = null;
                return false;
            }
        }

        #endregion
    }
}
