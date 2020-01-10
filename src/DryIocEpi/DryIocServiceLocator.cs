using DryIoc;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: EPiServer.ServiceLocation.AutoDiscovery.ServiceLocatorFactory(typeof(DryIocEpi.DryIocLocatorFactory))]

namespace DryIocEpi
{
    public class DryIocServiceLocator : IServiceLocator, IDisposable, IServiceLocatorCreateScope
    {
        private readonly IResolverContext _resolveContext;
        private int count = 0;

        public static Action<String> CheckType;

        public DryIocServiceLocator(IResolverContext context) => _resolveContext = context;

        public void Dispose()
        {
            if (_resolveContext.IsDisposed) { return; }
            _resolveContext.Dispose();
        }

        public void Verify()
        {
            (_resolveContext as IContainer)?.Validate();
        }

        public IEnumerable<object> GetAllInstances(Type serviceType) =>
            _resolveContext.ResolveMany(serviceType);

        public object GetInstance(Type serviceType) => _resolveContext.Resolve(serviceType, ifUnresolvedReturnDefault: true);

        public TService GetInstance<TService>()
        {
            try
            {
                count++;
                return (TService)_resolveContext.Resolve<TService>(ifUnresolvedReturnDefault: true);
            }
            catch (Exception e)
            {
                var issues = (_resolveContext as IContainer)?
                    .Validate();
                var issueList = issues
                    .Select(kvp => kvp.Key.ToString() + " = " + kvp.Value.ToString())
                    .ToList();


                CheckType?.Invoke(string.Join(Environment.NewLine,issues));

                throw new Exception("Unable to resolve: " + typeof(TService).FullName, e);
            }
        }

        public ICollection<string> Debug => (_resolveContext as Container)?
            .GetServiceRegistrations()
            .Select(x => x.ToString())
            .ToList();

        public object GetService(Type serviceType) => _resolveContext.Resolve(serviceType);

        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {   
            instance = _resolveContext.Resolve(serviceType, ifUnresolvedReturnDefault: true);

            return instance is object;
        }

        public IServiceLocator CreateScope() => new DryIocServiceLocator(_resolveContext.OpenScope());
    }

    public interface IServiceLocatorCreateScope
    {
        IServiceLocator CreateScope();
    }
}
