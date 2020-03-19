using AbstractEpiserverIoc.Abstractions;
using EPiServer.Framework.Cache;
using EPiServer.ServiceLocation;
using EPiServer.ServiceLocation.Internal; // todo: requires internal
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace AbstractEpiserverIoc.Core
{
    public class ServiceConfigurationProvider : IServiceConfigurationProviderWithEnvironment, IRegisteredService, IInterceptorRegister
    {
        private static readonly Dictionary<int, MethodInfo> _refectedMethods = new Dictionary<int, MethodInfo>(4);
        private static readonly Type _serviceLocatorType = typeof(IServiceLocator);
        private readonly IServiceCollectionExtended _serviceCollection;
        private MethodInfo _funcFactory = typeof(FuncFactory).GetMethod("Get");
        private MicrosoftServiceDescriptor _latestType;

        public ServiceConfigurationProvider(IServiceCollectionExtended serviceCollection, IEpiserverEnvironment episerverEnvironment)
        {
            _serviceCollection = serviceCollection;
            Environment = episerverEnvironment;
        }

        internal IServiceLocator InternalServiceLocator { get; set; }

        public IEpiserverEnvironment Environment { get; }

        public virtual IRegisteredService Add(Type serviceType, Type implementationType, ServiceInstanceScope lifetime)
        {
            //if (HasOptionAttribute(serviceType))
            //{
            //    ValidateOptionUsage(serviceType, implementationType, lifetime);

            //    var optionsType = typeof(IOptions<>).MakeGenericType(serviceType);
            //    _serviceCollection.Add(_latestType = new MicrosoftServiceDescriptor(serviceType, s => (s.GetService(optionsType) as IOptions<object>).Value,
            //       ConvertLifeTime(lifetime)));
            //}
            //else
            if (lifetime == ServiceInstanceScope.Hybrid || lifetime == ServiceInstanceScope.HttpContext)
            {
                var genericMethod = GetMethodInfo(lifetime, isFunc: false).MakeGenericMethod(serviceType, implementationType);
                genericMethod.Invoke(null, new object[] { this });
            }
            else
            {
                var descriptor = new MicrosoftServiceDescriptor(serviceType, implementationType, ConvertLifeTime(lifetime));

                return AddInternal(descriptor);
            }

            return this;
        }

        public virtual IRegisteredService Add(Type serviceType, Func<IServiceLocator, object> implementationFactory, ServiceInstanceScope lifetime)
        {
            if (HasOptionAttribute(serviceType))
            {
                throw new ArgumentException("Types decorated with options can not be registered with a factory");
            }
            else if (lifetime == ServiceInstanceScope.Hybrid || lifetime == ServiceInstanceScope.HttpContext)
            {
                var genericMethod = GetMethodInfo(lifetime, isFunc: true).MakeGenericMethod(serviceType);
                var funcFactoryMethod = _funcFactory.MakeGenericMethod(serviceType);
                genericMethod.Invoke(null, new object[] { this, funcFactoryMethod.Invoke(null, new object[] { implementationFactory }) });
            }
            else
            {
                var descriptor = new MicrosoftServiceDescriptor(serviceType, provider => implementationFactory(provider as IServiceLocator), ConvertLifeTime(lifetime));

                return AddInternal(descriptor);
            }

            return this;
        }

        public virtual IRegisteredService Add(Type serviceType, object instance)
        {
            var descriptor = new MicrosoftServiceDescriptor(serviceType, instance);

            return AddInternal(descriptor);
        }

        public virtual IServiceConfigurationProvider AddServiceAccessor()
        {
            ReflectiveServiceConfigurationHelper.RegisterServiceAccessorDelegates(this, _latestType.ServiceType);

            return this;
        }

        public virtual bool Contains(Type serviceType) => _serviceCollection.Any(sd => sd.ServiceType == serviceType);

        public void Intercept<T>(Func<IServiceLocator, T, T> interceptorFactory) where T : class
        {
            //To change as little as possible we replace in same place in list
            var toBeReplacedIndexes = new List<int>();
            for (int i = 0; i < _serviceCollection.Count; i++)
            {
                if (_serviceCollection[i].ServiceType == typeof(T))
                {
                    toBeReplacedIndexes.Add(i);
                }
            }

            foreach (var index in toBeReplacedIndexes)
            {
                var previous = index > 1 ? _serviceCollection[index - 1] : null;
                if (previous?.ServiceType == typeof(HybridHttpOrThreadLocal<T>))
                {
                    HybridHttpOrThreadLocal<T> hybridAccessor(IServiceProvider s) =>
                        ((HybridHttpOrThreadLocal<T>)GetDefaultFactory(typeof(HybridHttpOrThreadLocal<T>), previous).Invoke(s));
                    var decorated = new MicrosoftServiceDescriptor(
                      previous.ServiceType,
                      s => new HybridHttpOrThreadLocal<T>(hybridAccessor(s).UniqueId, () => interceptorFactory(s as IServiceLocator, hybridAccessor(s).Value), s.GetService<EPiServer.Framework.Cache.IRequestCache>()),
                      previous.Lifetime);
                    _serviceCollection[index - 1] = decorated;
                }
                else if (previous?.ServiceType == typeof(RequestOrFactory<T>))
                {
                    RequestOrFactory<T> valueAccessor(IServiceProvider s) => ((RequestOrFactory<T>)GetDefaultFactory(typeof(RequestOrFactory<T>), previous).Invoke(s));
                    var decorated = new MicrosoftServiceDescriptor(
                      previous.ServiceType,
                      s => new RequestOrFactory<T>(valueAccessor(s).UniqueId, () => interceptorFactory(s as IServiceLocator, valueAccessor(s).Value), s.GetService<EPiServer.Framework.Cache.IRequestCache>()),
                      previous.Lifetime);
                    _serviceCollection[index - 1] = decorated;
                }
                else
                {
                    var existing = _serviceCollection[index];
                    var decorated = new MicrosoftServiceDescriptor(
                        existing.ServiceType,
                        s => interceptorFactory(s as IServiceLocator, (T)GetDefaultFactory(typeof(T), existing).Invoke(s)),
                        existing.Lifetime);
                    _serviceCollection[index] = decorated;
                }
            }
        }

        public void Intercept2<T>(Func<IServiceLocator, T, T> interceptorFactory) where T : class
        {
            _serviceCollection.Decorate<T>((existing, provider) => ResolveDecorator(provider, existing, interceptorFactory));
        }

        public virtual IServiceConfigurationProvider RemoveAll(Type serviceType)
        {
            if (serviceType is null) { throw new ArgumentNullException(nameof(serviceType)); }

            var toRemove = _serviceCollection.Where(sd => serviceType == sd.ServiceType);

            foreach (var serviceToRemove in toRemove)
            {
                _serviceCollection.Remove(serviceToRemove);

                if (_serviceCollection.IsConfigured)
                {
                    var args = new ServiceCollectionChangedArgs(serviceToRemove, isRemove: true);
                    _serviceCollection.ServiceCollectionChanged?.Invoke(args);
                }
            }

            return this;
        }

        protected virtual IRegisteredService AddInternal(MicrosoftServiceDescriptor descriptor)
        {
            if (_serviceCollection.IsConfigured)
            {
                var args = new ServiceCollectionChangedArgs(descriptor, isRemove: false);
                _serviceCollection.ServiceCollectionChanged?.Invoke(args);
            }

            _serviceCollection.Add(descriptor);
            _latestType = descriptor;
            return this;
        }

        private static ServiceLifetime ConvertLifeTime(ServiceInstanceScope lifetime)
        {
            switch (lifetime)
            {
                case ServiceInstanceScope.Singleton:
                    return ServiceLifetime.Singleton;
#pragma warning disable CS0618 // Type or member is obsolete
                case ServiceInstanceScope.Unique:
                case ServiceInstanceScope.PerRequest:
#pragma warning restore CS0618 // Type or member is obsolete
                case ServiceInstanceScope.Transient:
                    return ServiceLifetime.Transient;
                case ServiceInstanceScope.HttpContext:
                case ServiceInstanceScope.Hybrid:
                    return ServiceLifetime.Scoped;
            }

            throw new NotSupportedException(lifetime.ToString() + " is not supported!");
        }

        private static MethodInfo GetMethodInfo(ServiceInstanceScope lifetime, bool isFunc)
        {
            int key;

            if (isFunc)
            {
                key = lifetime == ServiceInstanceScope.HttpContext ? 0 : 1;
            }
            else
            {
                key = lifetime == ServiceInstanceScope.HttpContext ? 2 : 3;
            }

            if (!_refectedMethods.TryGetValue(key, out var method))
            {
                var paramLength = isFunc ? 2 : 1;
                var methodName = lifetime == ServiceInstanceScope.Hybrid ? "AddHttpContextOrThreadScoped" : "AddHttpContextScoped";
                method = typeof(EPiServer.ServiceLocation.ServiceConfigurationProviderExtensions).GetMethods()
                        .Single(m => m.Name.Equals(methodName) && m.GetParameters().Length == paramLength);

                _refectedMethods[key] = method;
            }

            return method;
        }

        private static bool HasOptionAttribute(Type serviceType)
        {
            return serviceType.CustomAttributes.Any(a => a.AttributeType.Name.Equals(typeof(OptionsAttribute).Name));
            // && a.AttributeType.Assembly.FullName.StartsWith("EPiServer"));
        }

        private static T ResolveDecorator<T>(IServiceProvider serviceProvider, T existing, Func<IServiceLocator, T, T> interceptorFactory)
        {
            var locator = serviceProvider.GetRequiredService(_serviceLocatorType);

            return interceptorFactory(locator as IServiceLocator, existing);
        }

        private static void ValidateOptionUsage(Type serviceType, Type implementationType, ServiceInstanceScope lifetime)
        {
            if (lifetime != ServiceInstanceScope.Singleton)
                throw new ArgumentException($"Types decorated with Options attribute must be registered as Singleton");
            if (implementationType != serviceType)
                throw new ArgumentException($"Options cannot be registered with different implementation");
            if (serviceType.IsAbstract)
                throw new ArgumentException($"Classes decorated as Options must be concrete types");
            //if (serviceType.GetConstructor(Type.EmptyTypes) == null)
            //    throw new ArgumentException($"Classes decorated as Options must have a default constuctor, {serviceType.FullName}");
        }
        private Func<IServiceProvider, object> GetDefaultFactory(Type _, MicrosoftServiceDescriptor descriptor)
        {
            // https://github.com/dotnet/extensions/blob/5bcbaa2cd1b54da4d919841df815820185189a5c/src/Shared/src/ActivatorUtilities/ActivatorUtilities.cs
            if (descriptor.ImplementationInstance is object)
                return s => descriptor.ImplementationInstance;
            else if (descriptor.ImplementationType is object)
                return s =>
                {
                    // todo: show to Jeff
                    //return ActivatorUtilities.GetServiceOrCreateInstance(s, descriptor.ImplementationType); // recursive and bad
                    return ActivatorUtilities.CreateInstance(s, descriptor.ImplementationType); //not recursive and goode
                };
            else
                return s => descriptor.ImplementationFactory(s);
        }
    }

    internal static class FuncFactory
    {
        // This method is invoked via reflection from ServiceConfigurationScanner.
        [System.Diagnostics.DebuggerStepThrough]
        public static Func<IServiceLocator, TService> Get<TService>(Func<IServiceLocator, object> untyped)
        {
            return (s) => (TService)untyped(s);
        }
    }
    /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice. Helper class to store variables that should be cached either on HTTP context or on thread local storage
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    /// <exclude />
    public class HybridHttpOrThreadLocal2<T> : IDisposable
    {
        private readonly Guid _uniqueId;
        private ThreadLocal<T> _threadLocal;
        private readonly Func<T> _valueFactory;
        private readonly IRequestCache _requestCache;
        private string _name;

        /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice. Creates new instance of <see cref="T:EPiServer.ServiceLocation.Internal.HybridHttpOrThreadLocal`1" /></summary>
        /// <param name="uniqueId">The unique id for this instance</param>
        /// <param name="valueFactory">The factory to get not cached value</param>
        /// <param name="requestCache">The request cache to use</param>
        /// <exclude />
        public HybridHttpOrThreadLocal2(Guid uniqueId, Func<T> valueFactory, IRequestCache requestCache)
        {
            //Validator.ThrowIfNull(nameof(valueFactory), (object)valueFactory);
            //Validator.ThrowIfNull(nameof(requestCache), (object)requestCache);
            if (uniqueId == Guid.Empty)
                throw new ArgumentException("uniqueId must be set");
            this._uniqueId = uniqueId;
            this._name = uniqueId.ToString("N");
            this._valueFactory = valueFactory;
            this._threadLocal = new ThreadLocal<T>(valueFactory);
            this._requestCache = requestCache;
        }

        /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice. The unique id for this instance
        /// </summary>
        /// <exclude />
        public Guid UniqueId
        {
            get
            {
                return this._uniqueId;
            }
        }

        /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice. Accessor to get value from cache or factory (if not cached)
        /// </summary>
        /// <exclude />
        public T Value
        {
            get
            {
                if (!this._requestCache.IsActive)
                    return this._threadLocal.Value;
                T x = this._requestCache.Get<T>(this._name);
                if (EqualityComparer<T>.Default.Equals(x, default(T)))
                {
                    x = this._valueFactory();
                    this._requestCache.Set<T>(this._name, x);
                }
                return x;
            }
        }

        /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice.</summary>
        /// <inheritdoc />
        /// <exclude />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice. Dispose implementation
        /// </summary>
        /// <param name="disposing">indicate if disposing</param>
        /// <exclude />
        protected virtual void Dispose(bool disposing)
        {
            if (this._threadLocal == null)
                return;
            if (this._threadLocal.IsValueCreated)
                ((object)this._threadLocal.Value as IDisposable)?.Dispose();
            this._threadLocal.Dispose();
            this._threadLocal = (ThreadLocal<T>)null;
        }
    }

}