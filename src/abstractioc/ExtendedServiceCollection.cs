using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AbstractEpiserverIoc.Core.Tests")]

namespace AbstractEpiserverIoc.Core
{
    internal class ExtendedServiceCollection : ServiceCollection, IServiceCollectionExtended
    {
        public bool IsConfigured { get; set; }

        public Action<ServiceCollectionChangedArgs> ServiceCollectionChanged { get; set; }
    }
}