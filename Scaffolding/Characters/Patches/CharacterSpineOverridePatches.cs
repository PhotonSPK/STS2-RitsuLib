using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Compat;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Utils;

namespace STS2RitsuLib.Scaffolding.Characters.Patches
{
    public class CharacterCombatSpineOverridePatch : IPatchMethod
    {
        public static string PatchId => "character_combat_spine_override";

        public static string Description =>
            "Allow mod characters to replace combat Spine skeleton data while reusing existing visuals scenes";

        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(NCreature), nameof(NCreature._Ready))];
        }

        // ReSharper disable InconsistentNaming
        public static void Postfix(NCreature __instance)
            // ReSharper restore InconsistentNaming
        {
            var player = __instance.Entity?.Player;
            if (player?.Character is not IModCharacterAssetOverrides overrides)
                return;

            var skeletonPath = overrides.CustomCombatSpineSkeletonDataPath;
            if (string.IsNullOrWhiteSpace(skeletonPath))
                return;

            if (!AssetPathDiagnostics.Exists(skeletonPath, player.Character,
                    nameof(IModCharacterAssetOverrides.CustomCombatSpineSkeletonDataPath)))
                return;

            var visuals = __instance.Visuals;
            if (visuals == null || !visuals.HasSpineAnimation ||
                !NCreatureVisualsSpineCompat.HasSpineTargetForOverride(visuals))
                return;

            try
            {
                var skeletonData = ResourceLoader.Load<Resource>(skeletonPath);
                if (skeletonData == null)
                {
                    RitsuLibFramework.Logger.Warn($"[Visuals] Failed to load combat spine data: {skeletonPath}");
                    return;
                }

                if (!NCreatureVisualsSpineCompat.TryApplyCombatSkeletonOverride(visuals, skeletonData))
                    RitsuLibFramework.Logger.Warn(
                        $"[Visuals] Could not apply combat spine override (no Body/SpineBody target): {skeletonPath}");
            }
            catch (Exception ex)
            {
                RitsuLibFramework.Logger.Error(
                    $"[Visuals] Failed to apply combat spine override '{skeletonPath}': {ex.Message}");
            }
        }
    }
}
