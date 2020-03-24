using System;
using System.Collections;

namespace EpiserverIoc.Abstractions
{
    /// <summary>
    /// ASP.NET cannot gurantee AsyncLocal in the pipeline this is a mechanism to bypass this
    /// </summary>
    public interface IServiceLocatorAmbientPreferredStorage
    {
        void SetStorage(Func<IDictionary> contextStorage);
    }
}