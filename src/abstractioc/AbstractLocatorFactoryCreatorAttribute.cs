using System;

namespace AbstractEpiserverIoc.Core
{
    public delegate EPiServer.ServiceLocation.IServiceLocator BuildServiceLocator(IServiceCollectionExtended serviceCollection);

    // Add assembly attr to disable exception of registering after configuration complete

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class AbstractLocatorFactoryCreatorAttribute : Attribute
    {
        public Type CreatorType { get; }

        /// <summary>
        /// Must be static and return an IServiceLocator given an IServiceCollection
        /// </summary>
        /// <example>
        /// public static IServiceLocator CreateServiceLocator(IServiceCollectionExtended services){}
        /// </example>
        public string MethodName { get; set; }

        public AbstractLocatorFactoryCreatorAttribute(Type creatorType, string methodName)
        {
            CreatorType = creatorType ?? throw new ArgumentNullException(nameof(creatorType));
            MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
        }
    }
}