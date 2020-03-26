# Episerver IOC

The goal of this project is to provide alternatives to Structuremap which was retired in 2018. There are many other dependency injection containers that offer much better performance as noted by [Daniel Palme](https://github.com/danielpalme/IocPerformance). The current status of containers are noted below:

* [DryIoc](https://github.com/dadhi/DryIoc)
* [Grace](https://github.com/ipjohnson/Grace)
* [Microsoft.Extensions.DependencyInjection](https://github.com/dotnet/extensions/tree/master/src/DependencyInjection)

## Sample Episerver Alloy site

A sample Alloy site is included in the tests folder. The main files of note in the project are **Business\Initialization\DependencyResolverInitialization.cs** and **web.config**. The relevant portion of the web.config is noted below where assembly scanning is fine tuned.

```xml
<episerver.framework updateDatabaseSchema="true" createDatabaseSchema="true">    
    <scanAssembly forceBinFolderScan="false">
      <add assembly="EPiServer" />
      <add assembly="EPiServer.ApplicationModules" />
      <add assembly="EPiServer.Cms.AspNet" />
      <add assembly="EPiServer.Cms.Shell.UI" />
      <add assembly="EPiServer.Cms.TinyMce" />
      <add assembly="EPiServer.Cms.UI.AspNetIdentity" />
      <add assembly="EPiServer.Data" />
      <add assembly="EPiServer.Enterprise" />
      <add assembly="EPiServer.Events" />
      <add assembly="EPiServer.Forms" />
      <add assembly="EPiServer.Forms.UI" />
      <add assembly="EPiServer.Framework" />
      <add assembly="EPiServer.Framework.AspNet" />
      <add assembly="EPiServer.LinkAnalyzer" />
      <add assembly="EPiServer.Shell" />
      <add assembly="EPiServer.Shell.UI" />
      <add assembly="EPiServer.UI" />
      <add assembly="EPiServerSite1" />
      <add assembly="EPiServer.DeveloperTools" />
      <add assembly="N1990.Episerver.Cms.Audit" />
      <add assembly="EpiserverIoc.AspNet" />

      <!-- The below will dictate which IOC is used, only 1 should be uncommented at a time -->

      <!--<add assembly="EPiServer.ServiceLocation.StructureMap" />-->
      <add assembly="EpiserverIoc.Core" /> <!-- actual DI container depends on project reference -->
      <!--<add assembly="EpiserverIoc.Locators.DryIocPure"/>-->
    </scanAssembly>    
</episerver.framework>
```

## Installation

These projects will not be created as NuGet packages since they rely on internal Episerver namespaces. Anyone wishing to use them may freely add them to their Visual Studio solutions. There components are as noted

* EpiserverIoc.Abstractions - baseline abstractions for IOC functionality
* EpiserverIoc.Core - provides ambient service locator, centralized service registration using Microsoft.Extensions.DependencyInjection.Abstractions, Episerver Environment and method for creating different locators for a given IServiceCollection.
* EpiserverIoc.AspNet - provides scoping for ASP.Net Framework HTTP requests and setup of environment based upon DXP application setting "episerver:EnvironmentName".

### Environment based registrations

The abstraction project provides an IEpiserverEnvironment interface which is available on the IServiceConfigurationProvider provided during Episerver configuration event. Below is a code example in an IConfigurableModule:

```cs
// using EpiserverIoc.Abstractions;
public void ConfigureContainer(ServiceConfigurationContext context)
{    
    context.Services.EnvironmentName();

    // Example of enviornment specific configuration, for example change logging
    if (context.Services.IsIntegrationEnvironment())
    {
        // could do something specific to integration here...
    }
}

```

## Commonly Needed Registrations

Grace Registration Dependency Issues Resolved
```cs
context.Services.ResolveGraceDependencyIssues();
```

```cs
context.Services.AddSingleton<IContentTypeRepository<BlockType>, BlockTypeRepository>();

context.Services.AddSingleton<IUrlResolver, EPiServer.Web.Routing.Internal.DefaultUrlResolver>();

context.Services.AddSingleton<IServiceProvider>(locator => locator);
```

```cs
context.Services.AddSingleton<IServiceScopeFactory>((locator) => new CustomServiceScopeFactory(locator));

internal class CustomServiceScopeFactory : IServiceScopeFactory
{
    private readonly IServiceLocatorCreateScope _serviceLocator;

    public CustomServiceScopeFactory(IServiceLocator serviceLocator)
    {
        _serviceLocator = serviceLocator as IServiceLocatorCreateScope;
    }

    public IServiceScope CreateScope() => new ServiceScope(_serviceLocator.CreateScope());

    private class ServiceScope : IServiceScope
    {
        private readonly IServiceLocator _scopedLocator;

        public ServiceScope(IServiceLocator scopedLocator)
        {
            _scopedLocator = scopedLocator;
        }

        public IServiceProvider ServiceProvider => _scopedLocator;

        public void Dispose()
        {
            (_scopedLocator as IDisposable).Dispose();
        }
    }
}
```
