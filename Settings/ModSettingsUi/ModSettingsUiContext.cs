using STS2RitsuLib.Compat;
using STS2RitsuLib.Utils.Persistence;

namespace STS2RitsuLib.Settings
{
    internal sealed class ModSettingsUiContext(RitsuModSettingsSubmenu submenu) : IModSettingsUiActionHost
    {
        public void MarkDirty(IModSettingsBinding binding)
        {
            submenu.MarkDirty(binding);
        }

        public void RequestRefresh()
        {
            submenu.RequestRefresh();
        }

        public static string Resolve(ModSettingsText? text, string fallback = "")
        {
            return text?.Resolve() ?? fallback;
        }

        public static string ResolvePageTitle(ModSettingsPage page)
        {
            return ModSettingsLocalization.ResolvePageDisplayName(page);
        }

        public static string? ResolvePageDescription(ModSettingsPage page)
        {
            var resolved = page.Description?.Resolve();
            if (!string.IsNullOrWhiteSpace(resolved))
                return resolved;

            return Sts2ModManagerCompat.EnumerateModsForManifestLookup()
                .FirstOrDefault(mod => string.Equals(mod.manifest?.id, page.ModId, StringComparison.OrdinalIgnoreCase))
                ?.manifest?.description;
        }

        public static string ComposeBindingDescription(ModSettingsText? description, IModSettingsBinding binding)
        {
            if (binding is ITransientModSettingsBinding)
            {
                var transientText = ModSettingsLocalization.Get("scope.transient", "Preview only - not persisted");
                var transientDescription = Resolve(description);
                return string.IsNullOrWhiteSpace(transientDescription)
                    ? transientText
                    : $"{transientDescription}  [color=#B9B09A]- {transientText}[/color]";
            }

            var scopeText = binding.Scope == SaveScope.Profile
                ? ModSettingsLocalization.Get("scope.profile", "Stored per profile")
                : ModSettingsLocalization.Get("scope.global", "Stored globally");

            var resolvedDescription = Resolve(description);
            return string.IsNullOrWhiteSpace(resolvedDescription)
                ? scopeText
                : $"{resolvedDescription}  [color=#B9B09A]- {scopeText}[/color]";
        }

        public void RegisterRefresh(Action action)
        {
            submenu.RegisterRefreshAction(action);
        }

        public void NavigateToPage(string pageId)
        {
            submenu.NavigateToPage(pageId);
        }

        public void NotifyPasteFailure(ModSettingsPasteFailureReason reason)
        {
            submenu.ShowPasteFailure(reason);
        }
    }
}
