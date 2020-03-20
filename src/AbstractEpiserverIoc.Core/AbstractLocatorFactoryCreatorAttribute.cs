using System;

namespace AbstractEpiserverIoc.Core
{   // Add assembly attr to disable exception of registering after configuration complete
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class AbstractLocatorFactoryCreatorAttribute : Attribute
    {
        public Type CreatorType { get; }

        /// <summary>
        /// Must be static method with no args that returns an IServiceLocator that implements IServiceLocatorWireupCollection.
        /// </summary>
        public string MethodName { get; set; }

        public AbstractLocatorFactoryCreatorAttribute(Type creatorType, string methodName)
        {
            CreatorType = creatorType ?? throw new ArgumentNullException(nameof(creatorType));
            MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
        }
    }
}