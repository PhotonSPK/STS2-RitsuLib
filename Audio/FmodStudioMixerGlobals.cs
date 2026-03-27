using Godot;
using STS2RitsuLib.Audio.Internal;

namespace STS2RitsuLib.Audio
{
    /// <summary>Studio global parameters, system-wide mute/pause, DSP buffer, and performance snapshot.</summary>
    public static class FmodStudioMixerGlobals
    {
        public static bool TrySetGlobalParameter(string name, float value)
        {
            return FmodStudioGateway.TryCall(FmodStudioMethodNames.SetGlobalParameterByName, name, value);
        }

        public static float TryGetGlobalParameter(string name)
        {
            if (!FmodStudioGateway.TryCall(out var v, FmodStudioMethodNames.GetGlobalParameterByName, name))
                return 0f;

            try
            {
                return v.AsSingle();
            }
            catch
            {
                return 0f;
            }
        }

        public static bool TrySetGlobalParameterByLabel(string name, string label)
        {
            return FmodStudioGateway.TryCall(FmodStudioMethodNames.SetGlobalParameterByNameWithLabel, name, label);
        }

        public static bool TryMuteAllEvents()
        {
            return FmodStudioGateway.TryCall(FmodStudioMethodNames.MuteAllEvents);
        }

        public static bool TryUnmuteAllEvents()
        {
            return FmodStudioGateway.TryCall(FmodStudioMethodNames.UnmuteAllEvents);
        }

        public static bool TryPauseAllEvents()
        {
            return FmodStudioGateway.TryCall(FmodStudioMethodNames.PauseAllEvents);
        }

        public static bool TryUnpauseAllEvents()
        {
            return FmodStudioGateway.TryCall(FmodStudioMethodNames.UnpauseAllEvents);
        }

        public static bool TrySetDspBufferSize(int bufferLength, int bufferCount)
        {
            return FmodStudioGateway.TryCall(FmodStudioMethodNames.SetSystemDspBufferSize, bufferLength, bufferCount);
        }

        /// <summary>Addon-specific performance payload; inspect in debugger or forward to your telemetry.</summary>
        public static Variant TryGetPerformanceData()
        {
            return FmodStudioGateway.TryCall(out var v, FmodStudioMethodNames.GetPerformanceData)
                ? v
                : default;
        }
    }
}
