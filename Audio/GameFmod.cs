namespace STS2RitsuLib.Audio
{
    /// <summary>Game-routed FMOD Studio API (vanilla <see cref="MegaCrit.Sts2.Core.Nodes.Audio.NAudioManager" />).</summary>
    public static class GameFmod
    {
        public static IGameFmodAudio Studio { get; } = GameFmodAudioService.Shared;
    }
}
