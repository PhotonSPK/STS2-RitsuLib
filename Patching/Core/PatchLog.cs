using System.Collections.Concurrent;
using MegaCrit.Sts2.Core.Logging;

namespace STS2RitsuLib.Patching.Core
{
    /// <summary>
    ///     Logger registry for IPatchMethod types.
    /// </summary>
    public static class PatchLog
    {
        private static readonly ConcurrentDictionary<Type, Logger> Registry = new();

        public static void Bind(Type patchType, Logger logger)
        {
            ArgumentNullException.ThrowIfNull(patchType);
            ArgumentNullException.ThrowIfNull(logger);

            Registry[patchType] = logger;
        }

        public static Logger For(Type patchType)
        {
            ArgumentNullException.ThrowIfNull(patchType);
            return Registry.TryGetValue(patchType, out var logger)
                ? logger
                : RitsuLibFramework.Logger;
        }

        public static Logger For<TPatch>()
        {
            return For(typeof(TPatch));
        }
    }
}
