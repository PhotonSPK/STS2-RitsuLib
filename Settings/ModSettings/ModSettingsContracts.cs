using STS2RitsuLib.Utils.Persistence;

namespace STS2RitsuLib.Settings
{
    public interface IModSettingsBinding
    {
        string ModId { get; }
        string DataKey { get; }
        SaveScope Scope { get; }
        void Save();
    }

    public interface IModSettingsValueBinding<TValue> : IModSettingsBinding
    {
        TValue Read();
        void Write(TValue value);
    }

    public interface IDefaultModSettingsValueBinding<TValue> : IModSettingsValueBinding<TValue>
    {
        TValue CreateDefaultValue();
    }

    public interface ITransientModSettingsBinding : IModSettingsBinding
    {
    }

    public interface IStructuredModSettingsValueAdapter<TValue>
    {
        TValue Clone(TValue value);
        string Serialize(TValue value);
        bool TryDeserialize(string text, out TValue value);
    }

    public interface IStructuredModSettingsValueBinding<TValue> : IModSettingsValueBinding<TValue>
    {
        IStructuredModSettingsValueAdapter<TValue> Adapter { get; }
    }
}
