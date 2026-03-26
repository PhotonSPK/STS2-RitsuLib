using System.Collections.Concurrent;

namespace STS2RitsuLib.Settings
{
    /// <summary>
    ///     Minimal host exposed when registering actions with <see cref="ModSettingsUiActionRegistry" /> (refresh and dirty
    ///     marking).
    /// </summary>
    public interface IModSettingsUiActionHost
    {
        void RequestRefresh();

        void MarkDirty(IModSettingsBinding binding);
    }

    /// <summary>
    ///     Stable ids for built-in settings menu items (convention for extensions; not guaranteed unique).
    /// </summary>
    public static class ModSettingsStandardActionIds
    {
        public const string ResetToDefault = "ritsulib.settings.resetDefault";
        public const string Copy = "ritsulib.settings.copy";
        public const string Paste = "ritsulib.settings.paste";
        public const string MoveUp = "ritsulib.settings.moveUp";
        public const string MoveDown = "ritsulib.settings.moveDown";
        public const string Duplicate = "ritsulib.settings.duplicate";
        public const string Remove = "ritsulib.settings.remove";
        public const string PageCopy = "ritsulib.settings.page.copy";
        public const string PagePaste = "ritsulib.settings.page.paste";
        public const string SectionCopy = "ritsulib.settings.section.copy";
        public const string SectionPaste = "ritsulib.settings.section.paste";
    }

    /// <summary>
    ///     Action context for a settings page (no binding).
    /// </summary>
    public sealed class ModSettingsPageUiContext(ModSettingsPage page, IModSettingsUiActionHost host)
    {
        public ModSettingsPage Page { get; } = page;
        public IModSettingsUiActionHost Host { get; } = host;
    }

    /// <summary>
    ///     Action context for a settings section (no standalone binding).
    /// </summary>
    public sealed class ModSettingsSectionUiContext(
        ModSettingsPage page,
        ModSettingsSection section,
        IModSettingsUiActionHost host)
    {
        public ModSettingsPage Page { get; } = page;
        public ModSettingsSection Section { get; } = section;
        public IModSettingsUiActionHost Host { get; } = host;
    }

    public enum ModSettingsClipboardScope
    {
        Self = 0,
        Subtree = 1,
    }

    /// <summary>
    ///     One command in the settings Actions menu or context menu.
    /// </summary>
    public sealed record ModSettingsMenuAction(string? Id, string Label, Func<bool> IsEnabled, Action Action)
    {
        public ModSettingsMenuAction(string label, bool enabled, Action action)
            : this(null, label, () => enabled, action)
        {
        }

        public ModSettingsMenuAction(string label, Func<bool> isEnabled, Action action)
            : this(null, label, isEnabled, action)
        {
        }

        public ModSettingsMenuAction(string? id, string label, bool enabled, Action action)
            : this(id, label, () => enabled, action)
        {
        }
    }

    /// <summary>
    ///     Appends custom menu items for binding rows, list items, pages, and sections.
    /// </summary>
    public static class ModSettingsUiActionRegistry
    {
        private static readonly ConcurrentDictionary<Type, BindingAppenderBag> BindingAppenders = new();
        private static readonly ConcurrentDictionary<Type, ListItemAppenderBag> ListItemAppenders = new();
        private static readonly PageAppenderBag PageAppenders = new();
        private static readonly SectionAppenderBag SectionAppenders = new();

        public static void RegisterBindingActionAppender<TValue>(
            Action<IModSettingsUiActionHost, IModSettingsValueBinding<TValue>, List<ModSettingsMenuAction>> append)
        {
            BindingAppenders.GetOrAdd(typeof(TValue), _ => new()).Add(append);
        }

        public static void RegisterListItemActionAppender<TItem>(
            Action<IModSettingsUiActionHost, ModSettingsListItemContext<TItem>, List<ModSettingsMenuAction>> append)
        {
            ListItemAppenders.GetOrAdd(typeof(TItem), _ => new()).Add(append);
        }

        public static void RegisterPageActionAppender(
            Action<IModSettingsUiActionHost, ModSettingsPageUiContext, List<ModSettingsMenuAction>> append)
        {
            ArgumentNullException.ThrowIfNull(append);
            PageAppenders.Add(append);
        }

        public static void RegisterSectionActionAppender(
            Action<IModSettingsUiActionHost, ModSettingsSectionUiContext, List<ModSettingsMenuAction>> append)
        {
            ArgumentNullException.ThrowIfNull(append);
            SectionAppenders.Add(append);
        }

        internal static void AppendBindingActions<TValue>(IModSettingsUiActionHost host,
            IModSettingsValueBinding<TValue> binding, List<ModSettingsMenuAction> list)
        {
            if (BindingAppenders.TryGetValue(typeof(TValue), out var bag))
                bag.Invoke(host, binding, list);
        }

        internal static void AppendListItemActions<TItem>(IModSettingsUiActionHost host,
            ModSettingsListItemContext<TItem> itemContext, List<ModSettingsMenuAction> list)
        {
            if (ListItemAppenders.TryGetValue(typeof(TItem), out var bag))
                bag.Invoke(host, itemContext, list);
        }

        internal static void AppendPageActions(IModSettingsUiActionHost host, ModSettingsPageUiContext pageContext,
            List<ModSettingsMenuAction> list)
        {
            PageAppenders.Invoke(host, pageContext, list);
        }

        internal static void AppendSectionActions(IModSettingsUiActionHost host,
            ModSettingsSectionUiContext sectionContext,
            List<ModSettingsMenuAction> list)
        {
            SectionAppenders.Invoke(host, sectionContext, list);
        }

        private sealed class BindingAppenderBag
        {
            private readonly List<Delegate> _delegates = [];

            public void Add<TValue>(
                Action<IModSettingsUiActionHost, IModSettingsValueBinding<TValue>, List<ModSettingsMenuAction>> d)
            {
                _delegates.Add(d);
            }

            public void Invoke<TValue>(IModSettingsUiActionHost host, IModSettingsValueBinding<TValue> binding,
                List<ModSettingsMenuAction> sink)
            {
                foreach (var d in _delegates)
                    ((Action<IModSettingsUiActionHost, IModSettingsValueBinding<TValue>, List<ModSettingsMenuAction>>)d)
                        (host, binding, sink);
            }
        }

        private sealed class ListItemAppenderBag
        {
            private readonly List<Delegate> _delegates = [];

            public void Add<TItem>(
                Action<IModSettingsUiActionHost, ModSettingsListItemContext<TItem>, List<ModSettingsMenuAction>> d)
            {
                _delegates.Add(d);
            }

            public void Invoke<TItem>(IModSettingsUiActionHost host, ModSettingsListItemContext<TItem> itemContext,
                List<ModSettingsMenuAction> sink)
            {
                foreach (var d in _delegates)
                    ((Action<IModSettingsUiActionHost, ModSettingsListItemContext<TItem>, List<ModSettingsMenuAction>>)
                            d)
                        (host, itemContext, sink);
            }
        }

        private sealed class PageAppenderBag
        {
            private readonly
                List<Action<IModSettingsUiActionHost, ModSettingsPageUiContext, List<ModSettingsMenuAction>>>
                _delegates = [];

            public void Add(Action<IModSettingsUiActionHost, ModSettingsPageUiContext, List<ModSettingsMenuAction>> d)
            {
                _delegates.Add(d);
            }

            public void Invoke(IModSettingsUiActionHost host, ModSettingsPageUiContext pageContext,
                List<ModSettingsMenuAction> sink)
            {
                foreach (var d in _delegates)
                    d(host, pageContext, sink);
            }
        }

        private sealed class SectionAppenderBag
        {
            private readonly List<Action<IModSettingsUiActionHost, ModSettingsSectionUiContext,
                    List<ModSettingsMenuAction>>>
                _delegates = [];

            public void Add(
                Action<IModSettingsUiActionHost, ModSettingsSectionUiContext, List<ModSettingsMenuAction>> d)
            {
                _delegates.Add(d);
            }

            public void Invoke(IModSettingsUiActionHost host, ModSettingsSectionUiContext sectionContext,
                List<ModSettingsMenuAction> sink)
            {
                foreach (var d in _delegates)
                    d(host, sectionContext, sink);
            }
        }
    }
}
