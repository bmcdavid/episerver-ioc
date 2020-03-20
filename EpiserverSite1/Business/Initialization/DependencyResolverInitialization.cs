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
using EPiServer.ServiceLocation.Internal;
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
                
                if (!this._requestCache.IsActive && this._threadLocal is object)
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
            this._contextCache = contextCache;
            this._databaseModeService = databaseModeService;
            this._currentHandler = new HybridHttpOrThreadLocal2<IDatabaseExecutor>(Guid.NewGuid(), (Func<IDatabaseExecutor>)(() => this.CreateDefaultHandler()), requestCache);
            this._dataAccessOptions = dataAccessOptions;
            this._databaseConnectionResolver = databaseConnectionResolver;
        }

        /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice.</summary>
        /// <internal-api />
        /// <exclude />
        public IDatabaseExecutor CurrentHandler 
        {
            get
            {
                return this._currentHandler.Value;
            }
        }

        /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice.</summary>
        /// <internal-api />
        /// <exclude />
        public IDatabaseExecutor CreateDefaultHandler()
        {
            return this.Create(this._databaseConnectionResolver.Resolve());
        }

        /// <summary>Unsupported INTERNAL API! Not covered by semantic versioning; might change without notice.</summary>
        /// <internal-api />
        /// <exclude />
        public IDatabaseExecutor CreateHandler(
          ConnectionStringOptions connectionStringOption)
        {
            return this.Create(connectionStringOption);
        }

        private IDatabaseExecutor Create(ConnectionStringOptions connectionStringOption)
        {
            if (this._databaseModeService.DatabaseMode != DatabaseMode.ReadOnly)
                return (IDatabaseExecutor)new SqlDatabaseExecutor(this._contextCache, connectionStringOption, this._dataAccessOptions.Retries, this._dataAccessOptions.RetryDelay, this._dataAccessOptions.DatabaseQueryTimeout);
            return (IDatabaseExecutor)new ReadOnlySqlDatabaseExecutor(this._contextCache, connectionStringOption, this._dataAccessOptions.Retries, this._dataAccessOptions.RetryDelay, this._dataAccessOptions.DatabaseQueryTimeout);
        }

        public void Dispose()
        {
            this._currentHandler.Dispose();
        }

        IAsyncDatabaseExecutor IAsyncDatabaseExecutorFactory.CreateDefaultHandler()
        {
            return this.CreateDefaultHandler() as IAsyncDatabaseExecutor;
        }
    }

    [InitializableModule]
    [ModuleDependency(typeof(ServiceContainerInitialization))]
    [ModuleDependency(typeof(EPiServer.Data.DataInitialization))]
    public class DependencyResolverInitialization : IConfigurableModule
    {
        public static string EnvironmentName { get; private set; } = "notset";

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
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

            EnvironmentName = context.Services.EnvironmentName();

            if (context.Services.IsIntegrationEnvironment())
            {
                // could do something specific to integration here...
            }


#pragma warning disable CS0618 // Type or member is obsolete
            context.Services.AddSingleton<IContentTypeRepository<BlockType>, BlockTypeRepository>();
#pragma warning restore CS0618 // Type or member is obsolete
            context.Services.AddSingleton<EPiServer.Web.IDisplayChannelService, EPiServer.Web.Internal.DefaultDisplayChannelService>();

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
