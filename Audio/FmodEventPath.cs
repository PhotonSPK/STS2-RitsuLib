namespace STS2RitsuLib.Audio
{
    /// <summary>
    ///     FMOD Studio event path (e.g. <c>event:/sfx/block_gain</c>). Implicitly converts to <see cref="string" />.
    /// </summary>
    public readonly record struct FmodEventPath(string Value)
    {
        public bool IsEmpty => string.IsNullOrEmpty(Value);

        public static implicit operator string(FmodEventPath path)
        {
            return path.Value;
        }

        public static implicit operator FmodEventPath(string value)
        {
            return new(value);
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
