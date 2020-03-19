using AbstractEpiserverIoc.Abstractions;
using System.Web;

namespace AbstractEpiserverIoc.Web
{
    public static class HttpContextExtensions
    {
        public static IServiceLocatorScoped GetServiceLocatorScoped(this HttpContextBase httpContextBase)
        {
            return httpContextBase?.Items[HttpScopeCreatorModule._itemKey] as IServiceLocatorScoped;
        }

        public static IServiceLocatorScoped GetServiceLocatorScoped(this HttpContext httpContext)
        {
            return httpContext?.Items[HttpScopeCreatorModule._itemKey] as IServiceLocatorScoped;
        }
    }
}