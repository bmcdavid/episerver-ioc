using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AbstractEpiserverIoc.Core.Tests.DryIoc")]
[assembly: InternalsVisibleTo("AbstractEpiserverIoc.Core.Tests.Grace")]
[assembly: InternalsVisibleTo("AbstractEpiserverIoc.Core.Tests.Stashbox")]

namespace AbstractEpiserverIoc.Core
{
    internal class ExtendedServiceCollection : ServiceCollection, IServiceCollectionExtended
    {
        public bool IsConfigured { get; internal set; }

        public Action<ServiceCollectionChangedArgs> ServiceCollectionChanged { get; set; }
    }
}