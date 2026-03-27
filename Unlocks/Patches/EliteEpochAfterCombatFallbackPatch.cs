using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Managers;
using STS2RitsuLib.Content;
using STS2RitsuLib.Patching.Models;

namespace STS2RitsuLib.Unlocks.Patches
{
    /// <summary>
    ///     When <c>CheckFifteenElitesDefeatedEpoch</c> is absent, elite logic may live only inside
    ///     <see cref="ProgressSaveManager.UpdateAfterCombatWon" />. Postfix covers the non-throwing case; Finalizer
    ///     recovers from vanilla <see cref="ArgumentOutOfRangeException" /> for unknown mod characters.
    /// </summary>
    public class EliteEpochAfterCombatFallbackPatch : IPatchMethod
    {
        public static string PatchId => "elite_epoch_after_combat_fallback";

        public static string Description =>
            "Elite epoch unlock fallback when CheckFifteenElitesDefeatedEpoch is missing (stable vs beta)";

        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(ProgressSaveManager), nameof(ProgressSaveManager.UpdateAfterCombatWon),
                    [typeof(Player), typeof(CombatRoom)]),
            ];
        }

        // ReSharper disable once InconsistentNaming
        public static void Postfix(ProgressSaveManager __instance, Player localPlayer, CombatRoom room)
        {
            if (EliteEpochModHandling.HasDedicatedEliteEpochCheckMethod)
                return;

            if (room.RoomType != RoomType.Elite)
                return;

            if (!ModContentRegistry.TryGetOwnerModId(localPlayer.Character.GetType(), out _))
                return;

            EliteEpochModHandling.TryHandleModEliteEpoch(__instance, localPlayer);
        }

        // ReSharper disable InconsistentNaming
        public static Exception? Finalizer(
                Exception? __exception,
                ProgressSaveManager __instance,
                Player localPlayer,
                CombatRoom room)
            // ReSharper restore InconsistentNaming
        {
            if (__exception == null)
                return null;

            if (EliteEpochModHandling.HasDedicatedEliteEpochCheckMethod || room.RoomType != RoomType.Elite ||
                !ModContentRegistry.TryGetOwnerModId(localPlayer.Character.GetType(), out _) ||
                __exception is not ArgumentOutOfRangeException aex || aex.ParamName != "character")
                return __exception;

            EliteEpochModHandling.TryHandleModEliteEpoch(__instance, localPlayer);
            return null;
        }
    }
}
