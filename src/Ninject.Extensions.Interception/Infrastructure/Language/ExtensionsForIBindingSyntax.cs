#region License

// 
// Author: Ian Davis <ian@innovatian.com>
// Copyright (c) 2010, Innovatian Software, LLC
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 

#endregion

#region Using Directives

using System;
using System.Reflection;
using Ninject.Extensions.Interception.Advice;
using Ninject.Extensions.Interception.Advice.Builders;
using Ninject.Extensions.Interception.Advice.Syntax;
using Ninject.Extensions.Interception.Parameters;
using Ninject.Extensions.Interception.Registry;
using Ninject.Syntax;

#endregion

namespace Ninject.Extensions.Interception.Infrastructure.Language
{
    /// <summary>
    /// 
    /// </summary>
    public static class ExtensionsForIBindingSyntax
    {
        /// <summary>
        /// Indicates that instances associated with this binding will be proxied.
        /// </summary>
        /// <typeparam name="T">The type associated with this binding.</typeparam>
        /// <param name="bindingSyntax">The binding syntax target.</param>
        /// <param name="additionalInterfaces">The additional interfaces for the proxy.</param>
        /// <returns>
        ///     An <see cref="IAdviceTargetSyntax"/> instance which allows the attachment of an <see cref="IInterceptor"/>.
        /// </returns>
        public static IAdviceTargetSyntax Intercept<T>( this IBindingOnSyntax<T> bindingSyntax, params Type[] additionalInterfaces )
        {
            return Intercept(bindingSyntax, 
                mi => mi.DeclaringType != typeof(object), 
                additionalInterfaces);
        }

        /// <summary>
        /// Indicates that instances associated with this binding will be proxied.
        /// Only methods that match the specified predicate will be intercepted.
        /// </summary>
        /// <typeparam name="T">The type associated with this binding.</typeparam>
        /// <param name="bindingSyntax">The binding syntax target.</param>
        /// <param name="methodPredicate">The method predicate that defines if a method shall be intercepted.</param>
        /// <param name="additionalInterfaces">The additional interfaces for the proxy.</param>
        /// <returns>An <see cref="IAdviceTargetSyntax" /> instance which allows the attachment of an <see cref="IInterceptor" />.</returns>
        public static IAdviceTargetSyntax Intercept<T>(this IBindingOnSyntax<T> bindingSyntax, Predicate<MethodInfo> methodPredicate, params Type[] additionalInterfaces)
        {
            IKernel kernel = bindingSyntax.Kernel;

            foreach (var additionalInterface in additionalInterfaces)
            {
                bindingSyntax.BindingConfiguration.Parameters.Add(new AdditionalInterfaceParameter(additionalInterface));
            }

            IAdvice advice = kernel.Components.Get<IAdviceFactory>()
                .Create(context => ReferenceEquals(bindingSyntax.BindingConfiguration, context.Binding.BindingConfiguration), methodPredicate);
            kernel.Components.Get<IAdviceRegistry>().Register(advice);

            return new AdviceBuilder(advice);
        }
    }
}