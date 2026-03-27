namespace STS2RitsuLib.Audio
{
    /// <summary>In-memory pool of event or file paths with simple no-repeat random selection.</summary>
    public sealed class FmodPathRoundRobinPool
    {
        private readonly List<string> _entries;
        private readonly Random _rng = new();
        private int _lastIndex = -1;

        public FmodPathRoundRobinPool(IEnumerable<string> paths)
        {
            ArgumentNullException.ThrowIfNull(paths);
            _entries = [.. paths];
        }

        public IReadOnlyList<string> Entries => _entries;

        public bool TryPickNext(out string path)
        {
            path = "";
            if (_entries.Count == 0)
                return false;

            if (_entries.Count == 1)
            {
                path = _entries[0];
                return true;
            }

            int index;
            do
            {
                index = _rng.Next(_entries.Count);
            } while (index == _lastIndex);

            _lastIndex = index;
            path = _entries[index];
            return true;
        }
    }
}
