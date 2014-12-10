#region License

// Author: Nate Kohari <nate@enkari.com> Copyright (c) 2007-2010, Enkari, Ltd.
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL). See the file
// LICENSE.txt for details.

#endregion License

#region Using Directives

using Ninject.Components;
using Ninject.Extensions.Interception.Advice;
using Ninject.Extensions.Interception.Infrastructure.Language;
using Ninject.Extensions.Interception.Request;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion Using Directives

namespace Ninject.Extensions.Interception.Registry
{
    using Ninject.Activation;
    using System.Collections;
    using System.Collections.Concurrent;

    /// <summary>
    /// Collects advice defined for methods.
    /// </summary>
    public class AdviceRegistry : NinjectComponent, IAdviceRegistry
    {
        #region Fields

        private readonly ConcurrentBag<IAdvice> _advice = new ConcurrentBag<IAdvice>();

        private readonly ConcurrentDictionary<RuntimeMethodHandle, ConcurrentDictionary<RuntimeTypeHandle, List<IInterceptor>>> _cache =
            new ConcurrentDictionary<RuntimeMethodHandle, ConcurrentDictionary<RuntimeTypeHandle, List<IInterceptor>>>();

        #endregion Fields

        #region IAdviceRegistry Members

        /// <summary>
        /// Gets a value indicating whether one or more dynamic interceptors have been registered.
        /// </summary>
        public bool HasDynamicAdvice { get; private set; }

        /// <summary>
        /// Gets the interceptors that should be invoked for the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>A collection of interceptors, ordered by the priority in which they should be invoked.</returns>
        public ICollection<IInterceptor> GetInterceptors(IProxyRequest request)
        {
            RuntimeMethodHandle methodHandle = request.Method.GetMethodHandle();
            RuntimeTypeHandle typeHandle = request.Target.GetType().TypeHandle;

            ICollection<IInterceptor> interceptors = null;
            ConcurrentDictionary<RuntimeTypeHandle, List<IInterceptor>> methodCache = _cache.GetOrAdd(methodHandle, (RuntimeMethodHandle handle) =>
            {
                return new ConcurrentDictionary<RuntimeTypeHandle, List<IInterceptor>>();
            });

            interceptors = methodCache.GetOrAdd(typeHandle, (RuntimeTypeHandle handle) =>
            {
                if (!HasDynamicAdvice)
                {
                    // If there are no dynamic interceptors defined, we can safely cache the results. Otherwise, we have
                    // to evaluate and re-activate the interceptors each time.
                    var interceptorsForRequest = GetInterceptorsForRequest(request);
                    if (interceptorsForRequest != null)
                    {
                        return interceptorsForRequest.ToList();
                    }
                }
                return null;
            });
            return interceptors ?? GetInterceptorsForRequest(request);
        }

        /// <summary>
        /// Determines whether an advice for the specified context exists.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if an advice for the specified context exists.; otherwise, <c>false</c>.</returns>
        public bool HasAdvice(IContext context)
        {
            return _advice.Any(a => a.IsDynamic && a.Condition(context));
        }

        /// <summary>
        /// Determines whether any static advice has been registered for the specified type.
        /// </summary>
        /// <param name="type">The type in question.</param>
        /// <returns><see langword="True"/> if advice has been registered, otherwise <see langword="false"/>.</returns>
        public bool HasStaticAdvice(Type type)
        {
            // TODO
            return true;
        }

        /// <summary>
        /// Registers the specified advice.
        /// </summary>
        /// <param name="advice">The advice to register.</param>
        public void Register(IAdvice advice)
        {
            if (advice.IsDynamic)
            {
                HasDynamicAdvice = true;
                _cache.Clear();
            }

            _advice.Add(advice);
        }

        #endregion IAdviceRegistry Members

        #region Methods

        private ICollection<IInterceptor> GetInterceptorsForRequest(IProxyRequest request)
        {
            List<IAdvice> matches = _advice.Where(advice => advice.Matches(request)).ToList();
            matches.Sort((lhs, rhs) => lhs.Order - rhs.Order);

            List<IInterceptor> interceptors = matches.Convert(a => a.GetInterceptor(request)).ToList();
            return interceptors;
        }

        #endregion Methods
    }
}