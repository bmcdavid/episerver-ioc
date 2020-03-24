using Microsoft.Extensions.DependencyInjection;
using System;

namespace EpiserverIoc.Core
{
    public interface IServiceCollectionExtended : IServiceCollection
    {
        bool IsConfigured { get; }

        Action<ServiceCollectionChangedArgs> ServiceCollectionChanged { get; set; }
    }
}