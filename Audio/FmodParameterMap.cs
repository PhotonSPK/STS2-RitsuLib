namespace STS2RitsuLib.Audio
{
    /// <summary>Builds parameter maps for <see cref="IFmodOneShotPlayback" /> multi-parameter overloads.</summary>
    public static class FmodParameterMap
    {
        public static Dictionary<string, float> Empty()
        {
            return [];
        }

        public static Dictionary<string, float> Single(string name, float value)
        {
            return new() { [name] = value };
        }

        public static Dictionary<string, float> Of(params (string Name, float Value)[] pairs)
        {
            if (pairs.Length == 0)
                return [];

            var d = new Dictionary<string, float>(pairs.Length);
            foreach (var (name, value) in pairs)
                d[name] = value;

            return d;
        }
    }
}
