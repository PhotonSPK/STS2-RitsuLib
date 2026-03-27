namespace STS2RitsuLib.Interop
{
    /// <summary>
    ///     Marks a class whose public methods, properties, and nested <see cref="InteropClassWrapper" /> types
    ///     are rewritten at runtime to call into another mod's assembly, avoiding a compile-time reference.
    /// </summary>
    /// <param name="modId">Manifest id of the mod that must be loaded for this interop block.</param>
    /// <param name="type">Default target CLR type name for members that do not specify <see cref="InteropTargetAttribute" />.</param>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ModInteropAttribute(string modId, string? type = null) : Attribute
    {
        public string ModId { get; } = modId;
        public string? Type { get; } = type;
    }

    /// <summary>
    ///     Optional per-member override for the target type or member name in the remote mod.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Method,
        Inherited = false)]
    public sealed class InteropTargetAttribute : Attribute
    {
        public InteropTargetAttribute(string type, string? name = null)
        {
            Type = type;
            Name = name;
        }

        public InteropTargetAttribute(string? name = null)
        {
            Name = name;
        }

        public string? Type { get; }
        public string? Name { get; }
    }
}
