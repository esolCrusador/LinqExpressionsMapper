using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqExpressionsMapper.Models;

namespace System.Linq.Expressions
{
    public interface ISelectExpression<TSource, TDest>
    {
        Expression<Func<TSource, TDest>> GetSelectExpression();
    }

    public interface ISelectDynamicExpression<TSource, TDest>: ISelectExpression<TSource, TDest>
    {
    }
}

namespace LinqExpressionsMapper.Resolvers.SelectsResolver
{
    public class SelectResolverWith0Params
    {
        private readonly object _sync = new object();

        private readonly Dictionary<PairId, Delegate> _dynamicExpressionFactories = new Dictionary<PairId, Delegate>();

        private readonly Dictionary<PairId, Delegate> _expressionFactories = new Dictionary<PairId, Delegate>();
		private readonly Dictionary<PairId, LambdaExpression> _expressions = new Dictionary<PairId, LambdaExpression>();
        public void Register<TSource, TDest>(ISelectExpression<TSource, TDest> resolver)
        {
            PairId id = PairId.GetId<TSource, TDest>();

            var factory = (Func<Expression<Func<TSource, TDest>>>)resolver.GetSelectExpression;

            lock (_sync)
            {
                if (_expressionFactories.ContainsKey(id))
                {
                    _expressionFactories[id] = factory;
					TryRemoveExpressions(id);
                }
                else
                {
                    _expressionFactories.Add(id, factory);
                }
            }
        }

        public void Register<TSource, TDest>(ISelectDynamicExpression<TSource, TDest> resolver)
        {
            PairId id = PairId.GetId<TSource, TDest>();

            var factory = (Func<Expression<Func<TSource, TDest>>>)resolver.GetSelectExpression;

            lock (_sync)
            {
                if (_dynamicExpressionFactories.ContainsKey(id))
                {
                    _dynamicExpressionFactories[id] = factory;
                }
                else
                {
                    _dynamicExpressionFactories.Add(id, factory);
                }
            }
        }

        public void RegisterAll(object mapper, Type mapperType, Type[] implementedInterfaces)
        {
            Type selectExpressionType = typeof (ISelectExpression<,>);
            Type dynamicSelectExpressionType = typeof (ISelectDynamicExpression<,>);

            var selectExpressions = implementedInterfaces.Where(i => i.GUID == selectExpressionType.GUID || i.GUID == dynamicSelectExpressionType.GUID)
                .Select(i =>
                {
                    var genericArgs = i.GetGenericArguments();
                    return new
                    {
                        InterfaceType = i,
						GenericArguments = genericArgs,
                        PairId = PairId.GetId(genericArgs[0], genericArgs[1])
                    };
                })
                .GroupBy(i => i.PairId)
                .Select(
                    g => g.FirstOrDefault(i => i.InterfaceType.GUID == dynamicSelectExpressionType.GUID)
                         ?? g.First(i => i.InterfaceType.GUID == selectExpressionType.GUID)
                )
                .Select(interfaceDescription =>
                {
                    var methodInfo = mapperType.GetInterfaceMap(interfaceDescription.InterfaceType).InterfaceMethods.SingleOrDefault()
                                     ?? interfaceDescription.InterfaceType.GetInterfaces().SelectMany(i => mapperType.GetInterfaceMap(i).TargetMethods).Single();

					Type delegateType = typeof (Func<>).MakeGenericType(
                    typeof (Expression<>).MakeGenericType(
							typeof (Func<,>).MakeGenericType(interfaceDescription.PairId.SourceId, interfaceDescription.PairId.DestId)
							)
						);

                    var factoryDelegate = Delegate.CreateDelegate(delegateType, mapper, methodInfo, true);

                    return new
                    {
                        InterfaceType = interfaceDescription.InterfaceType,
                        PairId = interfaceDescription.PairId,
                        FactoryDelegate = factoryDelegate
                    };
                })
				.ToList();

			lock(_sync)
			{
				foreach (var selectExpression in selectExpressions)
				{
					Dictionary<PairId, Delegate> cacheDictionary;
					if (selectExpression.InterfaceType.GUID == dynamicSelectExpressionType.GUID)
						cacheDictionary = _dynamicExpressionFactories;
					else if (selectExpression.InterfaceType.GUID == selectExpressionType.GUID)
						cacheDictionary = _expressionFactories;
					else
						throw new NotSupportedException(String.Format("The interface {0} caching is not supported.", selectExpression.InterfaceType.FullName));

					if (cacheDictionary.ContainsKey(selectExpression.PairId))
						cacheDictionary[selectExpression.PairId] = selectExpression.FactoryDelegate;
					else
						cacheDictionary.Add(selectExpression.PairId, selectExpression.FactoryDelegate);
				}
			}
        }

        public bool TryGetFromCache<TSource, TDest>(out Expression<Func<TSource, TDest>> expression)
        {
            return TryGetOrActivate(null, out expression);
        }

        public Expression<Func<TSource, TDest>> GetExpression<TSource, TDest>()
            where TDest: ISelectExpression<TSource, TDest>, new()
        {
            try
            {
                Expression<Func<TSource, TDest>> expression;
                if (TryGetOrActivate(() => new TDest(), out expression))
                {
                    return expression;
                }
                else
                {
                    throw new NotImplementedException("Something went wrong.");
                }
            }
            catch (Exception ex)
            {
                throw new NotSupportedException(String.Format("Convert expression for {0} -> {1} does not exist.", typeof(TSource).FullName, typeof(TDest).FullName), ex);
            }
        }

        public Expression<Func<TSource, TDest>> GetExternalExpression<TSource, TDest>()
        {
            try
            {
                Expression<Func<TSource, TDest>> expression;
                if (TryGetOrActivate(() => (ISelectExpression<TSource, TDest>) Activator.CreateInstance<TDest>(), out expression))
                {
                    return expression;
                }
                else
                {
                    throw new NotImplementedException("Something went wrong.");
                }
            }
            catch (Exception ex)
            {
                throw new NotSupportedException(String.Format("Convert expression for {0} -> {1} does not exist.", typeof(TSource).FullName, typeof(TDest).FullName), ex);
            }
        }

        private bool TryGetOrActivate<TSource, TDest>(Func<ISelectExpression<TSource, TDest>> activator, out Expression<Func<TSource, TDest>> expression)
        {
            PairId id = PairId.GetId<TSource, TDest>();

            Dictionary<PairId, LambdaExpression> expressions = GetOrAddParamExpressionsDictionary(add: false);

            LambdaExpression lambdaExpression;

            if (expressions != null && expressions.TryGetValue(id, out lambdaExpression))
            {
                expression = (Expression<Func<TSource, TDest>>)lambdaExpression;

                return true;
            }

            lock (_sync)
            {
                expressions = expressions ?? GetOrAddParamExpressionsDictionary(add: true);

                if (expressions.TryGetValue(id, out lambdaExpression))
                {
                    expression = (Expression<Func<TSource, TDest>>)lambdaExpression;

                    return true;
                }


                Delegate factoryDelegate;
                Func<Expression<Func<TSource, TDest>>> factory;

                if (_expressionFactories.TryGetValue(id, out factoryDelegate))
                {
                    factory = (Func<Expression<Func<TSource, TDest>>>)factoryDelegate;
                    expression = factory();
                    expressions.Add(id, expression);

                    TryRemoveFactory(id);

                    return true;
                }

                if (_dynamicExpressionFactories.TryGetValue(id, out factoryDelegate))
                {
                    factory = (Func<Expression<Func<TSource, TDest>>>)factoryDelegate;
                    expression = factory();

                    return true;
                }

                if (activator != null)
                {
                    ISelectExpression<TSource, TDest> selectResolver = activator();
                    factory = selectResolver.GetSelectExpression;

                    if (selectResolver is ISelectDynamicExpression<TSource, TDest>)
                    {
                        _dynamicExpressionFactories.Add(id, factory);
                        expression = factory();

                        return true;
                    }
                    else
                    {
                        _expressionFactories.Add(id, factory);
                        expression = factory();
                        expressions.Add(id, expression);

                        TryRemoveFactory(id);

                        return true;
                    }
                }
            }

            expression = null;
            return false;
        }

        private Dictionary<PairId, LambdaExpression> GetOrAddParamExpressionsDictionary(bool add)
        {
            return _expressions;
        }

        private void TryRemoveFactory(PairId id)
        {
            if (_expressionFactories.ContainsKey(id))
            {
                _expressionFactories.Remove(id);
            }
        }

		private void TryRemoveExpressions(PairId id)
		{
		    if (_expressions.ContainsKey(id))
		    {
		        _expressions.Remove(id);
		    }
		}
    }
}

namespace System.Linq.Expressions
{
    public interface ISelectExpression<TSource, TDest, in TParam1>
    {
        Expression<Func<TSource, TDest>> GetSelectExpression(TParam1 param1);
    }

    public interface ISelectDynamicExpression<TSource, TDest, in TParam1>: ISelectExpression<TSource, TDest, TParam1>
    {
    }
}

namespace LinqExpressionsMapper.Resolvers.SelectsResolver
{
    public class SelectResolverWith1Params
    {
        private readonly object _sync = new object();

        private readonly Dictionary<PairId, Delegate> _dynamicExpressionFactories = new Dictionary<PairId, Delegate>();

        private readonly Dictionary<PairId, Delegate> _expressionFactories = new Dictionary<PairId, Delegate>();
		private readonly Dictionary<object, Dictionary<PairId, LambdaExpression>> _expressions = new Dictionary<object, Dictionary<PairId, LambdaExpression>>();
        public void Register<TSource, TDest, TParam1>(ISelectExpression<TSource, TDest, TParam1> resolver)
        {
            PairId id = PairId.GetId<TSource, TDest>();

            var factory = (Func<TParam1, Expression<Func<TSource, TDest>>>)resolver.GetSelectExpression;

            lock (_sync)
            {
                if (_expressionFactories.ContainsKey(id))
                {
                    _expressionFactories[id] = factory;
					TryRemoveExpressions<TParam1>(id);
                }
                else
                {
                    _expressionFactories.Add(id, factory);
                }
            }
        }

        public void Register<TSource, TDest, TParam1>(ISelectDynamicExpression<TSource, TDest, TParam1> resolver)
        {
            PairId id = PairId.GetId<TSource, TDest>();

            var factory = (Func<TParam1, Expression<Func<TSource, TDest>>>)resolver.GetSelectExpression;

            lock (_sync)
            {
                if (_dynamicExpressionFactories.ContainsKey(id))
                {
                    _dynamicExpressionFactories[id] = factory;
                }
                else
                {
                    _dynamicExpressionFactories.Add(id, factory);
                }
            }
        }

        public void RegisterAll(object mapper, Type mapperType, Type[] implementedInterfaces)
        {
            Type selectExpressionType = typeof (ISelectExpression<,,>);
            Type dynamicSelectExpressionType = typeof (ISelectDynamicExpression<,,>);

            var selectExpressions = implementedInterfaces.Where(i => i.GUID == selectExpressionType.GUID || i.GUID == dynamicSelectExpressionType.GUID)
                .Select(i =>
                {
                    var genericArgs = i.GetGenericArguments();
                    return new
                    {
                        InterfaceType = i,
						GenericArguments = genericArgs,
                        PairId = PairId.GetId(genericArgs[0], genericArgs[1])
                    };
                })
                .GroupBy(i => i.PairId)
                .Select(
                    g => g.FirstOrDefault(i => i.InterfaceType.GUID == dynamicSelectExpressionType.GUID)
                         ?? g.First(i => i.InterfaceType.GUID == selectExpressionType.GUID)
                )
                .Select(interfaceDescription =>
                {
                    var methodInfo = mapperType.GetInterfaceMap(interfaceDescription.InterfaceType).InterfaceMethods.SingleOrDefault()
                                     ?? interfaceDescription.InterfaceType.GetInterfaces().SelectMany(i => mapperType.GetInterfaceMap(i).TargetMethods).Single();

					Type delegateType = typeof (Func<,>).MakeGenericType(
						interfaceDescription.GenericArguments[2],
                    typeof (Expression<>).MakeGenericType(
							typeof (Func<,>).MakeGenericType(interfaceDescription.PairId.SourceId, interfaceDescription.PairId.DestId)
							)
						);

                    var factoryDelegate = Delegate.CreateDelegate(delegateType, mapper, methodInfo, true);

                    return new
                    {
                        InterfaceType = interfaceDescription.InterfaceType,
                        PairId = interfaceDescription.PairId,
                        FactoryDelegate = factoryDelegate
                    };
                })
				.ToList();

			lock(_sync)
			{
				foreach (var selectExpression in selectExpressions)
				{
					Dictionary<PairId, Delegate> cacheDictionary;
					if (selectExpression.InterfaceType.GUID == dynamicSelectExpressionType.GUID)
						cacheDictionary = _dynamicExpressionFactories;
					else if (selectExpression.InterfaceType.GUID == selectExpressionType.GUID)
						cacheDictionary = _expressionFactories;
					else
						throw new NotSupportedException(String.Format("The interface {0} caching is not supported.", selectExpression.InterfaceType.FullName));

					if (cacheDictionary.ContainsKey(selectExpression.PairId))
						cacheDictionary[selectExpression.PairId] = selectExpression.FactoryDelegate;
					else
						cacheDictionary.Add(selectExpression.PairId, selectExpression.FactoryDelegate);
				}
			}
        }

        public bool TryGetFromCache<TSource, TDest, TParam1>(TParam1 param1, out Expression<Func<TSource, TDest>> expression)
        {
            return TryGetOrActivate(param1, null, out expression);
        }

        public Expression<Func<TSource, TDest>> GetExpression<TSource, TDest, TParam1>(TParam1 param1)
            where TDest: ISelectExpression<TSource, TDest, TParam1>, new()
        {
            try
            {
                Expression<Func<TSource, TDest>> expression;
                if (TryGetOrActivate(param1, () => new TDest(), out expression))
                {
                    return expression;
                }
                else
                {
                    throw new NotImplementedException("Something went wrong.");
                }
            }
            catch (Exception ex)
            {
                throw new NotSupportedException(String.Format("Convert expression for {0} -> {1} does not exist.", typeof(TSource).FullName, typeof(TDest).FullName), ex);
            }
        }

        public Expression<Func<TSource, TDest>> GetExternalExpression<TSource, TDest, TParam1>(TParam1 param1)
        {
            try
            {
                Expression<Func<TSource, TDest>> expression;
                if (TryGetOrActivate(param1, () => (ISelectExpression<TSource, TDest, TParam1>) Activator.CreateInstance<TDest>(), out expression))
                {
                    return expression;
                }
                else
                {
                    throw new NotImplementedException("Something went wrong.");
                }
            }
            catch (Exception ex)
            {
                throw new NotSupportedException(String.Format("Convert expression for {0} -> {1} does not exist.", typeof(TSource).FullName, typeof(TDest).FullName), ex);
            }
        }

        private bool TryGetOrActivate<TSource, TDest, TParam1>(TParam1 param1, Func<ISelectExpression<TSource, TDest, TParam1>> activator, out Expression<Func<TSource, TDest>> expression)
        {
            PairId id = PairId.GetId<TSource, TDest>();

            Dictionary<PairId, LambdaExpression> expressions = GetOrAddParamExpressionsDictionary(param1, add: false);

            LambdaExpression lambdaExpression;

            if (expressions != null && expressions.TryGetValue(id, out lambdaExpression))
            {
                expression = (Expression<Func<TSource, TDest>>)lambdaExpression;

                return true;
            }

            lock (_sync)
            {
                expressions = expressions ?? GetOrAddParamExpressionsDictionary(param1, add: true);

                if (expressions.TryGetValue(id, out lambdaExpression))
                {
                    expression = (Expression<Func<TSource, TDest>>)lambdaExpression;

                    return true;
                }


                Delegate factoryDelegate;
                Func<TParam1, Expression<Func<TSource, TDest>>> factory;

                if (_expressionFactories.TryGetValue(id, out factoryDelegate))
                {
                    factory = (Func<TParam1, Expression<Func<TSource, TDest>>>)factoryDelegate;
                    expression = factory(param1);
                    expressions.Add(id, expression);

                    TryRemoveFactory(id);

                    return true;
                }

                if (_dynamicExpressionFactories.TryGetValue(id, out factoryDelegate))
                {
                    factory = (Func<TParam1, Expression<Func<TSource, TDest>>>)factoryDelegate;
                    expression = factory(param1);

                    return true;
                }

                if (activator != null)
                {
                    ISelectExpression<TSource, TDest, TParam1> selectResolver = activator();
                    factory = selectResolver.GetSelectExpression;

                    if (selectResolver is ISelectDynamicExpression<TSource, TDest, TParam1>)
                    {
                        _dynamicExpressionFactories.Add(id, factory);
                        expression = factory(param1);

                        return true;
                    }
                    else
                    {
                        _expressionFactories.Add(id, factory);
                        expression = factory(param1);
                        expressions.Add(id, expression);

                        TryRemoveFactory(id);

                        return true;
                    }
                }
            }

            expression = null;
            return false;
        }

        private Dictionary<PairId, LambdaExpression> GetOrAddParamExpressionsDictionary<TParam1>(TParam1 param1, bool add)
        {
            Dictionary<PairId, LambdaExpression> dictionary;
            var key = new Tuple<TParam1>(param1);

            if (!_expressions.TryGetValue(key, out dictionary))
            {
                if (add)
                {
                    dictionary = new Dictionary<PairId, LambdaExpression>();
                    _expressions.Add(key, dictionary);
                }
            }

            return dictionary;
        }

        private void TryRemoveFactory(PairId id)
        {
        }

		private void TryRemoveExpressions<TParam1>(PairId id)
		{
            foreach (var expressionsDictionary in _expressions.Where(kvp=>kvp.Key is Tuple<TParam1>).Select(kvp=>kvp.Value))
            {
                if (expressionsDictionary.ContainsKey(id))
                {
                    expressionsDictionary.Remove(id);
                }
            }			
		}
    }
}

namespace System.Linq.Expressions
{
    public interface ISelectExpression<TSource, TDest, in TParam1, in TParam2>
    {
        Expression<Func<TSource, TDest>> GetSelectExpression(TParam1 param1, TParam2 param2);
    }

    public interface ISelectDynamicExpression<TSource, TDest, in TParam1, in TParam2>: ISelectExpression<TSource, TDest, TParam1, TParam2>
    {
    }
}

namespace LinqExpressionsMapper.Resolvers.SelectsResolver
{
    public class SelectResolverWith2Params
    {
        private readonly object _sync = new object();

        private readonly Dictionary<PairId, Delegate> _dynamicExpressionFactories = new Dictionary<PairId, Delegate>();

        private readonly Dictionary<PairId, Delegate> _expressionFactories = new Dictionary<PairId, Delegate>();
		private readonly Dictionary<object, Dictionary<PairId, LambdaExpression>> _expressions = new Dictionary<object, Dictionary<PairId, LambdaExpression>>();
        public void Register<TSource, TDest, TParam1, TParam2>(ISelectExpression<TSource, TDest, TParam1, TParam2> resolver)
        {
            PairId id = PairId.GetId<TSource, TDest>();

            var factory = (Func<TParam1, TParam2, Expression<Func<TSource, TDest>>>)resolver.GetSelectExpression;

            lock (_sync)
            {
                if (_expressionFactories.ContainsKey(id))
                {
                    _expressionFactories[id] = factory;
					TryRemoveExpressions<TParam1, TParam2>(id);
                }
                else
                {
                    _expressionFactories.Add(id, factory);
                }
            }
        }

        public void Register<TSource, TDest, TParam1, TParam2>(ISelectDynamicExpression<TSource, TDest, TParam1, TParam2> resolver)
        {
            PairId id = PairId.GetId<TSource, TDest>();

            var factory = (Func<TParam1, TParam2, Expression<Func<TSource, TDest>>>)resolver.GetSelectExpression;

            lock (_sync)
            {
                if (_dynamicExpressionFactories.ContainsKey(id))
                {
                    _dynamicExpressionFactories[id] = factory;
                }
                else
                {
                    _dynamicExpressionFactories.Add(id, factory);
                }
            }
        }

        public void RegisterAll(object mapper, Type mapperType, Type[] implementedInterfaces)
        {
            Type selectExpressionType = typeof (ISelectExpression<,,,>);
            Type dynamicSelectExpressionType = typeof (ISelectDynamicExpression<,,,>);

            var selectExpressions = implementedInterfaces.Where(i => i.GUID == selectExpressionType.GUID || i.GUID == dynamicSelectExpressionType.GUID)
                .Select(i =>
                {
                    var genericArgs = i.GetGenericArguments();
                    return new
                    {
                        InterfaceType = i,
						GenericArguments = genericArgs,
                        PairId = PairId.GetId(genericArgs[0], genericArgs[1])
                    };
                })
                .GroupBy(i => i.PairId)
                .Select(
                    g => g.FirstOrDefault(i => i.InterfaceType.GUID == dynamicSelectExpressionType.GUID)
                         ?? g.First(i => i.InterfaceType.GUID == selectExpressionType.GUID)
                )
                .Select(interfaceDescription =>
                {
                    var methodInfo = mapperType.GetInterfaceMap(interfaceDescription.InterfaceType).InterfaceMethods.SingleOrDefault()
                                     ?? interfaceDescription.InterfaceType.GetInterfaces().SelectMany(i => mapperType.GetInterfaceMap(i).TargetMethods).Single();

					Type delegateType = typeof (Func<,,>).MakeGenericType(
						interfaceDescription.GenericArguments[2],
						interfaceDescription.GenericArguments[3],
                    typeof (Expression<>).MakeGenericType(
							typeof (Func<,>).MakeGenericType(interfaceDescription.PairId.SourceId, interfaceDescription.PairId.DestId)
							)
						);

                    var factoryDelegate = Delegate.CreateDelegate(delegateType, mapper, methodInfo, true);

                    return new
                    {
                        InterfaceType = interfaceDescription.InterfaceType,
                        PairId = interfaceDescription.PairId,
                        FactoryDelegate = factoryDelegate
                    };
                })
				.ToList();

			lock(_sync)
			{
				foreach (var selectExpression in selectExpressions)
				{
					Dictionary<PairId, Delegate> cacheDictionary;
					if (selectExpression.InterfaceType.GUID == dynamicSelectExpressionType.GUID)
						cacheDictionary = _dynamicExpressionFactories;
					else if (selectExpression.InterfaceType.GUID == selectExpressionType.GUID)
						cacheDictionary = _expressionFactories;
					else
						throw new NotSupportedException(String.Format("The interface {0} caching is not supported.", selectExpression.InterfaceType.FullName));

					if (cacheDictionary.ContainsKey(selectExpression.PairId))
						cacheDictionary[selectExpression.PairId] = selectExpression.FactoryDelegate;
					else
						cacheDictionary.Add(selectExpression.PairId, selectExpression.FactoryDelegate);
				}
			}
        }

        public bool TryGetFromCache<TSource, TDest, TParam1, TParam2>(TParam1 param1, TParam2 param2, out Expression<Func<TSource, TDest>> expression)
        {
            return TryGetOrActivate(param1, param2, null, out expression);
        }

        public Expression<Func<TSource, TDest>> GetExpression<TSource, TDest, TParam1, TParam2>(TParam1 param1, TParam2 param2)
            where TDest: ISelectExpression<TSource, TDest, TParam1, TParam2>, new()
        {
            try
            {
                Expression<Func<TSource, TDest>> expression;
                if (TryGetOrActivate(param1, param2, () => new TDest(), out expression))
                {
                    return expression;
                }
                else
                {
                    throw new NotImplementedException("Something went wrong.");
                }
            }
            catch (Exception ex)
            {
                throw new NotSupportedException(String.Format("Convert expression for {0} -> {1} does not exist.", typeof(TSource).FullName, typeof(TDest).FullName), ex);
            }
        }

        public Expression<Func<TSource, TDest>> GetExternalExpression<TSource, TDest, TParam1, TParam2>(TParam1 param1, TParam2 param2)
        {
            try
            {
                Expression<Func<TSource, TDest>> expression;
                if (TryGetOrActivate(param1, param2, () => (ISelectExpression<TSource, TDest, TParam1, TParam2>) Activator.CreateInstance<TDest>(), out expression))
                {
                    return expression;
                }
                else
                {
                    throw new NotImplementedException("Something went wrong.");
                }
            }
            catch (Exception ex)
            {
                throw new NotSupportedException(String.Format("Convert expression for {0} -> {1} does not exist.", typeof(TSource).FullName, typeof(TDest).FullName), ex);
            }
        }

        private bool TryGetOrActivate<TSource, TDest, TParam1, TParam2>(TParam1 param1, TParam2 param2, Func<ISelectExpression<TSource, TDest, TParam1, TParam2>> activator, out Expression<Func<TSource, TDest>> expression)
        {
            PairId id = PairId.GetId<TSource, TDest>();

            Dictionary<PairId, LambdaExpression> expressions = GetOrAddParamExpressionsDictionary(param1, param2, add: false);

            LambdaExpression lambdaExpression;

            if (expressions != null && expressions.TryGetValue(id, out lambdaExpression))
            {
                expression = (Expression<Func<TSource, TDest>>)lambdaExpression;

                return true;
            }

            lock (_sync)
            {
                expressions = expressions ?? GetOrAddParamExpressionsDictionary(param1, param2, add: true);

                if (expressions.TryGetValue(id, out lambdaExpression))
                {
                    expression = (Expression<Func<TSource, TDest>>)lambdaExpression;

                    return true;
                }


                Delegate factoryDelegate;
                Func<TParam1, TParam2, Expression<Func<TSource, TDest>>> factory;

                if (_expressionFactories.TryGetValue(id, out factoryDelegate))
                {
                    factory = (Func<TParam1, TParam2, Expression<Func<TSource, TDest>>>)factoryDelegate;
                    expression = factory(param1, param2);
                    expressions.Add(id, expression);

                    TryRemoveFactory(id);

                    return true;
                }

                if (_dynamicExpressionFactories.TryGetValue(id, out factoryDelegate))
                {
                    factory = (Func<TParam1, TParam2, Expression<Func<TSource, TDest>>>)factoryDelegate;
                    expression = factory(param1, param2);

                    return true;
                }

                if (activator != null)
                {
                    ISelectExpression<TSource, TDest, TParam1, TParam2> selectResolver = activator();
                    factory = selectResolver.GetSelectExpression;

                    if (selectResolver is ISelectDynamicExpression<TSource, TDest, TParam1, TParam2>)
                    {
                        _dynamicExpressionFactories.Add(id, factory);
                        expression = factory(param1, param2);

                        return true;
                    }
                    else
                    {
                        _expressionFactories.Add(id, factory);
                        expression = factory(param1, param2);
                        expressions.Add(id, expression);

                        TryRemoveFactory(id);

                        return true;
                    }
                }
            }

            expression = null;
            return false;
        }

        private Dictionary<PairId, LambdaExpression> GetOrAddParamExpressionsDictionary<TParam1, TParam2>(TParam1 param1, TParam2 param2, bool add)
        {
            Dictionary<PairId, LambdaExpression> dictionary;
            var key = new Tuple<TParam1, TParam2>(param1, param2);

            if (!_expressions.TryGetValue(key, out dictionary))
            {
                if (add)
                {
                    dictionary = new Dictionary<PairId, LambdaExpression>();
                    _expressions.Add(key, dictionary);
                }
            }

            return dictionary;
        }

        private void TryRemoveFactory(PairId id)
        {
        }

		private void TryRemoveExpressions<TParam1, TParam2>(PairId id)
		{
            foreach (var expressionsDictionary in _expressions.Where(kvp=>kvp.Key is Tuple<TParam1, TParam2>).Select(kvp=>kvp.Value))
            {
                if (expressionsDictionary.ContainsKey(id))
                {
                    expressionsDictionary.Remove(id);
                }
            }			
		}
    }
}
