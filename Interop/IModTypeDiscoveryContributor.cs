using System.Reflection;
using HarmonyLib;

namespace STS2RitsuLib.Interop
{
    /// <summary>
    ///     Runs once per mod-defined CLR type after all mods are loaded (see <see cref="ModTypeDiscoveryHub" />).
    ///     Used for cross-mod interop code generation and similar post-load reflection passes.
    /// </summary>
    public interface IModTypeDiscoveryContributor
    {
        void Contribute(Harmony harmony, IReadOnlyDictionary<string, Assembly> modAssembliesByManifestId, Type modType);
    }
}
