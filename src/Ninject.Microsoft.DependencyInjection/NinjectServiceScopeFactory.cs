using System;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;

using Ninject.Infrastructure.Disposal;

namespace Ninject.Microsoft.DependencyInjection
{
    public class NinjectServiceScopeFactory : IServiceScopeFactory
    {
        // ReSharper disable once InconsistentNaming
        private static readonly AsyncLocal<IServiceScope> _currentScope = new AsyncLocal<IServiceScope>();

        public static IServiceScope CurrentScope => _currentScope.Value;

        private readonly IServiceProvider _serviceProvider;

        public NinjectServiceScopeFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IServiceScope CreateScope()
        {
            return _currentScope.Value = CurrentScope ?? new NinjectServiceScope(_serviceProvider);
        }
    }

    public class NinjectServiceScope : IServiceScope, INotifyWhenDisposed
    {
        public IServiceProvider ServiceProvider { get; }

        public bool IsDisposed { get; private set; }

        public event EventHandler Disposed;

        public NinjectServiceScope(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public void Dispose()
        {
            IsDisposed = true;
            Disposed?.Invoke(this, EventArgs.Empty);
        }
    }
}