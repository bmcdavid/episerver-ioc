using Microsoft.Extensions.DependencyInjection;
using System;

namespace AbstractEpiserverIoc.Core
{
    public interface IServiceCollectionExtended : IServiceCollection
    {
        bool IsConfigured { get; }

        Action<ServiceCollectionChangedArgs> ServiceCollectionChanged { get; set; }
    }
}