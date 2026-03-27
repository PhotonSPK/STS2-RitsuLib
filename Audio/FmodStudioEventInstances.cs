using Godot;
using STS2RitsuLib.Audio.Internal;

namespace STS2RitsuLib.Audio
{
    /// <summary>Long-lived Studio event instances (manual start/stop/release).</summary>
    public static class FmodStudioEventInstances
    {
        public static GodotObject? TryCreate(string eventOrSnapshotPath)
        {
            return !FmodStudioGateway.TryCall(out var v, FmodStudioMethodNames.CreateEventInstance, eventOrSnapshotPath)
                ? null
                : v.AsGodotObject();
        }

        public static bool TryStart(GodotObject? instance)
        {
            if (instance is null)
                return false;

            try
            {
                instance.Call("start");
                return true;
            }
            catch (Exception ex)
            {
                RitsuLibFramework.Logger.Error($"[Audio] FMOD event start: {ex.Message}");
                return false;
            }
        }

        public static bool TryStop(GodotObject? instance, bool allowFadeOut = true)
        {
            if (instance is null)
                return false;

            try
            {
                instance.Call("stop", allowFadeOut ? 0 : 1);
                return true;
            }
            catch (Exception ex)
            {
                RitsuLibFramework.Logger.Error($"[Audio] FMOD event stop: {ex.Message}");
                return false;
            }
        }

        public static void TryRelease(GodotObject? instance)
        {
            if (instance is null)
                return;

            try
            {
                instance.Call("release");
            }
            catch (Exception ex)
            {
                RitsuLibFramework.Logger.Error($"[Audio] FMOD event release: {ex.Message}");
            }
        }
    }
}
