using EPiServer.ServiceLocation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

[assembly: InternalsVisibleTo("AbstractEpiserverIoc.Core.Tests")]

namespace AbstractEpiserverIoc.Core
{
    internal class ExtendedServiceCollection : ServiceCollection, IServiceCollectionExtended
    {
        public bool IsConfigured { get; set; }

        private readonly List<KeyValuePair<Type, Func<IServiceLocator,object,object>>> _interceptors = new List<KeyValuePair<Type, Func<IServiceLocator, object, object>>>(100);

        public Action<MicrosoftServiceDescriptor> AfterConfigurationAddedDescriptor { get; set; }

        public Action<MicrosoftServiceDescriptor> AfterConfigurationRemovedDescriptor { get; set; }

        public void AddInterceptor<T>(Func<IServiceLocator, T, T> interceptor) where T : class
        {
            _interceptors.Add(new KeyValuePair<Type, Func<IServiceLocator, object, object>>(typeof(T), ConvertToFuncObject(interceptor)));
        }

        public static Func<IServiceLocator, object, object> ConvertToFuncObject<T>(Func<IServiceLocator, T, T> interceptor) where T : class
        {
            return (locator, existing) => interceptor.Invoke(locator, (T)existing);
        }

        public object TestInterceptor<T>()
        {
            object o = null;
            IServiceLocator sl = null;
            foreach(var interceptor in _interceptors.Where(i => i.Key == typeof(T)))
            {
                o = interceptor.Value.Invoke(sl, o);
            }

            return o;
        }
    }
}