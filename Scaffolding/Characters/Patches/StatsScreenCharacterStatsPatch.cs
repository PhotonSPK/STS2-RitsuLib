using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.StatsScreen;
using MegaCrit.Sts2.Core.Saves;
using STS2RitsuLib.Content;
using STS2RitsuLib.Patching.Models;

namespace STS2RitsuLib.Scaffolding.Characters.Patches
{
    /// <summary>
    ///     Appends mod character history sections to the general stats screen.
    ///     Base game NGeneralStatsGrid.LoadStats hard-codes five vanilla characters,
    ///     so mod character records never render without this patch.
    /// </summary>
    public class StatsScreenCharacterStatsPatch : IPatchMethod
    {
        public static string PatchId => "stats_screen_mod_character_sections";

        public static string Description =>
            "Append registered mod characters to NGeneralStatsGrid character history sections";

        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(NGeneralStatsGrid), nameof(NGeneralStatsGrid.LoadStats))];
        }

        // ReSharper disable InconsistentNaming
        public static void Postfix(NGeneralStatsGrid __instance)
            // ReSharper restore InconsistentNaming
        {
            var progressSave = SaveManager.Instance.Progress;
            var createCharacterSection = AccessTools.Method(typeof(NGeneralStatsGrid), "CreateCharacterSection");
            if (createCharacterSection == null)
                return;

            foreach (var character in ModContentRegistry.GetModCharacters())
                createCharacterSection.Invoke(__instance, [progressSave, character.Id]);
        }
    }
}
