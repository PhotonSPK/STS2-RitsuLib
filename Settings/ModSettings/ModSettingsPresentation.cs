namespace STS2RitsuLib.Settings
{
    public readonly record struct ModSettingsChoiceOption<TValue>(TValue Value, ModSettingsText Label);

    public enum ModSettingsChoicePresentation
    {
        Stepper = 0,
        Dropdown = 1,
    }

    public enum ModSettingsButtonTone
    {
        Normal = 0,
        Accent = 1,
        Danger = 2,
    }
}
