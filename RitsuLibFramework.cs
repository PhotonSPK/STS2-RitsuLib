using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using STS2RitsuLib.Data;
using STS2RitsuLib.Patching.Core;
using STS2RitsuLib.Utils;
using STS2RitsuLib.Utils.Persistence;
using STS2RitsuLib.Utils.Persistence.Patches;

namespace STS2RitsuLib
{
    /// <summary>
    ///     Shared runtime bootstrap for the framework itself and for mods that reference it.
    /// </summary>
    [ModInitializer(nameof(Initialize))]
    public static class RitsuLibFramework
    {
        private static readonly Lock SyncRoot = new();
        private static ModPatcher? _frameworkPatcher;
        private static bool _profileServicesInitialized;
        private static readonly List<ILifecycleObserver> LifecycleObservers = [];
        private static FrameworkInitializedEvent? _lastFrameworkInitializedEvent;
        private static ProfileServicesInitializedEvent? _lastProfileServicesInitializedEvent;
        private static ProfileDataReadyEvent? _lastProfileDataReadyEvent;

        static RitsuLibFramework()
        {
            Logger = CreateLogger(Const.ModId);
        }

        public static Logger Logger { get; private set; }
        public static bool IsInitialized { get; private set; }
        public static bool IsActive { get; private set; }

        public static IDisposable SubscribeLifecycle(ILifecycleObserver observer, bool replayCurrentState = true)
        {
            ArgumentNullException.ThrowIfNull(observer);

            FrameworkInitializedEvent? initializedSnapshot;
            ProfileServicesInitializedEvent? profileSnapshot;
            ProfileDataReadyEvent? dataReadySnapshot;

            lock (SyncRoot)
            {
                LifecycleObservers.Add(observer);
                initializedSnapshot = replayCurrentState ? _lastFrameworkInitializedEvent : null;
                profileSnapshot = replayCurrentState ? _lastProfileServicesInitializedEvent : null;
                dataReadySnapshot = replayCurrentState ? _lastProfileDataReadyEvent : null;
            }

            if (initializedSnapshot.HasValue)
                SafeNotify(observer, o => o.OnEvent(initializedSnapshot.Value), nameof(FrameworkInitializedEvent));

            if (profileSnapshot.HasValue)
                SafeNotify(observer, o => o.OnEvent(profileSnapshot.Value), nameof(ProfileServicesInitializedEvent));

            if (dataReadySnapshot.HasValue)
                SafeNotify(observer, o => o.OnEvent(dataReadySnapshot.Value), nameof(ProfileDataReadyEvent));

            return new FrameworkLifecycleSubscription(() =>
            {
                lock (SyncRoot)
                {
                    LifecycleObservers.Remove(observer);
                }
            });
        }

        public static void Initialize()
        {
            lock (SyncRoot)
            {
                if (IsInitialized)
                {
                    Logger.Debug("Framework already initialized, skipping duplicate initialization.");
                    return;
                }

                Logger = CreateLogger(Const.ModId);

                Logger.Info($"Framework ID: {Const.ModId}");
                Logger.Info($"Framework Name: {Const.Name}");
                Logger.Info($"Version: {Const.Version}");
                Logger.Info("Initializing shared framework...");
                PublishLifecycleEvent(
                    new FrameworkInitializingEvent(Const.ModId, Const.Version, DateTimeOffset.UtcNow),
                    nameof(FrameworkInitializingEvent)
                );

                try
                {
                    _frameworkPatcher = CreatePatcher(Const.ModId, "framework", "framework");

                    _frameworkPatcher.RegisterPatch<ProfilePathInitializedPatch>();
                    _frameworkPatcher.RegisterPatch<ProfileDeletePatch>();

                    if (!_frameworkPatcher.PatchAll())
                    {
                        Logger.Error("Framework initialization failed: critical framework patches failed.");
                        IsActive = false;
                        return;
                    }

                    IsInitialized = true;
                    IsActive = true;

                    var frameworkInitializedEvent = new FrameworkInitializedEvent(
                        Const.ModId,
                        IsActive,
                        DateTimeOffset.UtcNow
                    );

                    _lastFrameworkInitializedEvent = frameworkInitializedEvent;
                    PublishLifecycleEvent(frameworkInitializedEvent, nameof(FrameworkInitializedEvent));

                    Logger.Info("Shared framework initialization complete.");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Framework initialization failed: {ex.Message}");
                    Logger.Error($"Stack trace: {ex.StackTrace}");
                    IsActive = false;
                }
            }
        }

        public static void EnsureProfileServicesInitialized()
        {
            lock (SyncRoot)
            {
                if (_profileServicesInitialized)
                    return;

                PublishLifecycleEvent(
                    new ProfileServicesInitializingEvent(DateTimeOffset.UtcNow),
                    nameof(ProfileServicesInitializingEvent)
                );

                ProfileManager.Instance.Initialize();
                ModDataStore.InitializeAllProfileScoped();

                _profileServicesInitialized = true;

                var profileInitializedEvent = new ProfileServicesInitializedEvent(
                    ProfileManager.Instance.CurrentProfileId,
                    DateTimeOffset.UtcNow
                );

                _lastProfileServicesInitializedEvent = profileInitializedEvent;
                PublishLifecycleEvent(profileInitializedEvent, nameof(ProfileServicesInitializedEvent));

                Logger.Debug("Profile-scoped framework services initialized.");
            }
        }

        public static IDisposable BeginModDataRegistration(string modId, bool initializeProfileIfReady = true)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(modId);
            return ModDataStore.For(modId).BeginRegistrationScope(initializeProfileIfReady);
        }

        public static ModDataStore GetDataStore(string modId)
        {
            return ModDataStore.For(modId);
        }

        public static Logger CreateLogger(string modId, LogType logType = LogType.Generic)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(modId);
            return new(modId, logType);
        }

        public static ModPatcher CreatePatcher(
            string ownerModId,
            string patcherName,
            string? patcherLabel = null,
            LogType logType = LogType.Generic)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(ownerModId);
            ArgumentException.ThrowIfNullOrWhiteSpace(patcherName);

            var logger = CreateLogger(ownerModId, logType);

            return new(
                $"{ownerModId}.{patcherName}",
                logger,
                patcherLabel ?? patcherName
            );
        }

        public static I18N CreateLocalization(
            string instanceName,
            IEnumerable<string>? fileSystemFolders = null,
            IEnumerable<string>? resourceFolders = null,
            IEnumerable<string>? pckFolders = null,
            Assembly? resourceAssembly = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(instanceName);

            return new(
                instanceName,
                fileSystemFolders?.ToArray() ?? [],
                resourceFolders?.ToArray() ?? [],
                pckFolders?.ToArray() ?? [],
                resourceAssembly ?? Assembly.GetCallingAssembly()
            );
        }

        public static I18N CreateModLocalization(
            string modId,
            string instanceName,
            IEnumerable<string>? fileSystemFolders = null,
            IEnumerable<string>? resourceFolders = null,
            IEnumerable<string>? pckFolders = null,
            Assembly? resourceAssembly = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(modId);
            ArgumentException.ThrowIfNullOrWhiteSpace(instanceName);

            var folders = fileSystemFolders?.ToArray() ?? [$"user://mod-configs/{modId}/localization"];
            return CreateLocalization(instanceName, folders, resourceFolders, pckFolders, resourceAssembly);
        }

        public static bool ApplyRequiredPatcher(ModPatcher patcher, Action disableMod, string? failureMessage = null)
        {
            ArgumentNullException.ThrowIfNull(patcher);
            ArgumentNullException.ThrowIfNull(disableMod);

            var success = patcher.PatchAll();
            if (success)
                return true;

            patcher.Logger.Error(
                failureMessage ?? $"Required patcher '{patcher.PatcherName}' failed. The mod will be disabled.");
            disableMod();
            return false;
        }

        internal static void PublishLifecycleEvent(IFrameworkLifecycleEvent evt, string phase)
        {
            _lastProfileDataReadyEvent = evt switch
            {
                ProfileDataReadyEvent dataReadyEvent => dataReadyEvent,
                ProfileDataInvalidatedEvent => null,
                _ => _lastProfileDataReadyEvent,
            };

            NotifyObservers(o => o.OnEvent(evt), phase);
        }

        private static void NotifyObservers(Action<ILifecycleObserver> notify, string phase)
        {
            ILifecycleObserver[] snapshot;

            lock (SyncRoot)
            {
                snapshot = LifecycleObservers.ToArray();
            }

            foreach (var observer in snapshot)
                SafeNotify(observer, notify, phase);
        }

        private static void SafeNotify(ILifecycleObserver observer, Action<ILifecycleObserver> notify, string phase)
        {
            try
            {
                notify(observer);
            }
            catch (Exception ex)
            {
                Logger.Warn($"[Lifecycle] Observer callback failed in {phase}: {ex.Message}");
            }
        }
    }
}
