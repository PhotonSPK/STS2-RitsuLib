using System.Reflection;
using MegaCrit.Sts2.Core.Modding;

namespace STS2RitsuLib.Compat
{
    /// <summary>
    ///     Enumerates mods across STS2 API variants (stable vs beta): <c>AllMods</c>/<c>LoadedMods</c> vs
    ///     <c>Mods</c>/<c>GetLoadedMods()</c>.
    /// </summary>
    internal static class Sts2ModManagerCompat
    {
        private static readonly Lock InitLock = new();
        private static Func<IEnumerable<Mod>>? _loadedMods;
        private static Func<IEnumerable<Mod>>? _allModsForLookup;

        internal static IEnumerable<Mod> EnumerateLoadedModsWithAssembly()
        {
            EnsureInitialized();
            return _loadedMods!();
        }

        /// <summary>
        ///     All registered mods (including disabled / not loaded), for manifest name/description lookup.
        /// </summary>
        internal static IEnumerable<Mod> EnumerateModsForManifestLookup()
        {
            EnsureInitialized();
            return _allModsForLookup!();
        }

        private static void EnsureInitialized()
        {
            if (_loadedMods != null && _allModsForLookup != null)
                return;

            lock (InitLock)
            {
                if (_loadedMods != null && _allModsForLookup != null)
                    return;

                _loadedMods = BuildLoadedModsEnumerator();
                _allModsForLookup = BuildAllModsEnumerator();
            }
        }

        private static Func<IEnumerable<Mod>> BuildLoadedModsEnumerator()
        {
            var t = typeof(ModManager);

            var getLoaded = t.GetMethod("GetLoadedMods", BindingFlags.Public | BindingFlags.Static, null,
                Type.EmptyTypes, null);
            if (getLoaded != null)
                return () => (IEnumerable<Mod>)getLoaded.Invoke(null, null)!;

            var loadedProp = t.GetProperty("LoadedMods", BindingFlags.Public | BindingFlags.Static);
            if (loadedProp != null)
                return () => (IEnumerable<Mod>)loadedProp.GetValue(null)!;

            var modsProp = t.GetProperty("Mods", BindingFlags.Public | BindingFlags.Static);
            if (modsProp != null)
                return () => FilterLoadedModsFromModsList(modsProp);

            throw new InvalidOperationException(
                "ModManager exposes no GetLoadedMods(), LoadedMods, or Mods; cannot enumerate loaded mods.");
        }

        private static IEnumerable<Mod> FilterLoadedModsFromModsList(PropertyInfo modsProp)
        {
            var raw = modsProp.GetValue(null);
            if (raw is not IEnumerable<Mod> enumerable)
                yield break;

            var modType = typeof(Mod);
            var stateProp = modType.GetProperty("state", BindingFlags.Public | BindingFlags.Instance);
            var wasLoadedField = modType.GetField("wasLoaded", BindingFlags.Public | BindingFlags.Instance);
            var wasLoadedProp = modType.GetProperty("wasLoaded", BindingFlags.Public | BindingFlags.Instance);

            foreach (var m in enumerable)
                if (IsModLoadedForDiscovery(m, stateProp, wasLoadedField, wasLoadedProp))
                    yield return m;
        }

        private static bool IsModLoadedForDiscovery(Mod m, PropertyInfo? stateProp, FieldInfo? wasLoadedField,
            PropertyInfo? wasLoadedProp)
        {
            if (Sts2ApiCapabilityGate.PreferModLoadStateEnumForLoadedDiscovery())
            {
                if (stateProp?.GetValue(m) is { } stateValue)
                    return string.Equals(stateValue.ToString(), "Loaded", StringComparison.Ordinal);
                if (wasLoadedProp?.GetValue(m) is bool wp)
                    return wp;
                if (wasLoadedField?.GetValue(m) is bool wf)
                    return wf;
                return true;
            }

            if (wasLoadedProp?.GetValue(m) is bool wpr)
                return wpr;
            if (wasLoadedField?.GetValue(m) is bool wfd)
                return wfd;
            return true;
        }

        private static Func<IEnumerable<Mod>> BuildAllModsEnumerator()
        {
            var t = typeof(ModManager);

            var allProp = t.GetProperty("AllMods", BindingFlags.Public | BindingFlags.Static);
            if (allProp != null)
                return () => (IEnumerable<Mod>)allProp.GetValue(null)!;

            var modsProp = t.GetProperty("Mods", BindingFlags.Public | BindingFlags.Static);
            if (modsProp != null)
                return () => (IEnumerable<Mod>)modsProp.GetValue(null)!;

            var loadedProp = t.GetProperty("LoadedMods", BindingFlags.Public | BindingFlags.Static);
            if (loadedProp != null)
                return () => (IEnumerable<Mod>)loadedProp.GetValue(null)!;

            var getLoaded = t.GetMethod("GetLoadedMods", BindingFlags.Public | BindingFlags.Static, null,
                Type.EmptyTypes, null);
            if (getLoaded != null)
                return () => (IEnumerable<Mod>)getLoaded.Invoke(null, null)!;

            throw new InvalidOperationException(
                "ModManager exposes no AllMods, Mods, LoadedMods, or GetLoadedMods(); cannot enumerate mods.");
        }
    }
}
