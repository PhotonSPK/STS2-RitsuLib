namespace STS2RitsuLib.Audio
{
    /// <summary>Single active music instance (same as vanilla proxy).</summary>
    public interface IFmodMusicPlayback
    {
        void PlayMusic(string eventPath);

        void StopMusic();

        void UpdateMusicParameter(string parameterName, string labelValue);
    }
}
