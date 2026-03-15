using MegaCrit.Sts2.Core.Saves;
using STS2RitsuLib.Patching.Models;

namespace STS2RitsuLib.Utils.Persistence.Patches
{
    /// <summary>
    ///     Framework trigger point for safe data operations.
    ///     Fires after SaveManager has initialized/switched profile path, then forwards to DataReadyLifecycle.
    /// </summary>
    public class ProfilePathInitializedPatch : IPatchMethod
    {
        public static string PatchId => "profile_path_initialized";
        public static string Description => "Notify safe data-ready lifecycle after profile path initialization";
        public static bool IsCritical => true;

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(SaveManager), "InitProfileId", [typeof(int?)]),
                new(typeof(SaveManager), "SwitchProfileId", [typeof(int)]),
            ];
        }

        public static void Postfix()
        {
            try
            {
                DataReadyLifecycle.NotifyPotentialReady("SaveManager.InitProfileId/SwitchProfileId");
            }
            catch (Exception ex)
            {
                RitsuLibFramework.Logger.Warn(
                    $"[Persistence] Failed to process profile path initialized hook: {ex.Message}");
            }
        }
    }
}
