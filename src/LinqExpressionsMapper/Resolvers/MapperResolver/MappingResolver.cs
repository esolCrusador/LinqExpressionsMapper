﻿using System;
using System.Collections.Generic;
using LinqExpressionsMapper.Models;

namespace LinqExpressionsMapper.Resolvers.MapperResolver
{
    public class MappingResolver : IMappingResolver
    {
        #region Private Fields

        private readonly object _sync = new object();
        private readonly Dictionary<PairId, object> _cache = new Dictionary<PairId, object>();

        #endregion

        #region Implemetation Of IMappingResolver

        public void Register<TSource, TDest>(IPropertiesMapper<TSource, TDest> mapper)
        {
            _cache.Add(PairId.GetId<TSource, TDest>(), mapper);
        }

        public IPropertiesMapper<TSource, TDest> GetMapper<TSource, TDest>(TSource source, TDest dest)
        {
            PairId pairId = PairId.GetId<TSource, TDest>();

            IPropertiesMapper<TSource, TDest> mapper;
            object mapperObject;

            if (_cache.TryGetValue(pairId, out mapperObject))
            {
                mapper = (IPropertiesMapper<TSource, TDest>) mapperObject;
            }
            else
            {
                lock (_sync)
                {
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
                }
            }

            return mapper;
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

        #region Singleton

        private static readonly Object StaticSyncRoot = new Object();
        private static MappingResolver _instance;

        public static MappingResolver Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (StaticSyncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new MappingResolver();
                        }
                    }
                }

                return _instance;
            }
        }

        #endregion
    }
}