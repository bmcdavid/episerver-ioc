using AbstractEpiserverIoc.Abstractions;
using EPiServer.Data;
using EPiServer.Data.Internal;
using EPiServer.Data.Providers;
using EPiServer.Data.Providers.Internal;
using EPiServer.DataAbstraction;
using EPiServer.Framework;
using EPiServer.Framework.Cache;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using EpiserverSite1.Business.Rendering;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace EpiserverSite1.Business.Initialization
{
    public class HybridHttpOrThreadLocal2<T> : IDisposable
    {
        private ThreadLocal<T> _threadLocal;
        private readonly Func<T> _valueFactory;
        private readonly IRequestCache _requestCache;
        private readonly string _name;

        /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice. Creates new instance of <see cref="T:EPiServer.ServiceLocation.Internal.HybridHttpOrThreadLocal`1" /></summary>
        /// <param name="uniqueId">The unique id for this instance</param>
        /// <param name="valueFactory">The factory to get not cached value</param>
        /// <param name="requestCache">The request cache to use</param>
        /// <exclude />
        public HybridHttpOrThreadLocal2(Guid uniqueId, Func<T> valueFactory, IRequestCache requestCache)
        {
            if (uniqueId == Guid.Empty)
                throw new ArgumentException("uniqueId must be set");
            UniqueId = uniqueId;
            _name = uniqueId.ToString("N");
            _valueFactory = valueFactory;
            _threadLocal = new ThreadLocal<T>(valueFactory);
            _requestCache = requestCache;
        }

        /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice. The unique id for this instance
        /// </summary>
        /// <exclude />
        public Guid UniqueId { get; }

        /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice. Accessor to get value from cache or factory (if not cached)
        /// </summary>
        /// <exclude />
        public T Value
        {
            get
            {   
                if (!_requestCache.IsActive && _threadLocal is object)
                    return _threadLocal.Value;

                T x = _requestCache.Get<T>(_name);
                if (EqualityComparer<T>.Default.Equals(x, default(T)))
                {
                    x = _valueFactory();
                    _requestCache.Set<T>(_name, x);
                }
                return x;
            }
        }

        /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice.</summary>
        /// <inheritdoc />
        /// <exclude />
        public void Dispose()
        {
            Dispose(true); 
            GC.SuppressFinalize(this);
        }

        /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice. Dispose implementation
        /// </summary>
        /// <param name="disposing">indicate if disposing</param>
        /// <exclude />
        protected virtual void Dispose(bool disposing)
        {
            if (_threadLocal == null)
                return;
            if (_threadLocal.IsValueCreated)
                (_threadLocal.Value as IDisposable)?.Dispose();
            _threadLocal.Dispose();
            _threadLocal = null;
        }
    }

    public class SqlDatabaseExecutorFactory2 : IDatabaseExecutorFactory, IAsyncDatabaseExecutorFactory, IDisposable
    {
        private readonly HybridHttpOrThreadLocal2<IDatabaseExecutor> _currentHandler;
        private readonly IDatabaseMode _databaseModeService;
        private readonly IDatabaseConnectionResolver _databaseConnectionResolver;
        private readonly DataAccessOptions _dataAccessOptions;
        private readonly ContextCache _contextCache;

        public SqlDatabaseExecutorFactory2()
          : this(ServiceLocator.Current.GetInstance<ContextCache>(), ServiceLocator.Current.GetInstance<IRequestCache>(), ServiceLocator.Current.GetInstance<IDatabaseMode>(), ServiceLocator.Current.GetInstance<IDatabaseConnectionResolver>(), ServiceLocator.Current.GetInstance<DataAccessOptions>())
        {
        }

        public SqlDatabaseExecutorFactory2(
          ContextCache contextCache,
          IRequestCache requestCache,
          IDatabaseMode databaseModeService,
          IDatabaseConnectionResolver databaseConnectionResolver,
          DataAccessOptions dataAccessOptions)
        {
            _contextCache = contextCache;
            _databaseModeService = databaseModeService;
            _currentHandler = new HybridHttpOrThreadLocal2<IDatabaseExecutor>(Guid.NewGuid(), () => CreateDefaultHandler(), requestCache);
            _dataAccessOptions = dataAccessOptions;
            _databaseConnectionResolver = databaseConnectionResolver;
        }

        /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice.</summary>
        /// <internal-api />
        /// <exclude />
        public IDatabaseExecutor CurrentHandler => _currentHandler.Value;

        /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice.</summary>
        /// <internal-api />
        /// <exclude />
        public IDatabaseExecutor CreateDefaultHandler() => Create(_databaseConnectionResolver.Resolve());

        /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice.</summary>
        /// <internal-api />
        /// <exclude />
        public IDatabaseExecutor CreateHandler(
          ConnectionStringOptions connectionStringOption) => Create(connectionStringOption);

        private IDatabaseExecutor Create(ConnectionStringOptions connectionStringOption)
        {
            if (_databaseModeService.DatabaseMode != DatabaseMode.ReadOnly)
                return new SqlDatabaseExecutor(_contextCache, connectionStringOption, _dataAccessOptions.Retries, _dataAccessOptions.RetryDelay, _dataAccessOptions.DatabaseQueryTimeout);

            return new ReadOnlySqlDatabaseExecutor(_contextCache, connectionStringOption, _dataAccessOptions.Retries, _dataAccessOptions.RetryDelay, _dataAccessOptions.DatabaseQueryTimeout);
        }

        public void Dispose() => _currentHandler.Dispose();

        IAsyncDatabaseExecutor IAsyncDatabaseExecutorFactory.CreateDefaultHandler() => CreateDefaultHandler() as IAsyncDatabaseExecutor;
    }

    [InitializableModule]
    [ModuleDependency(typeof(ServiceContainerInitialization))]
    [ModuleDependency(typeof(EPiServer.Data.DataInitialization))]
    public class DependencyResolverInitialization : IConfigurableModule
    {
        public static string EnvironmentName { get; private set; } = "notset";

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            // todo for Grace
            var services = context.Services;
            services.RemoveAll<IDatabaseExecutorFactory>();
            services.RemoveAll<ServiceAccessor<IDatabaseExecutorFactory>>();
            services.RemoveAll<IAsyncDatabaseExecutorFactory>();
            services.RemoveAll<ServiceAccessor<IAsyncDatabaseExecutorFactory>>();

            services.AddSingleton(s => (IDatabaseExecutorFactory)new SqlDatabaseDelegatorFactory(new SqlDatabaseExecutorFactory2()));
            services.Forward<IDatabaseExecutorFactory, IAsyncDatabaseExecutorFactory>();
            services.AddTransient(s => s.GetInstance<IDatabaseExecutorFactory>().CurrentHandler);
            services.AddTransient(s => (ServiceAccessor<IDatabaseExecutor>)(() => s.GetInstance<IDatabaseExecutor>()));
            services.AddTransient(s => s.GetInstance<IAsyncDatabaseExecutorFactory>().CreateDefaultHandler());
            services.AddTransient(s => (ServiceAccessor<IAsyncDatabaseExecutor>)(() => s.GetInstance<IAsyncDatabaseExecutor>()));
            // end for Grace

            EnvironmentName = context.Services.EnvironmentName();

            if (context.Services.IsIntegrationEnvironment())
            {
                // could do something specific to integration here...
            }

            // for DryIoc
#pragma warning disable CS0618 // Type or member is obsolete
            context.Services.AddSingleton<IContentTypeRepository<BlockType>, BlockTypeRepository>();
#pragma warning restore CS0618 // Type or member is obsolete
            // end for DryIoc


            context.ConfigurationComplete += (o, e) =>
            {
                    //Register custom implementations that should be used in favour of the default implementations
                    e.Services
                            .AddTransient<IContentRenderer, ErrorHandlingContentRenderer>()
                            .AddTransient<ContentAreaRenderer, AlloyContentAreaRenderer>();
            };
        }

        public void Initialize(InitializationEngine context)
        {
            DependencyResolver.SetResolver(new ServiceLocatorDependencyResolver(context.Locate.Advanced));
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
