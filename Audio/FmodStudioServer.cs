using Godot;
using STS2RitsuLib.Audio.Internal;

namespace STS2RitsuLib.Audio
{
    /// <summary>
    ///     FMOD Studio bank and path probes. For gameplay sounds that should follow vanilla mixer settings, use
    ///     <see cref="GameFmod.Studio" /> instead.
    /// </summary>
    public static class FmodStudioServer
    {
        public static GodotObject? TryGet()
        {
            return FmodStudioGateway.TryGetServer();
        }

        public static bool TryLoadBank(string resourcePath, FmodStudioLoadBankMode mode = FmodStudioLoadBankMode.Normal)
        {
            return FmodStudioGateway.TryCall(FmodStudioMethodNames.LoadBank, resourcePath, (int)mode);
        }

        public static bool TryUnloadBank(string resourcePath)
        {
            return FmodStudioGateway.TryCall(FmodStudioMethodNames.UnloadBank, resourcePath);
        }

        public static bool? TryCheckEventPath(string eventPath)
        {
            if (!FmodStudioGateway.TryCall(out var v, FmodStudioMethodNames.CheckEventPath, eventPath))
                return null;

            return v.AsBool();
        }

        public static bool? TryCheckBusPath(string busPath)
        {
            if (!FmodStudioGateway.TryCall(out var v, FmodStudioMethodNames.CheckBusPath, busPath))
                return null;

            return v.AsBool();
        }
    }
}
