namespace STS2RitsuLib.Patching.Models
{
    /// <summary>
    ///     Represents a target for patching (Type + Method combination)
    /// </summary>
    public record ModPatchTarget(Type TargetType, string MethodName, Type[]? ParameterTypes, bool IgnoreIfMissing)
    {
        public ModPatchTarget(Type targetType, string methodName, Type[]? parameterTypes)
            // ReSharper disable once IntroduceOptionalParameters.Global
            : this(targetType, methodName, parameterTypes, false)
        {
        }

        public ModPatchTarget(Type targetType, string methodName, bool ignoreIfMissing)
            : this(targetType, methodName, null, ignoreIfMissing)
        {
        }

        public ModPatchTarget(Type targetType, string methodName)
            // ReSharper disable IntroduceOptionalParameters.Global
            : this(targetType, methodName, null, false)
        // ReSharper restore IntroduceOptionalParameters.Global
        {
        }

        public override string ToString()
        {
            if (ParameterTypes == null) return $"{TargetType.Name}.{MethodName}";

            var paramNames = ParameterTypes.Length == 0
                ? "no parameters"
                : string.Join(", ", ParameterTypes.Select(p => p.Name));
            return $"{TargetType.Name}.{MethodName}({paramNames})";
        }
    }
}
