using MegaCrit.Sts2.Core.Nodes.Audio;

namespace STS2RitsuLib.Audio
{
    /// <summary>
    ///     Forwards to <see cref="NAudioManager" /> (same FMOD routing, buses, and test-mode behavior as vanilla).
    /// </summary>
    public sealed class GameFmodAudioService : IGameFmodAudio
    {
        private GameFmodAudioService()
        {
        }

        public static GameFmodAudioService Shared { get; } = new();

        private static NAudioManager? Manager => NAudioManager.Instance;

        public void PlayOneShot(string eventPath, float volume = 1f)
        {
            Manager?.PlayOneShot(eventPath, volume);
        }

        public void PlayOneShot(string eventPath, IReadOnlyDictionary<string, float> parameters, float volume = 1f)
        {
            if (Manager is null)
                return;

            if (parameters.Count == 0)
            {
                Manager.PlayOneShot(eventPath, volume);
                return;
            }

            Manager.PlayOneShot(eventPath, ToManagedDictionary(parameters), volume);
        }

        public void PlayLoop(string eventPath, bool usesLoopParam = true)
        {
            Manager?.PlayLoop(eventPath, usesLoopParam);
        }

        public void StopLoop(string eventPath)
        {
            Manager?.StopLoop(eventPath);
        }

        public void SetLoopParameter(string eventPath, string parameterName, float value)
        {
            Manager?.SetParam(eventPath, parameterName, value);
        }

        public void StopAllLoops()
        {
            Manager?.StopAllLoops();
        }

        public void PlayMusic(string eventPath)
        {
            Manager?.PlayMusic(eventPath);
        }

        public void StopMusic()
        {
            Manager?.StopMusic();
        }

        public void UpdateMusicParameter(string parameterName, string labelValue)
        {
            Manager?.UpdateMusicParameter(parameterName, labelValue);
        }

        public void SetMasterVolume(float linear01)
        {
            Manager?.SetMasterVol(linear01);
        }

        public void SetSfxVolume(float linear01)
        {
            Manager?.SetSfxVol(linear01);
        }

        public void SetAmbienceVolume(float linear01)
        {
            Manager?.SetAmbienceVol(linear01);
        }

        public void SetBgmVolume(float linear01)
        {
            Manager?.SetBgmVol(linear01);
        }

        private static Dictionary<string, float> ToManagedDictionary(IReadOnlyDictionary<string, float> parameters)
        {
            var d = new Dictionary<string, float>(parameters.Count);
            foreach (var kv in parameters)
                d[kv.Key] = kv.Value;

            return d;
        }
    }
}
