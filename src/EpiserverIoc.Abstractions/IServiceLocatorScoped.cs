using EPiServer.ServiceLocation;
using System;

namespace EpiserverIoc.Abstractions
{
    public interface IServiceLocatorScoped : IServiceLocator, IServiceLocatorCreateScope, IDisposable
    {
        /// <summary>
        /// Never dispose this, it should be handled elsewhere
        /// </summary>
        IServiceLocatorScoped ParentScope { get; }
    }
}