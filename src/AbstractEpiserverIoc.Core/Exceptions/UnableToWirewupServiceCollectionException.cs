using System;

namespace AbstractEpiserverIoc.Core.Exceptions
{
    public class UnableToWirewupServiceCollectionException : Exception { }

    public class MissingTypeRegistrationException : InvalidOperationException
    {
        public MissingTypeRegistrationException(Type serviceType)
            : base($"Could not find any registered services for type '{serviceType.FullName}'.")
        {
            ServiceType = serviceType;
        }

        public Type ServiceType { get; }
    }
}