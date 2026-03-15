using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Saves;

namespace STS2RitsuLib.Utils.Persistence
{
    public class ProfileManager
    {
        private static ProfileManager? _instance;
        private bool _isInitialized;

        private ProfileManager()
        {
        }

        public static ProfileManager Instance => _instance ??= new();

        public int CurrentProfileId { get; private set; } = -1;

        public event Action<int, int>? ProfileChanged;

        public event Action<int>? ProfileDeleted;

        public void Initialize()
        {
            if (_isInitialized) return;

            CurrentProfileId = GetCurrentProfileIdFromGame();
            SaveManager.Instance.ProfileIdChanged += OnGameProfileChanged;

            _isInitialized = true;
            RitsuLibFramework.Logger.Info(
                $"[Persistence] ProfileManager initialized with profile ID: {CurrentProfileId}");
        }

        private void OnGameProfileChanged(int newProfileId)
        {
            OnProfileChanged(newProfileId);
        }

        public void OnProfileChanged(int newProfileId)
        {
            if (newProfileId == CurrentProfileId) return;

            var oldProfileId = CurrentProfileId;
            CurrentProfileId = newProfileId;

            if (oldProfileId >= 0)
                RitsuLibFramework.Logger.Info($"[Persistence] Profile changed from {oldProfileId} to {newProfileId}");
            ProfileChanged?.Invoke(oldProfileId, newProfileId);
        }

        public void RefreshCurrentProfile()
        {
            var newProfileId = GetCurrentProfileIdFromGame();
            if (newProfileId != CurrentProfileId)
                OnProfileChanged(newProfileId);
        }

        public static string GetAccountBasePath(string modId = Const.ModId)
        {
            var platformDir = GetPlatformDirectory();
            var userId = GetUserId();
            return $"user://{platformDir}/{userId}/mod_data/{modId}";
        }

        public string GetProfileDirectory()
        {
            return GetProfileDirectory(CurrentProfileId);
        }

        public static string GetProfileDirectory(int profileId)
        {
            return UserDataPathProvider.GetProfileDir(profileId);
        }

        public string GetBasePath(SaveScope scope)
        {
            return GetBasePath(scope, CurrentProfileId);
        }

        public static string GetBasePath(SaveScope scope, int profileId, string modId = Const.ModId)
        {
            var accountBase = GetAccountBasePath(modId);
            return scope switch
            {
                SaveScope.Global => accountBase,
                SaveScope.Profile => $"{accountBase}/{GetProfileDirectory(profileId)}",
                _ => accountBase,
            };
        }

        public string GetFilePath(string fileName, SaveScope scope)
        {
            return GetFilePath(fileName, scope, CurrentProfileId, Const.ModId);
        }

        public static string GetFilePath(string fileName, SaveScope scope, int profileId)
        {
            return GetFilePath(fileName, scope, profileId, Const.ModId);
        }

        public string GetFilePath(string fileName, SaveScope scope, string modId)
        {
            return GetFilePath(fileName, scope, CurrentProfileId, modId);
        }

        public static string GetFilePath(string fileName, SaveScope scope, int profileId, string modId)
        {
            return $"{GetBasePath(scope, profileId, modId)}/{fileName}";
        }

        public static void DeleteProfileData(int profileId, string modId = Const.ModId)
        {
            var profilePath = GetBasePath(SaveScope.Profile, profileId, modId);
            RitsuLibFramework.Logger.Info($"[Persistence] Deleting mod data for profile {profileId} at: {profilePath}");

            try
            {
                FileOperations.DeleteDirectoryRecursive(profilePath);
                RitsuLibFramework.Logger.Info($"[Persistence] Successfully deleted mod data for profile {profileId}");
            }
            catch (Exception ex)
            {
                RitsuLibFramework.Logger.Error(
                    $"[Persistence] Failed to delete mod data for profile {profileId}: {ex.Message}");
            }
        }

        internal void OnProfileDeleted(int profileId)
        {
            ProfileDeleted?.Invoke(profileId);
        }

        private static int GetCurrentProfileIdFromGame()
        {
            try
            {
                return SaveManager.Instance.CurrentProfileId;
            }
            catch
            {
                return 1;
            }
        }

        private static string GetPlatformDirectory()
        {
            try
            {
                var platform = PlatformUtil.PrimaryPlatform;
                return UserDataPathProvider.GetPlatformDirectoryName(platform);
            }
            catch
            {
                return "default";
            }
        }

        private static string GetUserId()
        {
            try
            {
                var platform = PlatformUtil.PrimaryPlatform;
                return PlatformUtil.GetLocalPlayerId(platform).ToString();
            }
            catch
            {
                return "0";
            }
        }
    }
}
