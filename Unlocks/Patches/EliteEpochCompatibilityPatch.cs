using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Saves.Managers;
using STS2RitsuLib.Content;
using STS2RitsuLib.Patching.Models;

namespace STS2RitsuLib.Unlocks.Patches
{
    public class EliteEpochCompatibilityPatch : IPatchMethod
    {
        public static string PatchId => "elite_epoch_compatibility";

        public static string Description =>
            "Handle elite-win epoch unlock checks for mod characters via registered RitsuLib unlock rules";

        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(ProgressSaveManager), "CheckFifteenElitesDefeatedEpoch",
                    [typeof(Player)], true),
            ];
        }

        // ReSharper disable once InconsistentNaming
        public static bool Prefix(ProgressSaveManager __instance, Player localPlayer)
        {
            ArgumentNullException.ThrowIfNull(__instance);
            ArgumentNullException.ThrowIfNull(localPlayer);

            if (!ModContentRegistry.TryGetOwnerModId(localPlayer.Character.GetType(), out _))
                return true;

            EliteEpochModHandling.TryHandleModEliteEpoch(__instance, localPlayer);
            return false;
        }
    }
}
