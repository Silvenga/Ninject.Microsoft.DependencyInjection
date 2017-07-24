using System;

using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Microsoft.DependencyInjection
{
    public class NinjectServiceProvider : IServiceProvider, ISupportRequiredService
    {
        public NinjectServiceProvider(IKernel kernel)
        {
            Kernel = kernel;
        }

        private IKernel Kernel { get; }

        public object GetService(Type serviceType)
        {
            return Kernel.TryGet(serviceType);
        }

        public object GetRequiredService(Type serviceType)
        {
            return Kernel.Get(serviceType);
        }
    }
}