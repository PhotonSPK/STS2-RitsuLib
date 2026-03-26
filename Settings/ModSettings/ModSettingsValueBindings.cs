using System.Text.Json;
using STS2RitsuLib.Utils.Persistence;

namespace STS2RitsuLib.Settings
{
    public sealed class ModSettingsValueBinding<TModel, TValue>(
        string modId,
        string dataKey,
        SaveScope scope,
        Func<TModel, TValue> getter,
        Action<TModel, TValue> setter)
        : IModSettingsValueBinding<TValue>
        where TModel : class, new()
    {
        public string ModId { get; } = modId;
        public string DataKey { get; } = dataKey;
        public SaveScope Scope { get; } = scope;

        public TValue Read()
        {
            var store = RitsuLibFramework.GetDataStore(ModId);
            return getter(store.Get<TModel>(DataKey));
        }

        public void Write(TValue value)
        {
            var store = RitsuLibFramework.GetDataStore(ModId);
            store.Modify<TModel>(DataKey, model => setter(model, value));
        }

        public void Save()
        {
            RitsuLibFramework.GetDataStore(ModId).Save(DataKey);
        }
    }

    public sealed class InMemoryModSettingsValueBinding<TValue>(string modId, string dataKey, TValue initialValue)
        : IStructuredModSettingsValueBinding<TValue>, ITransientModSettingsBinding,
            IDefaultModSettingsValueBinding<TValue>
    {
        private readonly TValue _defaultValue = initialValue;
        private TValue _value = initialValue;

        public TValue CreateDefaultValue()
        {
            return Adapter.Clone(_defaultValue);
        }

        public string ModId { get; } = modId;
        public string DataKey { get; } = dataKey;
        public SaveScope Scope { get; } = SaveScope.Global;
        public IStructuredModSettingsValueAdapter<TValue> Adapter { get; } = ModSettingsStructuredData.Json<TValue>();

        public TValue Read()
        {
            return _value;
        }

        public void Write(TValue value)
        {
            _value = value;
        }

        public void Save()
        {
        }
    }

    public sealed class StructuredModSettingsValueBinding<TValue>(
        IModSettingsValueBinding<TValue> inner,
        IStructuredModSettingsValueAdapter<TValue> adapter)
        : IStructuredModSettingsValueBinding<TValue>
    {
        public string ModId => inner.ModId;
        public string DataKey => inner.DataKey;
        public SaveScope Scope => inner.Scope;
        public IStructuredModSettingsValueAdapter<TValue> Adapter { get; } = adapter;

        public TValue Read()
        {
            return inner.Read();
        }

        public void Write(TValue value)
        {
            inner.Write(value);
        }

        public void Save()
        {
            inner.Save();
        }
    }

    public sealed class ProjectedModSettingsValueBinding<TSource, TValue>(
        IModSettingsValueBinding<TSource> parent,
        string dataKey,
        Func<TSource, TValue> getter,
        Func<TSource, TValue, TSource> setter,
        IStructuredModSettingsValueAdapter<TValue>? adapter = null)
        : IStructuredModSettingsValueBinding<TValue>
    {
        public string ModId => parent.ModId;
        public string DataKey => string.IsNullOrWhiteSpace(dataKey) ? parent.DataKey : $"{parent.DataKey}.{dataKey}";
        public SaveScope Scope => parent.Scope;

        public IStructuredModSettingsValueAdapter<TValue> Adapter { get; } =
            adapter ?? ModSettingsStructuredData.Json<TValue>();

        public TValue Read()
        {
            return getter(parent.Read());
        }

        public void Write(TValue value)
        {
            var source = parent.Read();
            parent.Write(setter(source, value));
        }

        public void Save()
        {
            parent.Save();
        }
    }

    public sealed class DefaultModSettingsValueBinding<TValue>(
        IModSettingsValueBinding<TValue> inner,
        Func<TValue> defaultValueFactory,
        IStructuredModSettingsValueAdapter<TValue>? adapter = null)
        : IStructuredModSettingsValueBinding<TValue>, IDefaultModSettingsValueBinding<TValue>
    {
        public TValue CreateDefaultValue()
        {
            return defaultValueFactory();
        }

        public string ModId => inner.ModId;
        public string DataKey => inner.DataKey;
        public SaveScope Scope => inner.Scope;

        public IStructuredModSettingsValueAdapter<TValue> Adapter { get; } =
            inner is IStructuredModSettingsValueBinding<TValue> structured
                ? structured.Adapter
                : adapter ?? ModSettingsStructuredData.Json<TValue>();

        public TValue Read()
        {
            return inner.Read();
        }

        public void Write(TValue value)
        {
            inner.Write(value);
        }

        public void Save()
        {
            inner.Save();
        }
    }

    internal sealed class JsonStructuredValueAdapter<TValue>(JsonSerializerOptions? options)
        : IStructuredModSettingsValueAdapter<TValue>
    {
        public TValue Clone(TValue value)
        {
            var json = JsonSerializer.Serialize(value, options);
            return JsonSerializer.Deserialize<TValue>(json, options)!;
        }

        public string Serialize(TValue value)
        {
            return JsonSerializer.Serialize(value, options);
        }

        public bool TryDeserialize(string text, out TValue value)
        {
            try
            {
                value = JsonSerializer.Deserialize<TValue>(text, options)!;
                return true;
            }
            catch
            {
                value = default!;
                return false;
            }
        }
    }

    internal sealed class ListStructuredValueAdapter<TItem>(
        IStructuredModSettingsValueAdapter<TItem>? itemAdapter,
        JsonSerializerOptions? options)
        : IStructuredModSettingsValueAdapter<List<TItem>>
    {
        public List<TItem> Clone(List<TItem> value)
        {
            return itemAdapter == null ? value.ToList() : value.Select(itemAdapter.Clone).ToList();
        }

        public string Serialize(List<TItem> value)
        {
            return JsonSerializer.Serialize(value, options);
        }

        public bool TryDeserialize(string text, out List<TItem> value)
        {
            try
            {
                value = JsonSerializer.Deserialize<List<TItem>>(text, options) ?? [];
                return true;
            }
            catch
            {
                value = [];
                return false;
            }
        }
    }
}
