namespace EpiserverIoc.Abstractions
{
    public interface IEpiserverEnvironment
    {
        string Name { get; }

        bool IsProduction { get; }

        bool IsPreproduction { get; }

        bool IsIntegration { get; }

        bool IsLocal { get; }

        bool IsUnitTest { get; }

        bool IsEnvironment(string name);
    }
}