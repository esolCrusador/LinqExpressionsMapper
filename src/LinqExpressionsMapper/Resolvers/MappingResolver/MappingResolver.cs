using System;
using System.Collections.Generic;
using System.Linq;
using LinqExpressionsMapper.Models;

namespace LinqExpressionsMapper.Resolvers.MappingResolver
{
    internal class MappingResolver
    {
        #region Private Fields

        private readonly object _sync = new object();
        private readonly Dictionary<PairId, Delegate> _cache = new Dictionary<PairId, Delegate>();

        #endregion

        #region Implemetation Of IMappingResolver

        public void Register<TSource, TDest>(IPropertiesMapper<TSource, TDest> mapper)
        {
            lock (_sync)
            {
                PairId id = PairId.GetId<TSource, TDest>();
                if (_cache.ContainsKey(id))
                {
                    _cache[id] = (Action<TSource, TDest>) mapper.MapProperties;
                }
                else
                {
                    _cache.Add(PairId.GetId<TSource, TDest>(), (Action<TSource, TDest>) mapper.MapProperties);
                }
            }
        }

        public void RegisterAll(object mapper, Type interfaces, Type[] implementedInterfaces)
        {
            lock (_sync)
            {
                Type propertiesMapperType = typeof (IPropertiesMapper<,>);
                foreach (Type interfaceType in implementedInterfaces.Where(i=>i.GUID==propertiesMapperType.GUID))
                {
                    Type[] genericArgs = interfaceType.GetGenericArguments();

                    var pairId = PairId.GetId(genericArgs[0], genericArgs[1]);

                    var methodInfo = interfaces.GetInterfaceMap(interfaceType).InterfaceMethods.Single();
                    Type delegateType = typeof (Action<,>).MakeGenericType(genericArgs[0], genericArgs[1]);
                    var factoryDelegate = Delegate.CreateDelegate(delegateType, mapper, methodInfo, true);

                    if (_cache.ContainsKey(pairId))
                    {
                        _cache[pairId] = factoryDelegate;
                    }
                    else
                    {
                        _cache.Add(pairId, factoryDelegate);
                    }
                }
            }
        }

        private static Func<TSource, TDest> GetMappingDelegate<TSource, TDest>(Action<TSource, TDest> mapAction) 
            where TDest : new()
        {
            return source =>
            {
                var dest = new TDest();
                mapAction(source, dest);

                return dest;
            };
        }

        private Func<TSource, TDest> GetMappingDelegate<TSource, TDest>() 
            where TDest : new()
        {
            Action<TSource, TDest> mapAction = null;

            return source =>
            {
                var dest = new TDest();
                if (mapAction == null)
                {
                    mapAction = GetMapper(source, dest);
                }
                mapAction(source, dest);

                return dest;
            };
        }

        public Func<TSource, TDest> GetMapper<TSource, TDest>()
            where TDest: new()
        {
            Action<TSource, TDest> mapAction;
            if (TryGetMapperFromCache(out mapAction))
            {
                return GetMappingDelegate(mapAction);
            }
            else
            {
                return GetMappingDelegate<TSource, TDest>();
            }
        }

        public bool TryGetMapperFromCache<TSource, TDest>(out Func<TSource, TDest> mapFunc)
            where TDest : new()
        {
            Action<TSource, TDest> mapAction;
            if (TryGetMapperFromCache(out mapAction))
            {
                mapFunc = GetMappingDelegate(mapAction);
                return true;
            }
            else
            {
                mapFunc = null;
                return false;
            }
        }

        public Action<TSource, TDest> GetMapper<TSource, TDest>(TSource source, TDest dest)
        {
            PairId pairId = PairId.GetId<TSource, TDest>();

            Delegate mappingDelegate;

            if (_cache.TryGetValue(pairId, out mappingDelegate))
            {
                return (Action<TSource, TDest>) mappingDelegate;
            }

            lock (_sync)
            {
                IPropertiesMapper<TSource, TDest> mapper;
                Action<TSource, TDest> mappingAction;

                if (_cache.TryGetValue(pairId, out mappingDelegate))
                {
                    mappingAction = (Action<TSource, TDest>) mappingDelegate;
                }
                else
                {
                    if ((mapper = dest as IPropertiesMapper<TSource, TDest>) != null)
                    {
                        mappingAction = mapper.MapProperties;
                        _cache.Add(pairId, mappingAction);
                    }
                    else
                    {
                        if ((mapper = source as IPropertiesMapper<TSource, TDest>) != null)
                        {
                            mappingAction = mapper.MapProperties;
                            _cache.Add(pairId, mappingAction);
                        }
                        else
                        {
                            throw new NotSupportedException(
                                String.Format("Convert expression for {0}->{1} does not exist.",
                                    typeof (TSource).FullName, typeof (TDest).FullName));
                        }
                    }
                }

                return mappingAction;
            }
        }

        public bool TryGetMapperFromCache<TSource, TDest>(out Action<TSource, TDest> mapper)
        {
            PairId pairId = PairId.GetId<TSource, TDest>();
            Delegate mapperObject;

            if (_cache.TryGetValue(pairId, out mapperObject))
            {
                mapper = (Action<TSource, TDest>) mapperObject;
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
