using System.Text.Json;
using STS2RitsuLib.Utils.Persistence.Migration;

namespace STS2RitsuLib.Utils.Persistence
{
    public class PersistentDataEntry<T> where T : class, new()
    {
        private readonly bool _autoCreateIfMissing;
        private readonly T _defaultValues;
        private readonly string _fileName;
        private readonly string _modId;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly MigrationManager _migrationManager;

        public PersistentDataEntry(
            string modId,
            string fileName,
            SaveScope scope,
            T defaultValues,
            JsonSerializerOptions jsonOptions,
            MigrationManager migrationManager,
            bool autoCreateIfMissing = false)
        {
            _modId = modId;
            _fileName = fileName;
            Scope = scope;
            _defaultValues = defaultValues;
            _jsonOptions = jsonOptions;
            _migrationManager = migrationManager;
            _autoCreateIfMissing = autoCreateIfMissing;
            Data = DeepClone(defaultValues);
        }

        public T Data { get; private set; }
        public string FilePath => ProfileManager.Instance.GetFilePath(_fileName, Scope, _modId);
        public SaveScope Scope { get; }

        public event Action? Changed;

        public bool Load()
        {
            var currentPath = FilePath;
            RitsuLibFramework.Logger.Debug($"[Persistence] [{_fileName}] Loading from: {currentPath}");

            var result = FileOperations.ReadTextWithBackupFallback(currentPath, _fileName);

            if (!result.Success || string.IsNullOrEmpty(result.Content))
            {
                RitsuLibFramework.Logger.Info(
                    $"[Persistence] [{_fileName}] Using default values: {result.ErrorMessage}");
                Data = DeepClone(_defaultValues);

                if (_autoCreateIfMissing && !FileOperations.FileExists(currentPath))
                    Save();

                Changed?.Invoke();
                return false;
            }

            var migrationResult = _migrationManager.Migrate<T>(result.Content, _jsonOptions);

            if (!migrationResult.Success)
            {
                RitsuLibFramework.Logger.Warn(
                    $"[Persistence] [{_fileName}] Migration failed: {migrationResult.ErrorMessage}");

                if (migrationResult.RequiresRecovery)
                    MarkCorrupt(currentPath);

                Data = DeepClone(_defaultValues);
                Changed?.Invoke();
                return false;
            }

            Data = migrationResult.Data!;

            if (migrationResult.WasMigrated)
            {
                RitsuLibFramework.Logger.Info(
                    $"[Persistence] [{_fileName}] Data migrated to version {migrationResult.FinalVersion}");
                Save();
            }

            if (result.LoadedFromBackup)
                Save();

            Changed?.Invoke();
            return true;
        }

        public bool Save()
        {
            return SaveTo(FilePath);
        }

        public bool SaveTo(string path)
        {
            try
            {
                RitsuLibFramework.Logger.Debug($"[Persistence] [{_fileName}] Saving to: {path}");
                var json = JsonSerializer.Serialize(Data, _jsonOptions);
                var result = FileOperations.WriteText(path, json, _fileName);
                return result.Success;
            }
            catch (Exception ex)
            {
                RitsuLibFramework.Logger.Error($"[Persistence] [{_fileName}] Save to '{path}' failed: {ex.Message}");
                return false;
            }
        }

        public void Modify(Action<T> modifier)
        {
            modifier(Data);
            Changed?.Invoke();
        }

        private void MarkCorrupt(string path)
        {
            try
            {
                var corruptPath = path + ".corrupt";
                FileOperations.RenameFile(path, corruptPath, _fileName);
                RitsuLibFramework.Logger.Warn($"[Persistence] [{_fileName}] Corrupt file renamed to {corruptPath}");
            }
            catch (Exception ex)
            {
                RitsuLibFramework.Logger.Error($"[Persistence] [{_fileName}] Failed to mark corrupt: {ex.Message}");
            }
        }

        private T DeepClone(T source)
        {
            try
            {
                var json = JsonSerializer.Serialize(source, _jsonOptions);
                return JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? new T();
            }
            catch
            {
                return new();
            }
        }
    }
}
