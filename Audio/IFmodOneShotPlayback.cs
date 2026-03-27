namespace STS2RitsuLib.Audio
{
    /// <summary>One-shots through <see cref="MegaCrit.Sts2.Core.Nodes.Audio.NAudioManager" />.</summary>
    public interface IFmodOneShotPlayback
    {
        void PlayOneShot(string eventPath, float volume = 1f);

        void PlayOneShot(string eventPath, IReadOnlyDictionary<string, float> parameters, float volume = 1f);
    }
}
