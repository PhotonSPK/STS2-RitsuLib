namespace STS2RitsuLib.Settings
{
    public sealed class ModSettingsPage
    {
        internal ModSettingsPage(
            string modId,
            string id,
            string? parentPageId,
            ModSettingsText? title,
            ModSettingsText? description,
            int sortOrder,
            IReadOnlyList<ModSettingsSection> sections)
        {
            ModId = modId;
            Id = id;
            ParentPageId = parentPageId;
            Title = title;
            Description = description;
            SortOrder = sortOrder;
            Sections = sections;
        }

        public string ModId { get; }
        public string Id { get; }
        public string? ParentPageId { get; }
        public ModSettingsText? Title { get; }
        public ModSettingsText? Description { get; }
        public int SortOrder { get; }
        public IReadOnlyList<ModSettingsSection> Sections { get; }
    }

    public sealed class ModSettingsSection
    {
        internal ModSettingsSection(
            string id,
            ModSettingsText? title,
            ModSettingsText? description,
            bool isCollapsible,
            bool startCollapsed,
            IReadOnlyList<ModSettingsEntryDefinition> entries)
        {
            Id = id;
            Title = title;
            Description = description;
            IsCollapsible = isCollapsible;
            StartCollapsed = startCollapsed;
            Entries = entries;
        }

        public string Id { get; }
        public ModSettingsText? Title { get; }
        public ModSettingsText? Description { get; }
        public bool IsCollapsible { get; }
        public bool StartCollapsed { get; }
        public IReadOnlyList<ModSettingsEntryDefinition> Entries { get; }
    }
}
