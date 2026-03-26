using System.Reflection.Emit;
using HarmonyLib;

namespace STS2RitsuLib.Utils.HarmonyIl
{
    /// <summary>
    ///     Small, auditable IL helpers for Harmony transpilers. Only patterns that are easy to reason about
    ///     and match verified game / stub method shapes belong here — not a general instruction matcher.
    /// </summary>
    public static class HarmonyVerifiedIl
    {
        /// <summary>
        ///     Inserts <paramref name="prefix" /> immediately before the first <see cref="OpCodes.Ret" /> in
        ///     <paramref name="body" />. Suitable for empty or single-return stub methods (e.g. ModInterop shims).
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <b>Not supported:</b> methods with no <c>ret</c>, multiple returns, or filter/try regions
        ///         where the first <c>ret</c> is not the intended injection site.
        ///     </para>
        ///     <para>Prefer writing a dedicated transpiler when you need label preservation or multi-site edits.</para>
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="prefix" /> is empty.</exception>
        /// <exception cref="InvalidOperationException">No <c>ret</c> opcode was found.</exception>
        public static List<CodeInstruction> InsertBeforeFirstRet(
            IEnumerable<CodeInstruction> body,
            IReadOnlyList<CodeInstruction> prefix)
        {
            ArgumentNullException.ThrowIfNull(body);
            ArgumentNullException.ThrowIfNull(prefix);
            if (prefix.Count == 0)
                throw new ArgumentException("Prefix must contain at least one instruction.", nameof(prefix));

            var list = body.ToList();
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].opcode != OpCodes.Ret)
                    continue;
                list.InsertRange(i, prefix);
                return list;
            }

            throw new InvalidOperationException(
                "No ret opcode found in method body; InsertBeforeFirstRet only supports bodies with at least one ret.");
        }

        /// <summary>
        ///     Same as <see cref="InsertBeforeFirstRet" /> but returns false when no <c>ret</c> exists, instead of throwing.
        /// </summary>
        public static bool TryInsertBeforeFirstRet(
            IEnumerable<CodeInstruction> body,
            IReadOnlyList<CodeInstruction> prefix,
            out List<CodeInstruction>? result)
        {
            result = null;
            ArgumentNullException.ThrowIfNull(body);
            ArgumentNullException.ThrowIfNull(prefix);
            if (prefix.Count == 0)
                return false;

            var list = body.ToList();
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].opcode != OpCodes.Ret)
                    continue;
                list.InsertRange(i, prefix);
                result = list;
                return true;
            }

            return false;
        }
    }
}
