using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer.ServiceLocation;
using EpiserverSite1.Business.Initialization;

namespace EpiserverSite1.Business
{
    public class ServiceLocatorDependencyResolver : IDependencyResolver
    {
        readonly IServiceLocator _serviceLocator;

        public ServiceLocatorDependencyResolver(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType.IsInterface || serviceType.IsAbstract)
            {
                return GetInterfaceService(serviceType);
            }
            return GetConcreteService(serviceType);
        }

        private object GetConcreteService(Type serviceType)
        {
            try
            {
                // Can't use TryGetInstance here because it won’t create concrete types
                return ResolveLocator().GetInstance(serviceType);
            }
            catch (ActivationException)
            {
                return null;
            }
        }

        private object GetInterfaceService(Type serviceType)
        {
            object instance;
            return ResolveLocator().TryGetExistingInstance(serviceType, out instance) ? instance : null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return ResolveLocator().GetAllInstances(serviceType).Cast<object>();
        }

        private IServiceLocator ResolveLocator()
        {
            return HttpContext.Current.Items[nameof(HttpIoc)] as IServiceLocator ??
                _serviceLocator;
        }
    }
}
