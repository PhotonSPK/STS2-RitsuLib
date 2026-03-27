namespace STS2RitsuLib.Compat
{
    /// <summary>
    ///     Configure minimum host versions for API branches. When <see cref="Sts2HostVersion.Numeric" /> is known and
    ///     compares to these, RitsuLib picks the matching path; when host version is unknown, behavior falls back to
    ///     reflection on the loaded <c>sts2</c> assembly.
    ///     <para />
    ///     Set non-null values when you know the first Steam / <c>release_info.json</c> version that shipped each API.
    /// </summary>
    internal static class Sts2ApiFeatureThresholds
    {
        /// <summary>
        ///     Host builds at or above this: use <c>SerializableRun.GameMode</c> and <c>RunState.GameMode</c> for epoch logic.
        ///     Below: use pre-GameMode heuristics (daily / modifiers). <c>null</c> = decide only by reflection.
        ///     <para />
        ///     Floor matches STS2 <c>release_info.json</c> <c>v0.101.0</c> and later retail / beta lines.
        /// </summary>
        internal static readonly Version? RunAndStateGameModeApiMinimum = new(0, 100, 0);

        /// <summary>
        ///     Host builds at or above this: treat <c>Mod.state</c> (load-state enum) as authoritative when filtering
        ///     <c>ModManager.Mods</c>. Below: use <c>Mod.wasLoaded</c>. <c>null</c> = decide only by reflection.
        ///     <para />
        ///     Same floor as <see cref="RunAndStateGameModeApiMinimum" /> (v0.101.0+).
        /// </summary>
        internal static readonly Version? ModLoadStateEnumApiMinimum = new(0, 100, 0);
    }
}
