using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace STS2RitsuLib.Compat
{
    /// <summary>
    ///     Epoch-related game-mode checks: uses <c>GameMode</c> on save/state when the capability gate says so, else
    ///     host-era heuristics (daily / modifiers).
    /// </summary>
    internal static class Sts2RunGameModeCompat
    {
        internal static bool IsStandardSerializableRunForEpochUnlocks(SerializableRun run)
        {
            if (Sts2ApiCapabilityGate.UseRunAndStateGameModeForEpochLogic())
            {
                var p = typeof(SerializableRun).GetProperty("GameMode", BindingFlags.Public | BindingFlags.Instance);
                if (p != null)
                    return GameMode.Standard.Equals(p.GetValue(run));
            }

            if (run.DailyTime.HasValue)
                return false;

            return run.Modifiers.Count <= 0;
        }

        internal static bool AreMidRunEpochsLockedFor(Player localPlayer)
        {
            ArgumentNullException.ThrowIfNull(localPlayer);
            var runState = localPlayer.RunState;

            if (!Sts2ApiCapabilityGate.UseRunAndStateGameModeForEpochLogic()) return runState.Modifiers.Count > 0;
            var gmProp = runState.GetType().GetProperty("GameMode", BindingFlags.Public | BindingFlags.Instance);
            if (gmProp == null) return runState.Modifiers.Count > 0;
            var value = gmProp.GetValue(runState);
            return value != null && !GameMode.Standard.Equals(value);
        }
    }
}
