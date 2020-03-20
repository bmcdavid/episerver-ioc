using System;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace AbstractEpiserverIoc.Core
{
    public class ServiceCollectionChangedArgs : EventArgs
    {
        public MicrosoftServiceDescriptor ServiceDescriptor { get; }
        public bool IsRemove { get; }

        public ServiceCollectionChangedArgs(MicrosoftServiceDescriptor service, bool isRemove = false)
        {
            ServiceDescriptor = service;
            IsRemove = isRemove;
        }
    }
}