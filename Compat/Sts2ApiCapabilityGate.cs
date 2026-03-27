using System.Reflection;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Saves;

namespace STS2RitsuLib.Compat
{
    /// <summary>
    ///     Chooses which STS2 API shape to assume: version thresholds when <see cref="Sts2HostVersion.Numeric" /> is
    ///     known, otherwise reflection on the loaded assembly.
    /// </summary>
    internal static class Sts2ApiCapabilityGate
    {
        internal static bool UseRunAndStateGameModeForEpochLogic()
        {
            var host = Sts2HostVersion.Numeric;
            var min = Sts2ApiFeatureThresholds.RunAndStateGameModeApiMinimum;
            if (host != null && min != null)
                return host >= min;

            return typeof(SerializableRun).GetProperty("GameMode", BindingFlags.Public | BindingFlags.Instance) != null;
        }

        internal static bool PreferModLoadStateEnumForLoadedDiscovery()
        {
            var host = Sts2HostVersion.Numeric;
            var min = Sts2ApiFeatureThresholds.ModLoadStateEnumApiMinimum;
            if (host != null && min != null)
                return host >= min;

            return typeof(Mod).GetProperty("state", BindingFlags.Public | BindingFlags.Instance) != null;
        }
    }
}
