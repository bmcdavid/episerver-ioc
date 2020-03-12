namespace AbstractEpiserverIoc.Core
{
    public interface IServiceLocatorCreateScope
    {
        IServiceLocatorScoped CreateScope();
    }
}