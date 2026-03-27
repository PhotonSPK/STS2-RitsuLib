namespace STS2RitsuLib.Unlocks
{
    /// <summary>
    ///     Emits at most one warning per key so mod characters missing unlock-rule registration stay playable
    ///     without spamming logs every combat or frame.
    /// </summary>
    internal static class ModUnlockMissingRuleWarnings
    {
        private static readonly Lock SyncRoot = new();
        private static readonly HashSet<string> WarnedKeys = [];

        internal static void WarnOnce(string key, string message)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);

            lock (SyncRoot)
            {
                if (!WarnedKeys.Add(key))
                    return;
            }

            RitsuLibFramework.Logger.Warn(message);
        }
    }
}
