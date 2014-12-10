#region License

// Author: Nate Kohari <nate@enkari.com> Copyright (c) 2007-2010, Enkari, Ltd.
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL). See the file
// LICENSE.txt for details.

#endregion License

#region Using Directives

using Ninject.Components;
using System;
using System.Collections.Concurrent;
using System.Reflection;

#endregion Using Directives

namespace Ninject.Extensions.Interception.Injection
{
    /// <summary>
    /// Creates instances of injectors.
    /// </summary>
    public abstract class InjectorFactoryBase : NinjectComponent, IInjectorFactory
    {
        #region Fields

        private readonly ConcurrentDictionary<MethodInfo, IMethodInjector> _methodInjectors =
            new ConcurrentDictionary<MethodInfo, IMethodInjector>();

        #endregion Fields

        #region IInjectorFactory Members

        /// <summary>
        /// Gets an injector for the specified method.
        /// </summary>
        /// <param name="method">The method that the injector will invoke.</param>
        /// <returns>A new injector for the method.</returns>
        public IMethodInjector GetInjector(MethodInfo method)
        {
            if (method.IsGenericMethodDefinition)
            {
                throw new InvalidOperationException(
                    /*ExceptionFormatter.CannotCreateInjectorFromGenericTypeDefinition(method)*/ );
            }

            return _methodInjectors.GetOrAdd(method, (MethodInfo methodInfo) =>
            {
                return CreateInjector(method);
            });
        }

        #endregion IInjectorFactory Members

        #region Methods

        /// <summary>
        /// Creates a new method injector.
        /// </summary>
        /// <param name="method">The method that the injector will invoke.</param>
        /// <returns>A new injector for the method.</returns>
        protected abstract IMethodInjector CreateInjector(MethodInfo method);

        #endregion Methods
    }
}