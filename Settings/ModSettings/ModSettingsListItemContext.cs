using Godot;

namespace STS2RitsuLib.Settings
{
    public sealed class ModSettingsListItemContext<TItem>
    {
        private readonly Action? _duplicate;
        private readonly Action? _moveDown;
        private readonly Action? _moveUp;
        private readonly Action _remove;
        private readonly Action _requestRefresh;
        private readonly ModSettingsUiContext _uiContext;
        private readonly Action<TItem> _update;

        internal ModSettingsListItemContext(
            ModSettingsUiContext uiContext,
            IModSettingsValueBinding<TItem> binding,
            int index,
            int itemCount,
            TItem item,
            Action<TItem> update,
            Action? moveUp,
            Action? moveDown,
            Action? duplicate,
            Action remove,
            Action requestRefresh)
        {
            _uiContext = uiContext;
            Binding = binding;
            Index = index;
            ItemCount = itemCount;
            Item = item;
            _update = update;
            _moveUp = moveUp;
            _moveDown = moveDown;
            _duplicate = duplicate;
            _remove = remove;
            _requestRefresh = requestRefresh;
        }

        public int Index { get; }
        public int ItemCount { get; }
        public TItem Item { get; }
        public bool CanMoveUp => Index > 0;
        public bool CanMoveDown => Index < ItemCount - 1;
        public IModSettingsValueBinding<TItem> Binding { get; }

        public bool SupportsStructuredClipboard => Binding is IStructuredModSettingsValueBinding<TItem>;

        public void Update(TItem item)
        {
            _update(item);
        }

        public void Remove()
        {
            _remove();
        }

        public void MoveUp()
        {
            _moveUp?.Invoke();
        }

        public void MoveDown()
        {
            _moveDown?.Invoke();
        }

        public void Duplicate()
        {
            _duplicate?.Invoke();
        }

        public void RequestRefresh()
        {
            _requestRefresh();
        }

        public bool TryCopyToClipboard(ModSettingsClipboardScope scope = ModSettingsClipboardScope.Self)
        {
            if (Binding is not IStructuredModSettingsValueBinding<TItem> structured)
                return false;

            ModSettingsClipboardOperations.InvokeCopy(Binding, scope, structured.Adapter, Item);
            return true;
        }

        public bool CanPasteFromClipboard()
        {
            if (Binding is not IStructuredModSettingsValueBinding<TItem> structured)
                return false;

            return ModSettingsClipboardOperations.CanPasteBindingValue(Binding, structured.Adapter);
        }

        public bool TryPasteFromClipboard()
        {
            if (Binding is not IStructuredModSettingsValueBinding<TItem> structured)
                return false;

            if (!ModSettingsClipboardOperations.TryPasteBindingValue(Binding, structured.Adapter, out var value,
                    out var failureReason))
            {
                _uiContext.NotifyPasteFailure(failureReason);
                return false;
            }

            Update(value);
            return true;
        }

        public IModSettingsValueBinding<TValue> Project<TValue>(
            string dataKey,
            Func<TItem, TValue> getter,
            Func<TItem, TValue, TItem> setter,
            IStructuredModSettingsValueAdapter<TValue>? adapter = null)
        {
            return ModSettingsBindings.Project(Binding, dataKey, getter, setter, adapter);
        }

        public Control CreateEntry(ModSettingsEntryDefinition entry)
        {
            return entry.CreateControl(_uiContext);
        }

        public Control CreateListEditor<TChild>(
            string id,
            ModSettingsText label,
            IModSettingsValueBinding<List<TChild>> binding,
            Func<TChild> createItem,
            Func<TChild, ModSettingsText> itemLabel,
            Func<TChild, ModSettingsText?>? itemDescription = null,
            Func<ModSettingsListItemContext<TChild>, Control>? itemEditorFactory = null,
            ModSettingsText? addButtonText = null,
            ModSettingsText? description = null)
        {
            return CreateEntry(new ListModSettingsEntryDefinition<TChild>(
                id,
                label,
                binding,
                createItem,
                itemLabel,
                itemDescription,
                itemEditorFactory,
                null,
                addButtonText ?? ModSettingsText.I18N(ModSettingsLocalization.Instance, "button.add", "Add"),
                description));
        }
    }
}
