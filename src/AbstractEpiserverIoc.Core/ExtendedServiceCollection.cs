using Microsoft.Extensions.DependencyInjection;
using System;

namespace AbstractEpiserverIoc.Core
{
    public class ExtendedServiceCollection : ServiceCollection, IServiceCollectionExtended
    {
        public bool IsConfigured { get; internal set; }

        public Action<ServiceCollectionChangedArgs> ServiceCollectionChanged { get; set; }
    }
}