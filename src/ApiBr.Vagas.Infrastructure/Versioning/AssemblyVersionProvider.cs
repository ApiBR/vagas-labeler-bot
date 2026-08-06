using System.Reflection;
using ApiBr.Vagas.Application.Ports;

namespace ApiBr.Vagas.Infrastructure.Versioning;

public sealed class AssemblyVersionProvider : IVersionProvider
{
    public string InformationalVersion { get; } =
        Assembly.GetEntryAssembly()
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
        ?? "0.0.0-unknown";
}
