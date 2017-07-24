using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using Ninject.Syntax;

namespace Ninject.Microsoft.DependencyInjection
{
    public static class DependencyInjectorHelper
    {
        public static IServiceProvider GetServiceProvider(this IKernel kernel)
        {
            return kernel.Get<IServiceProvider>();
        }

        public static void Populate(this IKernel kernel, IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            kernel.Rebind<IServiceScopeFactory>().To<NinjectServiceScopeFactory>();
            kernel.Rebind<IServiceProvider>().To<NinjectServiceProvider>();
            BindServices(kernel, serviceDescriptors);
        }

        private static IBindingNamedWithOrOnSyntax<T> InLifecycle<T>(this IBindingInSyntax<T> binding, ServiceLifetime serviceLifetime)
        {
            switch (serviceLifetime)
            {
                case ServiceLifetime.Singleton:
                    return binding.InSingletonScope();
                case ServiceLifetime.Scoped: // TODO What to do with this?
                    return binding.InScope(context =>
                    {
                        var scope = NinjectServiceScopeFactory.CurrentScope;
                        if (scope != null)
                        {
                            return scope;
                        }
                        throw new Exception("Object bound per scope, but not currently within a scope.");
                    });
                case ServiceLifetime.Transient:
                    return binding.InTransientScope();
                default:
                    throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null);
            }
        }

        private static void BindServices(IKernel kernel, IEnumerable<ServiceDescriptor> descriptors)
        {
            foreach (var descriptor in descriptors)
            {
                if (descriptor.ImplementationType != null)
                {
                    kernel.Bind(descriptor.ServiceType)
                          .To(descriptor.ImplementationType)
                          .InLifecycle(descriptor.Lifetime);
                }
                else if (descriptor.ImplementationFactory != null)
                {
                    kernel.Bind(descriptor.ServiceType)
                          .ToMethod(context =>
                          {
                              var serviceProvider = context.Kernel.Get<IServiceProvider>();
                              return descriptor.ImplementationFactory(serviceProvider);
                          })
                          .InLifecycle(descriptor.Lifetime);
                }
                else
                {
                    kernel.Bind(descriptor.ServiceType)
                          .ToConstant(descriptor.ImplementationInstance)
                          .InLifecycle(descriptor.Lifetime);
                }
            }
        }
    }
}