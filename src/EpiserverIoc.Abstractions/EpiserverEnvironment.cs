using System;

namespace EpiserverIoc.Abstractions
{
    public class EpiserverEnvironment : IEpiserverEnvironment
    {
        public static Func<string> EnvironmentNameProvider { get; set; } = () => "Local";

        public EpiserverEnvironment(string environmentName)
        {
            Name = string.IsNullOrWhiteSpace(environmentName) ? "Local" : environmentName;

            if (IsProduction = IsEnvironment("Production")) { return; }
            if (IsPreproduction = IsEnvironment("Preproduction")) { return; }
            if (IsIntegration = IsEnvironment("Integration")) { return; }
            if (IsLocal = IsEnvironment("Local")) { return; }
            IsUnitTest = IsEnvironment("UnitTest");
        }

        public string Name { get; }

        public bool IsEnvironment(string name) => string.CompareOrdinal(Name, name) == 0;

        public bool IsIntegration { get; }

        public bool IsLocal { get; }

        public bool IsPreproduction { get; }

        public bool IsProduction { get; }

        public bool IsUnitTest { get; }
    }
}