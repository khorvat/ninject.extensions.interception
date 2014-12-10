#region License

// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL). See the file
// LICENSE.txt for details.

#endregion License

#region Using Directives

using System;
using System.Threading.Tasks;

#endregion Using Directives

namespace Ninject.Extensions.Interception
{
    /// <summary>
    /// Provides the ability to supply an action which will be invoked during method interception.
    /// </summary>
    public class ActionInterceptor : IInterceptor
    {
        #region Fields

        private readonly Action<IInvocation> _interceptAction;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionInterceptor"/> class.
        /// </summary>
        /// <param name="interceptAction">The intercept action to take.</param>
        public ActionInterceptor(Action<IInvocation> interceptAction)
        {
            _interceptAction = interceptAction;
        }

        #endregion Constructors

        #region IInterceptor Members

        /// <summary>
        /// Intercepts the specified invocation.
        /// </summary>
        /// <param name="invocation">The invocation to intercept.</param>
        public void Intercept(IInvocation invocation)
        {
            _interceptAction(invocation);
        }

        /// <summary>
        /// Intercepts the specified invocation.
        /// </summary>
        /// <param name="invocation">The invocation to intercept.</param>
        public Task InterceptAsync(IInvocation invocation)
        {
            _interceptAction(invocation);
            return Task.FromResult(true);
        }

        #endregion IInterceptor Members
    }
}