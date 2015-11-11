using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using LinqExpressionsMapper.Models;

namespace LinqExpressionsMapper.Resolvers.MapperResolver
{
    internal class MappingResolver
    {
        #region Private Fields

        private readonly object _sync = new object();
        private readonly Dictionary<PairId, Delegate> _cache = new Dictionary<PairId, Delegate>();
        private readonly List<KeyValuePair<Type, string>> _mappingInterfaces = new List<KeyValuePair<Type, string>>();

        #endregion

        #region Implemetation Of IMappingResolver

        public void Register<TSource, TDest>(IPropertiesMapper<TSource, TDest> mapper)
        {
            Register((Action<TSource, TDest>) mapper.MapProperties);
        }

        public Action<TSource, TDest> Register<TSource, TDest>(object mapper)
        {
            Action<TSource, TDest> mapAction;
            if (!TryRegisterPropertiesMapper(mapper, out mapAction) && !TryRegister(mapAction, out mapAction))
            {
                throw new ArgumentException(String.Format("Type {0} does not contain mapper {1}->{2}.", mapper.GetType().FullName, typeof (TSource).FullName, typeof (TDest).FullName));
            }

            return mapAction;
        }

        public bool TryRegisterPropertiesMapper<TSource, TDest>(object mapper, out Action<TSource, TDest> mapAction)
        {
            IPropertiesMapper<TSource, TDest> propertiesMapper;
            if ((propertiesMapper = (mapper as IPropertiesMapper<TSource, TDest>)) != null)
            {
                mapAction = propertiesMapper.MapProperties;
                Register(mapAction);
                return true;
            }
            else
            {
                mapAction = null;
                return false;
            }
        }

        public bool TryRegister<TSource, TDest>(object mapper, out Action<TSource, TDest> mapAction)
        {
            Type mapperType = mapper.GetType();

            foreach (KeyValuePair<Type, string> kvp in _mappingInterfaces)
            {
                if (mapperType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == kvp.Key))
                {
                    var mapInterface = mapperType.GetInterface(kvp.Key.FullName);
                    MethodInfo method = mapInterface.GetMethod(kvp.Value);

                    mapAction = (Action<TSource, TDest>) Delegate.CreateDelegate(typeof(Action<TSource, TDest>), mapper, method, true);
                    Register(mapAction);

                    return true;
                }
            }

            mapAction = null;
            return false;
        }

        public void Register<TSource, TDest>(Action<TSource, TDest> mapAction)
        {
            lock (_sync)
            {
                PairId id = PairId.GetId<TSource, TDest>();
                if (_cache.ContainsKey(id))
                {
                    _cache[id] = mapAction;
                }
                else
                {
                    _cache.Add(PairId.GetId<TSource, TDest>(), mapAction);
                }
            }
        }

        public Action<TSource, TDest> GetMapper<TSource, TDest>(TSource source, TDest dest)
        {
            PairId pairId = PairId.GetId<TSource, TDest>();

            Delegate mapDelegate;

            if (_cache.TryGetValue(pairId, out mapDelegate))
            {
                return (Action<TSource, TDest>) mapDelegate;
            }

            lock (_sync)
            {
                Action<TSource, TDest> mapper;

                if (_cache.TryGetValue(pairId, out mapDelegate))
                {
                    mapper = (Action<TSource, TDest>) mapDelegate;
                }
                else
                {
                    if (!TryRegisterPropertiesMapper(dest, out mapper) && !TryRegisterPropertiesMapper(source, out mapper) && !TryRegister(dest, out mapper) && !TryRegister(source, out mapper))
                    {
                        throw new NotSupportedException(String.Format("Convert expression for {0}->{1} does not exist.",
                            typeof (TSource).FullName, typeof (TDest).FullName));
                    }
                }

                return mapper;
            }
        }

        public bool TryGetMapper<TSource, TDest>(out Action<TSource, TDest> mapper)
        {
            PairId pairId = PairId.GetId<TSource, TDest>();
            Delegate mapDelegate;

            if (_cache.TryGetValue(pairId, out mapDelegate))
            {
                mapper = (Action<TSource, TDest>)mapDelegate;
                return true;
            }
            else
            {
                mapper = null;
                return false;
            }
        }

        #endregion

        public void RegisterInterface(Type type, string methodName)
        {
            _mappingInterfaces.Add(new KeyValuePair<Type, string>(type, methodName));
        }
    }
}
