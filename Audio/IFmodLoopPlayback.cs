namespace STS2RitsuLib.Audio
{
    /// <summary>Looping events keyed by path (vanilla loop dictionary semantics).</summary>
    public interface IFmodLoopPlayback
    {
        void PlayLoop(string eventPath, bool usesLoopParam = true);

        void StopLoop(string eventPath);

        void SetLoopParameter(string eventPath, string parameterName, float value);

        void StopAllLoops();
    }
}
