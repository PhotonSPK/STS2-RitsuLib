using System.Collections.Concurrent;
using Godot;
using STS2RitsuLib.Audio.Internal;

namespace STS2RitsuLib.Audio
{
    /// <summary>
    ///     Load loose audio files into the FMOD runtime (wav/ogg/mp3 per addon). Tracks loaded paths so you can unload
    ///     deterministically.
    /// </summary>
    public static class FmodStudioStreamingFiles
    {
        private static readonly ConcurrentDictionary<string, LoadedKind> Loaded = new(StringComparer.Ordinal);

        public static bool TryPreloadAsSound(string absoluteOrResPath)
        {
            if (Loaded.ContainsKey(absoluteOrResPath))
                return true;

            if (!FmodStudioGateway.TryCall(FmodStudioMethodNames.LoadFileAsSound, absoluteOrResPath))
                return false;

            Loaded[absoluteOrResPath] = LoadedKind.Sound;
            return true;
        }

        public static bool TryPreloadAsStreamingMusic(string absoluteOrResPath)
        {
            if (Loaded.ContainsKey(absoluteOrResPath))
                return true;

            if (!FmodStudioGateway.TryCall(FmodStudioMethodNames.LoadFileAsMusic, absoluteOrResPath))
                return false;

            Loaded[absoluteOrResPath] = LoadedKind.MusicStream;
            return true;
        }

        public static GodotObject? TryCreateSoundInstance(string absoluteOrResPath)
        {
            if (Loaded.ContainsKey(absoluteOrResPath))
                return !FmodStudioGateway.TryCall(out var record, FmodStudioMethodNames.CreateSoundInstance,
                    absoluteOrResPath)
                    ? null
                    : record.AsGodotObject();
            if (!TryPreloadAsSound(absoluteOrResPath))
                return null;

            return !FmodStudioGateway.TryCall(out var v, FmodStudioMethodNames.CreateSoundInstance, absoluteOrResPath)
                ? null
                : v.AsGodotObject();
        }

        public static GodotObject? TryCreateStreamingMusicInstance(string absoluteOrResPath)
        {
            if (Loaded.ContainsKey(absoluteOrResPath))
                return !FmodStudioGateway.TryCall(out var record, FmodStudioMethodNames.CreateSoundInstance,
                    absoluteOrResPath)
                    ? null
                    : record.AsGodotObject();
            if (!TryPreloadAsStreamingMusic(absoluteOrResPath))
                return null;

            return !FmodStudioGateway.TryCall(out var v, FmodStudioMethodNames.CreateSoundInstance, absoluteOrResPath)
                ? null
                : v.AsGodotObject();
        }

        public static bool TryPlaySoundFile(string absoluteOrResPath, float volume = 1f, float pitch = 1f)
        {
            var sound = TryCreateSoundInstance(absoluteOrResPath);
            if (sound is null)
                return false;

            try
            {
                if (Mathf.IsEqualApprox(volume, 1f))
                    sound.Call("set_volume", volume);

                if (Mathf.IsEqualApprox(pitch, 1f))
                    sound.Call("set_pitch", pitch);

                sound.Call("play");
                return true;
            }
            catch (Exception ex)
            {
                RitsuLibFramework.Logger.Error($"[Audio] FMOD play file: {ex.Message}");
                return false;
            }
        }

        public static bool TryUnloadFile(string absoluteOrResPath)
        {
            return !Loaded.TryRemove(absoluteOrResPath, out _) ||
                   FmodStudioGateway.TryCall(FmodStudioMethodNames.UnloadFile, absoluteOrResPath);
        }

        public static void TryUnloadAllTracked()
        {
            foreach (var key in Loaded.Keys.ToArray())
                TryUnloadFile(key);
        }

        private enum LoadedKind : byte
        {
            Sound = 1,
            MusicStream = 2,
        }
    }
}
