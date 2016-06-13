using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqExpressionsMapper.Models;

namespace System
{
    public interface IPropertiesMapper<in TSource, in TDest>
    {
        void MapProperties(TSource source, TDest dest);
    }
}

namespace LinqExpressionsMapper.Resolvers.MappingResolver
{
    internal class MappingResolverWith0Params
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
                    mapAction = GetMapper<TSource, TDest>(source, dest);
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
                return GetMappingDelegate<TSource, TDest>(mapAction);
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

        #region Mapping Methods

        internal TDest Map<TSource, TDest>(TSource source)
            where TDest : class, new()
            where TSource : class
        {
            TDest dest = new TDest();

            Action<TSource, TDest> mappingAction = GetMapper<TSource, TDest>(source, dest);

            mappingAction(source, dest);

            return dest;
        }

        internal TDest Map<TSource, TDest>(TSource source, TDest dest)
            where TDest : class
            where TSource : class
        {
            Action<TSource, TDest> mappingAction = GetMapper<TSource, TDest>(source, dest);

            mappingAction(source, dest);

            return dest;
        }

        internal TDest Map<TMapper, TSource, TDest>(TSource source, TDest dest)
            where TMapper : IPropertiesMapper<TSource, TDest>, new()
            where TDest : class
            where TSource : class
        {
            Action<TSource, TDest> mappingAction;
            if (!TryGetMapperFromCache<TSource, TDest>(out mappingAction))
            {
                var mapper = new TMapper();
                mappingAction = mapper.MapProperties;
                Mapper.Register(mapper);
            }

            mappingAction(source, dest);

            return dest;
        }

        internal TDest Map<TMapper, TSource, TDest>(TSource source)
            where TMapper : IPropertiesMapper<TSource, TDest>, new()
            where TDest : class, new()
            where TSource : class
        {
            var dest = new TDest();

            return Map<TMapper, TSource, TDest>(source, dest);
        }

        internal Func<TSource, TDest> GetMapper<TMapper, TSource, TDest>()
            where TMapper : IPropertiesMapper<TSource, TDest>, new()
            where TDest : new()
        {
            Func<TSource, TDest> mappingFunc;
            if (!TryGetMapperFromCache<TSource, TDest>(out mappingFunc))
            {
                Mapper.Register(new TMapper());
                mappingFunc = GetMapper<TSource, TDest>();
            }

            return mappingFunc;
        }

        #endregion
    }
}

namespace System
{
    public interface IPropertiesMapper<in TSource, in TDest, in TParam1>
    {
        void MapProperties(TSource source, TDest dest, TParam1 param1);
    }
}

namespace LinqExpressionsMapper.Resolvers.MappingResolver
{
    internal class MappingResolverWith1Params
    {
        #region Private Fields

        private readonly object _sync = new object();
        private readonly Dictionary<PairId, Delegate> _cache = new Dictionary<PairId, Delegate>();

        #endregion

        #region Implemetation Of IMappingResolver

        public void Register<TSource, TDest, TParam1>(IPropertiesMapper<TSource, TDest, TParam1> mapper)
        {
            lock (_sync)
            {
                PairId id = PairId.GetId<TSource, TDest>();
                if (_cache.ContainsKey(id))
                {
                    _cache[id] = (Action<TSource, TDest, TParam1>) mapper.MapProperties;
                }
                else
                {
                    _cache.Add(PairId.GetId<TSource, TDest>(), (Action<TSource, TDest, TParam1>) mapper.MapProperties);
                }
            }
        }

        public void RegisterAll(object mapper, Type interfaces, Type[] implementedInterfaces)
        {
            lock (_sync)
            {
                Type propertiesMapperType = typeof (IPropertiesMapper<,,>);
                foreach (Type interfaceType in implementedInterfaces.Where(i=>i.GUID==propertiesMapperType.GUID))
                {
                    Type[] genericArgs = interfaceType.GetGenericArguments();

                    var pairId = PairId.GetId(genericArgs[0], genericArgs[1]);

                    var methodInfo = interfaces.GetInterfaceMap(interfaceType).InterfaceMethods.Single();
                    Type delegateType = typeof (Action<,,>).MakeGenericType(genericArgs[0], genericArgs[1], genericArgs[2]);
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

        private static Func<TSource, TDest> GetMappingDelegate<TSource, TDest, TParam1>(Action<TSource, TDest, TParam1> mapAction, TParam1 param1) 
            where TDest : new()
        {
            return source =>
            {
                var dest = new TDest();
                mapAction(source, dest, param1);

                return dest;
            };
        }

        private Func<TSource, TDest> GetMappingDelegate<TSource, TDest, TParam1>(TParam1 param1) 
            where TDest : new()
        {
            Action<TSource, TDest, TParam1> mapAction = null;

            return source =>
            {
                var dest = new TDest();
                if (mapAction == null)
                {
                    mapAction = GetMapper<TSource, TDest, TParam1>(source, dest);
                }
                mapAction(source, dest, param1);

                return dest;
            };
        }

        public Func<TSource, TDest> GetMapper<TSource, TDest, TParam1>(TParam1 param1)
            where TDest: new()
        {
            Action<TSource, TDest, TParam1> mapAction;
            if (TryGetMapperFromCache(out mapAction))
            {
                return GetMappingDelegate<TSource, TDest, TParam1>(mapAction, param1);
            }
            else
            {
                return GetMappingDelegate<TSource, TDest, TParam1>(param1);
            }
        }

        public bool TryGetMapperFromCache<TSource, TDest, TParam1>(out Func<TSource, TDest> mapFunc, TParam1 param1)
            where TDest : new()
        {
            Action<TSource, TDest, TParam1> mapAction;
            if (TryGetMapperFromCache(out mapAction))
            {
                mapFunc = GetMappingDelegate(mapAction, param1);
                return true;
            }
            else
            {
                mapFunc = null;
                return false;
            }
        }

        public Action<TSource, TDest, TParam1> GetMapper<TSource, TDest, TParam1>(TSource source, TDest dest)
        {
            PairId pairId = PairId.GetId<TSource, TDest>();

            Delegate mappingDelegate;

            if (_cache.TryGetValue(pairId, out mappingDelegate))
            {
                return (Action<TSource, TDest, TParam1>) mappingDelegate;
            }

            lock (_sync)
            {
                IPropertiesMapper<TSource, TDest, TParam1> mapper;
                Action<TSource, TDest, TParam1> mappingAction;

                if (_cache.TryGetValue(pairId, out mappingDelegate))
                {
                    mappingAction = (Action<TSource, TDest, TParam1>) mappingDelegate;
                }
                else
                {
                    if ((mapper = dest as IPropertiesMapper<TSource, TDest, TParam1>) != null)
                    {
                        mappingAction = mapper.MapProperties;
                        _cache.Add(pairId, mappingAction);
                    }
                    else
                    {
                        if ((mapper = source as IPropertiesMapper<TSource, TDest, TParam1>) != null)
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

        public bool TryGetMapperFromCache<TSource, TDest, TParam1>(out Action<TSource, TDest, TParam1> mapper)
        {
            PairId pairId = PairId.GetId<TSource, TDest>();
            Delegate mapperObject;

            if (_cache.TryGetValue(pairId, out mapperObject))
            {
                mapper = (Action<TSource, TDest, TParam1>) mapperObject;
                return true;
            }
            else
            {
                mapper = null;
                return false;
            }
        }

        #endregion

        #region Mapping Methods

        internal TDest Map<TSource, TDest, TParam1>(TSource source, TParam1 param1)
            where TDest : class, new()
            where TSource : class
        {
            TDest dest = new TDest();

            Action<TSource, TDest, TParam1> mappingAction = GetMapper<TSource, TDest, TParam1>(source, dest);

            mappingAction(source, dest, param1);

            return dest;
        }

        internal TDest Map<TSource, TDest, TParam1>(TSource source, TDest dest, TParam1 param1)
            where TDest : class
            where TSource : class
        {
            Action<TSource, TDest, TParam1> mappingAction = GetMapper<TSource, TDest, TParam1>(source, dest);

            mappingAction(source, dest, param1);

            return dest;
        }

        internal TDest Map<TMapper, TSource, TDest, TParam1>(TSource source, TDest dest, TParam1 param1)
            where TMapper : IPropertiesMapper<TSource, TDest, TParam1>, new()
            where TDest : class
            where TSource : class
        {
            Action<TSource, TDest, TParam1> mappingAction;
            if (!TryGetMapperFromCache<TSource, TDest, TParam1>(out mappingAction))
            {
                var mapper = new TMapper();
                mappingAction = mapper.MapProperties;
                Mapper.Register(mapper);
            }

            mappingAction(source, dest, param1);

            return dest;
        }

        internal TDest Map<TMapper, TSource, TDest, TParam1>(TSource source, TParam1 param1)
            where TMapper : IPropertiesMapper<TSource, TDest, TParam1>, new()
            where TDest : class, new()
            where TSource : class
        {
            var dest = new TDest();

            return Map<TMapper, TSource, TDest, TParam1>(source, dest, param1);
        }

        internal Func<TSource, TDest> GetMapper<TMapper, TSource, TDest, TParam1>(TParam1 param1)
            where TMapper : IPropertiesMapper<TSource, TDest, TParam1>, new()
            where TDest : new()
        {
            Func<TSource, TDest> mappingFunc;
            if (!TryGetMapperFromCache<TSource, TDest, TParam1>(out mappingFunc, param1))
            {
                Mapper.Register(new TMapper());
                mappingFunc = GetMapper<TSource, TDest, TParam1>(param1);
            }

            return mappingFunc;
        }

        #endregion
    }
}

namespace System
{
    public interface IPropertiesMapper<in TSource, in TDest, in TParam1, in TParam2>
    {
        void MapProperties(TSource source, TDest dest, TParam1 param1, TParam2 param2);
    }
}

namespace LinqExpressionsMapper.Resolvers.MappingResolver
{
    internal class MappingResolverWith2Params
    {
        #region Private Fields

        private readonly object _sync = new object();
        private readonly Dictionary<PairId, Delegate> _cache = new Dictionary<PairId, Delegate>();

        #endregion

        #region Implemetation Of IMappingResolver

        public void Register<TSource, TDest, TParam1, TParam2>(IPropertiesMapper<TSource, TDest, TParam1, TParam2> mapper)
        {
            lock (_sync)
            {
                PairId id = PairId.GetId<TSource, TDest>();
                if (_cache.ContainsKey(id))
                {
                    _cache[id] = (Action<TSource, TDest, TParam1, TParam2>) mapper.MapProperties;
                }
                else
                {
                    _cache.Add(PairId.GetId<TSource, TDest>(), (Action<TSource, TDest, TParam1, TParam2>) mapper.MapProperties);
                }
            }
        }

        public void RegisterAll(object mapper, Type interfaces, Type[] implementedInterfaces)
        {
            lock (_sync)
            {
                Type propertiesMapperType = typeof (IPropertiesMapper<,,,>);
                foreach (Type interfaceType in implementedInterfaces.Where(i=>i.GUID==propertiesMapperType.GUID))
                {
                    Type[] genericArgs = interfaceType.GetGenericArguments();

                    var pairId = PairId.GetId(genericArgs[0], genericArgs[1]);

                    var methodInfo = interfaces.GetInterfaceMap(interfaceType).InterfaceMethods.Single();
                    Type delegateType = typeof (Action<,,,>).MakeGenericType(genericArgs[0], genericArgs[1], genericArgs[2], genericArgs[3]);
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

        private static Func<TSource, TDest> GetMappingDelegate<TSource, TDest, TParam1, TParam2>(Action<TSource, TDest, TParam1, TParam2> mapAction, TParam1 param1, TParam2 param2) 
            where TDest : new()
        {
            return source =>
            {
                var dest = new TDest();
                mapAction(source, dest, param1, param2);

                return dest;
            };
        }

        private Func<TSource, TDest> GetMappingDelegate<TSource, TDest, TParam1, TParam2>(TParam1 param1, TParam2 param2) 
            where TDest : new()
        {
            Action<TSource, TDest, TParam1, TParam2> mapAction = null;

            return source =>
            {
                var dest = new TDest();
                if (mapAction == null)
                {
                    mapAction = GetMapper<TSource, TDest, TParam1, TParam2>(source, dest);
                }
                mapAction(source, dest, param1, param2);

                return dest;
            };
        }

        public Func<TSource, TDest> GetMapper<TSource, TDest, TParam1, TParam2>(TParam1 param1, TParam2 param2)
            where TDest: new()
        {
            Action<TSource, TDest, TParam1, TParam2> mapAction;
            if (TryGetMapperFromCache(out mapAction))
            {
                return GetMappingDelegate<TSource, TDest, TParam1, TParam2>(mapAction, param1, param2);
            }
            else
            {
                return GetMappingDelegate<TSource, TDest, TParam1, TParam2>(param1, param2);
            }
        }

        public bool TryGetMapperFromCache<TSource, TDest, TParam1, TParam2>(out Func<TSource, TDest> mapFunc, TParam1 param1, TParam2 param2)
            where TDest : new()
        {
            Action<TSource, TDest, TParam1, TParam2> mapAction;
            if (TryGetMapperFromCache(out mapAction))
            {
                mapFunc = GetMappingDelegate(mapAction, param1, param2);
                return true;
            }
            else
            {
                mapFunc = null;
                return false;
            }
        }

        public Action<TSource, TDest, TParam1, TParam2> GetMapper<TSource, TDest, TParam1, TParam2>(TSource source, TDest dest)
        {
            PairId pairId = PairId.GetId<TSource, TDest>();

            Delegate mappingDelegate;

            if (_cache.TryGetValue(pairId, out mappingDelegate))
            {
                return (Action<TSource, TDest, TParam1, TParam2>) mappingDelegate;
            }

            lock (_sync)
            {
                IPropertiesMapper<TSource, TDest, TParam1, TParam2> mapper;
                Action<TSource, TDest, TParam1, TParam2> mappingAction;

                if (_cache.TryGetValue(pairId, out mappingDelegate))
                {
                    mappingAction = (Action<TSource, TDest, TParam1, TParam2>) mappingDelegate;
                }
                else
                {
                    if ((mapper = dest as IPropertiesMapper<TSource, TDest, TParam1, TParam2>) != null)
                    {
                        mappingAction = mapper.MapProperties;
                        _cache.Add(pairId, mappingAction);
                    }
                    else
                    {
                        if ((mapper = source as IPropertiesMapper<TSource, TDest, TParam1, TParam2>) != null)
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

        public bool TryGetMapperFromCache<TSource, TDest, TParam1, TParam2>(out Action<TSource, TDest, TParam1, TParam2> mapper)
        {
            PairId pairId = PairId.GetId<TSource, TDest>();
            Delegate mapperObject;

            if (_cache.TryGetValue(pairId, out mapperObject))
            {
                mapper = (Action<TSource, TDest, TParam1, TParam2>) mapperObject;
                return true;
            }
            else
            {
                mapper = null;
                return false;
            }
        }

        #endregion

        #region Mapping Methods

        internal TDest Map<TSource, TDest, TParam1, TParam2>(TSource source, TParam1 param1, TParam2 param2)
            where TDest : class, new()
            where TSource : class
        {
            TDest dest = new TDest();

            Action<TSource, TDest, TParam1, TParam2> mappingAction = GetMapper<TSource, TDest, TParam1, TParam2>(source, dest);

            mappingAction(source, dest, param1, param2);

            return dest;
        }

        internal TDest Map<TSource, TDest, TParam1, TParam2>(TSource source, TDest dest, TParam1 param1, TParam2 param2)
            where TDest : class
            where TSource : class
        {
            Action<TSource, TDest, TParam1, TParam2> mappingAction = GetMapper<TSource, TDest, TParam1, TParam2>(source, dest);

            mappingAction(source, dest, param1, param2);

            return dest;
        }

        internal TDest Map<TMapper, TSource, TDest, TParam1, TParam2>(TSource source, TDest dest, TParam1 param1, TParam2 param2)
            where TMapper : IPropertiesMapper<TSource, TDest, TParam1, TParam2>, new()
            where TDest : class
            where TSource : class
        {
            Action<TSource, TDest, TParam1, TParam2> mappingAction;
            if (!TryGetMapperFromCache<TSource, TDest, TParam1, TParam2>(out mappingAction))
            {
                var mapper = new TMapper();
                mappingAction = mapper.MapProperties;
                Mapper.Register(mapper);
            }

            mappingAction(source, dest, param1, param2);

            return dest;
        }

        internal TDest Map<TMapper, TSource, TDest, TParam1, TParam2>(TSource source, TParam1 param1, TParam2 param2)
            where TMapper : IPropertiesMapper<TSource, TDest, TParam1, TParam2>, new()
            where TDest : class, new()
            where TSource : class
        {
            var dest = new TDest();

            return Map<TMapper, TSource, TDest, TParam1, TParam2>(source, dest, param1, param2);
        }

        internal Func<TSource, TDest> GetMapper<TMapper, TSource, TDest, TParam1, TParam2>(TParam1 param1, TParam2 param2)
            where TMapper : IPropertiesMapper<TSource, TDest, TParam1, TParam2>, new()
            where TDest : new()
        {
            Func<TSource, TDest> mappingFunc;
            if (!TryGetMapperFromCache<TSource, TDest, TParam1, TParam2>(out mappingFunc, param1, param2))
            {
                Mapper.Register(new TMapper());
                mappingFunc = GetMapper<TSource, TDest, TParam1, TParam2>(param1, param2);
            }

            return mappingFunc;
        }

        #endregion
    }
}
