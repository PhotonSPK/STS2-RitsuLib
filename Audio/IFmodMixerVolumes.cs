namespace STS2RitsuLib.Audio
{
    /// <summary>Mixer volumes using the same linear curve as vanilla settings.</summary>
    public interface IFmodMixerVolumes
    {
        void SetMasterVolume(float linear01);

        void SetSfxVolume(float linear01);

        void SetAmbienceVolume(float linear01);

        void SetBgmVolume(float linear01);
    }
}
