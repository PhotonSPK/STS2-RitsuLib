using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;

namespace STS2RitsuLib.Audio
{
    /// <summary>
    ///     SFX aligned with <see cref="SfxCmd" /> guards. Use <see cref="GameFmod.Studio" /> for music, mixer, or unguarded
    ///     playback.
    /// </summary>
    public static class Sts2SfxAlignedFmod
    {
        public static void PlayOneShot(string eventPath, float volume = 1f)
        {
            SfxCmd.Play(eventPath, volume);
        }

        public static void PlayOneShot(string eventPath, string parameterName, float parameterValue, float volume = 1f)
        {
            SfxCmd.Play(eventPath, parameterName, parameterValue, volume);
        }

        public static void PlayOneShot(string eventPath, IReadOnlyDictionary<string, float> parameters,
            float volume = 1f)
        {
            if (NonInteractiveMode.IsActive || CombatManager.Instance.IsEnding)
                return;

            GameFmod.Studio.PlayOneShot(eventPath, parameters, volume);
        }

        public static void PlayLoop(string eventPath, bool usesLoopParam = true)
        {
            SfxCmd.PlayLoop(eventPath, usesLoopParam);
        }

        public static void StopLoop(string eventPath)
        {
            SfxCmd.StopLoop(eventPath);
        }

        public static void SetLoopParameter(string eventPath, string parameterName, float value)
        {
            SfxCmd.SetParam(eventPath, parameterName, value);
        }
    }
}
