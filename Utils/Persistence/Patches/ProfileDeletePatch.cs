using MegaCrit.Sts2.Core.Saves;
using STS2RitsuLib.Data;
using STS2RitsuLib.Patching.Models;

namespace STS2RitsuLib.Utils.Persistence.Patches
{
    public class ProfileDeletePatch : IPatchMethod
    {
        public static string PatchId => "profile_delete";
        public static string Description => "Delete mod data when game profile is deleted";
        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(SaveManager), "DeleteProfile", [typeof(int)])];
        }

        public static void Prefix(int profileId)
        {
            try
            {
                ModDataStore.DeleteAllProfileData(profileId);
                ProfileManager.Instance.OnProfileDeleted(profileId);
                DataReadyLifecycle.NotifyProfileInvalidated(profileId, "SaveManager.DeleteProfile");
            }
            catch (Exception ex)
            {
                RitsuLibFramework.Logger.Warn(
                    $"[Persistence] Failed to delete mod data for profile {profileId}: {ex.Message}");
            }
        }
    }
}
