#if !SILVERLIGHT && !NO_LINFU

#region License

// Author: Nate Kohari <nate@enkari.com> Copyright (c) 2007-2010, Enkari, Ltd.
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL). See the file
// LICENSE.txt for details.

#endregion License

#region Using Directives

using LinFu.DynamicProxy;
using Ninject.Activation;
using Ninject.Extensions.Interception.Request;
using System;
using System.Reflection;
using System.Threading.Tasks;

#endregion Using Directives

namespace Ninject.Extensions.Interception.Wrapper
{
    /// <summary>
    /// Defines an interception wrapper that can convert a LinFu <see cref="InvocationInfo"/> into a Ninject <see
    /// cref="IProxyRequest"/> for interception.
    /// </summary>
    public class LinFuWrapper : StandardWrapper, LinFu.DynamicProxy.IInterceptor
    {
        protected static MethodInfo interceptGenericAsyncMethod = typeof(LinFuWrapper).GetMethod("InterceptGenericAsync", BindingFlags.Instance | BindingFlags.NonPublic);
        private static Type taskType = typeof(Task);
        private static Type taskGenericType = typeof(Task<>);

        /// <summary>
        /// Initializes a new instance of the <see cref="LinFuWrapper"/> class.
        /// </summary>
        /// <param name="kernel">The kernel associated with the wrapper.</param>
        /// <param name="context">The context in which the instance was activated.</param>
        /// <param name="instance">The wrapped instance.</param>
        public LinFuWrapper(IKernel kernel, IContext context, object instance)
            : base(kernel, context, instance)
        {
        }

        #region IInterceptor Members

        object LinFu.DynamicProxy.IInterceptor.Intercept(InvocationInfo info)
        {
            IProxyRequest request = CreateRequest(info);
            IInvocation invocation = CreateInvocation(request);

            if (taskType.IsAssignableFrom(invocation.Request.Method.ReturnType))
            {
                if (invocation.Request.Method.ReturnType.IsGenericType &&
                    invocation.Request.Method.ReturnType.GetGenericTypeDefinition() == taskGenericType)
                {
                    Type itemType = invocation.Request.Method.ReturnType.GetGenericArguments()[0];
                    var genericMethod = interceptGenericAsyncMethod.MakeGenericMethod(itemType);
                    try
                    {
                        return genericMethod.Invoke(this, new object[] { invocation });
                    }
                    catch (System.Reflection.TargetInvocationException ex)
                    {
                        throw ex.InnerException;
                    }
                }
                else
                {
                    return InterceptAsync(invocation);
                }
            }
            else
            {
                invocation.Proceed();
                return invocation.ReturnValue;
            }
        }

        private async Task<T> InterceptGenericAsync<T>(IInvocation invocation)
        {
            await invocation.ProceedAsync();

            return await ((Task<T>)invocation.ReturnValue);
        }

        private async Task InterceptAsync(IInvocation invocation)
        {
            await invocation.ProceedAsync();

            //Note: There is no return type for this kind of operation
            //return invocation.ReturnValue;
        }

        #endregion IInterceptor Members

        private IProxyRequest CreateRequest(InvocationInfo info)
        {
            var requestFactory = Context.Kernel.Components.Get<IProxyRequestFactory>();

            return requestFactory.Create(
                Context,
                info.Target,
                Instance,
                info.TargetMethod,
                info.Arguments,
                info.TypeArguments);
        }
    }
}

#endif //!SILVERLIGHT && !NO_LINFU