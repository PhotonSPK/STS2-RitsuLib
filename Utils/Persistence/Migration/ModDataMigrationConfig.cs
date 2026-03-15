namespace STS2RitsuLib.Utils.Persistence.Migration
{
    public sealed class ModDataMigrationConfig
    {
        public required int CurrentDataVersion { get; init; }
        public int MinimumSupportedDataVersion { get; init; }
        public string SchemaVersionProperty { get; init; } = ModDataVersion.SchemaVersionProperty;
    }
}
