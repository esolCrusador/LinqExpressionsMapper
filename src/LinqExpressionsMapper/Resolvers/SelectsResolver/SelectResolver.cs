using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using GloryS.Common.Resolvers.SelectsResolver;
using LinqExpressionsMapper.Models;

namespace LinqExpressionsMapper.Resolvers.SelectsResolver
{
    internal class SelectResolver : ISelectResolver
    {
        #region Private Fields

        private readonly Dictionary<PairId, LambdaExpression> _cache;
        private readonly Dictionary<PairId, Func<LambdaExpression>> _lazyCache;

        private readonly Dictionary<PairId, Func<Culture, LambdaExpression>> _cultureSpecificLazyCache;
        private readonly Dictionary<PairId<PairId, Culture>, LambdaExpression> _cultureSpecificCache;

        #endregion

        #region Constructors

        private SelectResolver()
        {
            _cache = new Dictionary<PairId, LambdaExpression>();
            _lazyCache = new Dictionary<PairId, Func<LambdaExpression>>();

            _cultureSpecificLazyCache = new Dictionary<PairId, Func<Culture, LambdaExpression>>();
            _cultureSpecificCache = new Dictionary<PairId<PairId, Culture>, LambdaExpression>();
        }

        #endregion

        #region Private methods

        private void AddToCache<TSource, TDist>(PairId id, ISelectExpression<TSource, TDist> resolver)
        {
            Expression<Func<TSource, TDist>> expression = resolver.GetSelectExpression();

            _cache.Add(id, expression);
        }

        private void AddToCache<TSource, TDist>(PairId id, ISelectExpressionNonCache<TSource, TDist> resolver)
        {
            Func<Expression<Func<TSource, TDist>>> expressionGetter = resolver.GetSelectExpression;

            _lazyCache.Add(id, expressionGetter);
        }

        #endregion

        #region Implementation Of ISelectResolver

        public void Register<TSource, TDest>(ISelectExpression<TSource, TDest> resolver)
        {
            AddToCache(PairId.GetId<TSource, TDest>(), resolver);
        }

        public void Register<TSource, TDest>(ISelectExpressionNonCache<TSource, TDest> resolver)
        {
            AddToCache(PairId.GetId<TSource, TDest>(), resolver);
        }

        public void Register<TSource, TDest>(ICultureSelectExpression<TSource, TDest> resolver)
        {
            _cultureSpecificLazyCache.Add(PairId.GetId<TSource, TDest>(), resolver.GetSelectExpression);
        }

        public Expression<Func<TSource, TDest>> GetExternalExpression<TSource, TDest>()
        {
            return GetExpression(() => ((ISelectExpression<TSource, TDest>) Activator.CreateInstance<TDest>()));
        }
        
        public Expression<Func<TSource, TDest>> GetExpression<TSource, TDest>()
            where TDest : ISelectExpression<TSource, TDest>, new()
        {
            return GetExpression(() => new TDest());
        }

        private Expression<Func<TSource, TDest>> GetExpression<TSource, TDest>(Func<ISelectExpression<TSource, TDest>> activator)
        {
            PairId pairId = PairId.GetId<TSource, TDest>();

            LambdaExpression expression;

            if (_cache.TryGetValue(pairId, out expression))
            {
                return (Expression<Func<TSource, TDest>>)expression;
            }

            Func<LambdaExpression> getExpression;

            if (_lazyCache.TryGetValue(pairId, out getExpression))
            {
                return (Expression<Func<TSource, TDest>>)getExpression();
            }

            ISelectExpression<TSource, TDest> selectExpressionContainer;
            try
            {
                selectExpressionContainer = activator();
            }
            catch (Exception ex)
            {
                throw new NotSupportedException(String.Format("Convert expression for {0} -> {1} does not exist.", typeof(TSource).FullName, typeof(TDest).FullName), ex);
            }

            if (selectExpressionContainer is ISelectExpressionNonCache<TSource, TDest>)
            {
                getExpression = selectExpressionContainer.GetSelectExpression;

                _lazyCache.Add(pairId, getExpression);

                return (Expression<Func<TSource, TDest>>)getExpression();
            }

            expression = selectExpressionContainer.GetSelectExpression();

            _cache.Add(pairId, expression);

            return (Expression<Func<TSource, TDest>>)expression;
        }

        public Expression<Func<TSource, TDest>> GetExpression<TSource, TDest>(Culture culture) where TDest : ICultureSelectExpression<TSource, TDest>, new()
        {
            return GetExpression(culture, () => new TDest());
        }

        public Expression<Func<TSource, TDest>> GetExternalExpression<TSource, TDest>(Culture culture)
        {
            return GetExpression(culture, () => ((ICultureSelectExpression<TSource, TDest>) Activator.CreateInstance<TDest>()));
        }

        private Expression<Func<TSource, TDest>> GetExpression<TSource, TDest>(Culture cultureId, Func<ICultureSelectExpression<TSource, TDest>> activator)
        {
            PairId pairId = PairId.GetId<TSource, TDest>();
            PairId<PairId, Culture> cultureSpecificPairId = new PairId<PairId, Culture>(pairId, cultureId);

            LambdaExpression expression;

            if (_cultureSpecificCache.TryGetValue(cultureSpecificPairId, out expression))
            {
                return (Expression<Func<TSource, TDest>>)expression;
            }

            Func<Culture, LambdaExpression> getExpression;

            if (_cultureSpecificLazyCache.TryGetValue(pairId, out getExpression))
            {
                expression = getExpression(cultureId);

                _cultureSpecificCache.Add(cultureSpecificPairId, expression);

                return (Expression<Func<TSource, TDest>>)expression;
            }

            ICultureSelectExpression<TSource, TDest> selectExpressionContainer;
            try
            {
                selectExpressionContainer = activator();
            }
            catch (Exception ex)
            {
                throw new NotSupportedException(String.Format("Convert expression for {0} -> {1} does not exist.", typeof(TSource).FullName, typeof(TDest).FullName), ex);
            }

            _cultureSpecificLazyCache.Add(pairId, selectExpressionContainer.GetSelectExpression);

            expression = selectExpressionContainer.GetSelectExpression(cultureId);

            _cultureSpecificCache.Add(cultureSpecificPairId, expression);

            return (Expression<Func<TSource, TDest>>)expression;
        }

        #endregion

        #region Singleton

        private static readonly Object StaticSyncRoot = new Object();
        private static SelectResolver _instance;

        public static SelectResolver Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (StaticSyncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new SelectResolver();
                        }
                    }
                }

                return _instance;
            }
        }

        #endregion
    }
}
