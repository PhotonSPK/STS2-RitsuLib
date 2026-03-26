using System.Text.Json;

namespace STS2RitsuLib.Settings
{
    /// <summary>
    ///     Per-section UI snapshot carried inside <see cref="ModSettingsPageClipboardPayload" />.
    /// </summary>
    public sealed record ModSettingsSectionStructureSnapshot(
        string SectionId,
        bool IsCollapsible,
        bool StartCollapsed,
        string[] EntryIds);

    /// <summary>
    ///     Clipboard payload for a page (no binding data; UI structure metadata only).
    /// </summary>
    public sealed record ModSettingsPageClipboardPayload(
        string ModId,
        string PageId,
        string? ParentPageId,
        int SortOrder,
        string[] SectionIds,
        string? TitleSnapshot = null,
        ModSettingsSectionStructureSnapshot[]? SectionStructures = null);

    /// <summary>
    ///     Clipboard payload for a section.
    /// </summary>
    public sealed record ModSettingsSectionClipboardPayload(
        string ModId,
        string PageId,
        string SectionId,
        bool IsCollapsible,
        string[] EntryIds,
        bool StartCollapsed = false,
        string? TitleSnapshot = null,
        string? DescriptionSnapshot = null);

    /// <summary>
    ///     Raised before page copy; when <see cref="SuppressDefaultClipboardWrite" /> is true, the default JSON envelope is
    ///     not written.
    /// </summary>
    public sealed class ModSettingsPageCopyEventArgs(ModSettingsPageUiContext context) : EventArgs
    {
        public ModSettingsPageUiContext Context { get; } = context;
        public bool SuppressDefaultClipboardWrite { get; set; }
    }

    /// <summary>
    ///     Page paste: subscribers apply semantics (e.g. layout); default does not write any binding.
    /// </summary>
    public sealed class ModSettingsPagePasteEventArgs(
        ModSettingsPageUiContext target,
        ModSettingsPageClipboardPayload? payload)
        : EventArgs
    {
        public ModSettingsPageUiContext Target { get; } = target;
        public ModSettingsPageClipboardPayload? Payload { get; } = payload;

        /// <summary>When true, this paste was consumed and later subscribers should not run.</summary>
        public bool Handled { get; set; }

        public bool Success { get; set; }
    }

    /// <summary>
    ///     Raised before section copy.
    /// </summary>
    public sealed class ModSettingsSectionCopyEventArgs(ModSettingsSectionUiContext context) : EventArgs
    {
        public ModSettingsSectionUiContext Context { get; } = context;
        public bool SuppressDefaultClipboardWrite { get; set; }
    }

    /// <summary>
    ///     Section paste: subscribers apply semantics.
    /// </summary>
    public sealed class ModSettingsSectionPasteEventArgs(
        ModSettingsSectionUiContext target,
        ModSettingsSectionClipboardPayload? payload)
        : EventArgs
    {
        public ModSettingsSectionUiContext Target { get; } = target;
        public ModSettingsSectionClipboardPayload? Payload { get; } = payload;
        public bool Handled { get; set; }
        public bool Success { get; set; }
    }

    /// <summary>
    ///     Clipboard helpers for binding-less UI chrome (pages/sections): default copy serializes structure metadata; paste
    ///     requires custom handlers.
    /// </summary>
    public static class ModSettingsUiChromeClipboard
    {
        public const string PageKind = "ritsulib.settings.ui.page";
        public const string SectionKind = "ritsulib.settings.ui.section";

        private const string PageTypeName = "ritsulib.settings.ui.page.v2";
        private const string SectionTypeName = "ritsulib.settings.ui.section.v2";

        /// <summary>
        ///     When true, the page Paste menu item is enabled when the clipboard matches and ModId matches the page;
        ///     <see cref="PagePasteRequested" /> performs the work.
        /// </summary>
        public static bool EnablePagePasteUi { get; set; }

        /// <summary>
        ///     When true, the section Paste menu item is enabled when the clipboard matches and ModId matches.
        /// </summary>
        public static bool EnableSectionPasteUi { get; set; }

        public static event Action<ModSettingsPageCopyEventArgs>? PageCopyRequested;
        public static event Action<ModSettingsPagePasteEventArgs>? PagePasteRequested;
        public static event Action<ModSettingsSectionCopyEventArgs>? SectionCopyRequested;
        public static event Action<ModSettingsSectionPasteEventArgs>? SectionPasteRequested;

        public static bool TryCopyPage(ModSettingsPageUiContext context)
        {
            var args = new ModSettingsPageCopyEventArgs(context);
            PageCopyRequested?.Invoke(args);
            if (args.SuppressDefaultClipboardWrite)
                return true;

            var sectionIds = context.Page.Sections.Select(s => s.Id).ToArray();
            var sectionStructures = context.Page.Sections
                .Select(s => new ModSettingsSectionStructureSnapshot(
                    s.Id,
                    s.IsCollapsible,
                    s.StartCollapsed,
                    s.Entries.Select(e => e.Id).ToArray()))
                .ToArray();

            var payload = new ModSettingsPageClipboardPayload(
                context.Page.ModId,
                context.Page.Id,
                context.Page.ParentPageId,
                context.Page.SortOrder,
                sectionIds,
                context.Page.Title?.Resolve(),
                sectionStructures);

            ModSettingsClipboardData.WriteClipboardEnvelope(new(
                PageKind,
                PageTypeName,
                $"{context.Page.ModId}|{context.Page.Id}",
                string.Empty,
                ModSettingsClipboardScope.Self,
                JsonSerializer.Serialize(payload)));

            return true;
        }

        public static bool TryGetPagePayload(string clipboardText, out ModSettingsPageClipboardPayload? payload)
        {
            payload = null;
            if (!ModSettingsClipboardData.TryDeserializeEnvelope(clipboardText, out var env) || env == null)
                return false;

            if (!string.Equals(env.Kind, PageKind, StringComparison.Ordinal))
                return false;

            try
            {
                payload = JsonSerializer.Deserialize<ModSettingsPageClipboardPayload>(env.Payload);
                return payload != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool CanPastePage(ModSettingsPageUiContext context)
        {
            if (!EnablePagePasteUi)
                return false;

            if (!ModSettingsClipboardAccess.TryGetText(out var clip) ||
                !TryGetPagePayload(clip, out var payload) || payload == null)
                return false;

            return string.Equals(payload.ModId, context.Page.ModId, StringComparison.Ordinal);
        }

        public static bool TryPastePage(ModSettingsPageUiContext context)
        {
            ModSettingsClipboardAccess.TryGetText(out var clip);
            TryGetPagePayload(clip, out var payload);

            var args = new ModSettingsPagePasteEventArgs(context, payload);
            var h = PagePasteRequested;
            if (h == null) return false;
            foreach (var @delegate in h.GetInvocationList())
            {
                var d = (Action<ModSettingsPagePasteEventArgs>)@delegate;
                d(args);
                if (args.Handled)
                    return args.Success;
            }

            return false;
        }

        public static bool TryCopySection(ModSettingsSectionUiContext context)
        {
            var args = new ModSettingsSectionCopyEventArgs(context);
            SectionCopyRequested?.Invoke(args);
            if (args.SuppressDefaultClipboardWrite)
                return true;

            var payload = new ModSettingsSectionClipboardPayload(
                context.Page.ModId,
                context.Page.Id,
                context.Section.Id,
                context.Section.IsCollapsible,
                context.Section.Entries.Select(e => e.Id).ToArray(),
                context.Section.StartCollapsed,
                context.Section.Title?.Resolve(),
                context.Section.Description?.Resolve());

            ModSettingsClipboardData.WriteClipboardEnvelope(new(
                SectionKind,
                SectionTypeName,
                $"{context.Page.ModId}|{context.Page.Id}|{context.Section.Id}",
                string.Empty,
                ModSettingsClipboardScope.Self,
                JsonSerializer.Serialize(payload)));

            return true;
        }

        public static bool TryGetSectionPayload(string clipboardText, out ModSettingsSectionClipboardPayload? payload)
        {
            payload = null;
            if (!ModSettingsClipboardData.TryDeserializeEnvelope(clipboardText, out var env) || env == null)
                return false;

            if (!string.Equals(env.Kind, SectionKind, StringComparison.Ordinal))
                return false;

            try
            {
                payload = JsonSerializer.Deserialize<ModSettingsSectionClipboardPayload>(env.Payload);
                return payload != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool CanPasteSection(ModSettingsSectionUiContext context)
        {
            if (!EnableSectionPasteUi)
                return false;

            if (!ModSettingsClipboardAccess.TryGetText(out var clip) ||
                !TryGetSectionPayload(clip, out var payload) || payload == null)
                return false;

            return string.Equals(payload.ModId, context.Page.ModId, StringComparison.Ordinal);
        }

        public static bool TryPasteSection(ModSettingsSectionUiContext context)
        {
            ModSettingsClipboardAccess.TryGetText(out var clip);
            TryGetSectionPayload(clip, out var payload);

            var args = new ModSettingsSectionPasteEventArgs(context, payload);
            var h = SectionPasteRequested;
            if (h == null) return false;
            foreach (var @delegate in h.GetInvocationList())
            {
                var d = (Action<ModSettingsSectionPasteEventArgs>)@delegate;
                d(args);
                if (args.Handled)
                    return args.Success;
            }

            return false;
        }
    }
}
