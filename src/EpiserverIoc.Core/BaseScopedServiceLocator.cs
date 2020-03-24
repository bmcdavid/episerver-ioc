using EpiserverIoc.Abstractions;

namespace EpiserverIoc.Core
{
    public abstract class BaseScopedServiceLocator
    {
        protected void Dispose(bool disposing)
        {
            if (!disposing) { return; }

            AmbientServiceLocator.ClearScope();
        }

        protected IServiceLocatorScoped RegisterAndReturnScope(IServiceLocatorScoped serviceLocatorScoped)
        {
            AmbientServiceLocator.AddScope(serviceLocatorScoped);

            return serviceLocatorScoped;
        }
    }
}