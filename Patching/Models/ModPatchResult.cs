namespace STS2RitsuLib.Patching.Models
{
    /// <summary>
    ///     Represents the result of a patch application attempt
    /// </summary>
    public class ModPatchResult(
        ModPatchInfo modPatchInfo,
        bool success,
        string errorMessage = "",
        Exception? exception = null,
        bool ignored = false)
    {
        public ModPatchInfo ModPatchInfo { get; } = modPatchInfo;
        public bool Success { get; } = success;
        public string ErrorMessage { get; } = errorMessage;
        public Exception? Exception { get; } = exception;
        public bool Ignored { get; } = ignored;

        public static ModPatchResult CreateSuccess(ModPatchInfo modPatchInfo)
        {
            return new(modPatchInfo, true);
        }

        public static ModPatchResult CreateFailure(ModPatchInfo modPatchInfo, string errorMessage,
            Exception? exception = null)
        {
            return new(modPatchInfo, false, errorMessage, exception);
        }

        public static ModPatchResult CreateIgnored(ModPatchInfo modPatchInfo, string message)
        {
            return new(modPatchInfo, true, message, null, true);
        }

        public override string ToString()
        {
            return Success
                ? Ignored ? $"- {ModPatchInfo.Id}: {ErrorMessage}" : $"✓ {ModPatchInfo.Id}"
                : $"✗ {ModPatchInfo.Id}: {ErrorMessage}";
        }
    }
}
