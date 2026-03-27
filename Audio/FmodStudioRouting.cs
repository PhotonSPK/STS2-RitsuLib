namespace STS2RitsuLib.Audio
{
    /// <summary>
    ///     Studio bus paths used by the game's AudioManagerProxy (FMOD Studio). Use with <see cref="FmodStudioServer" /> for
    ///     direct bus access.
    /// </summary>
    public static class FmodStudioRouting
    {
        public const string MasterBus = "bus:/master";
        public const string SfxBus = "bus:/master/sfx";
        public const string AmbienceBus = "bus:/master/ambience";
        public const string MusicBus = "bus:/master/music";
    }
}
