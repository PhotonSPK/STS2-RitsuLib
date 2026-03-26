using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using STS2RitsuLib.Utils;

namespace STS2RitsuLib.Settings
{
    internal static class ModSettingsUiResources
    {
        public static Theme SettingsLineTheme =>
            PreloadManager.Cache.GetAsset<Theme>("res://themes/settings_screen_line_header.tres");

        public static Font KreonRegular =>
            PreloadManager.Cache.GetAsset<Font>("res://themes/kreon_regular_shared.tres");

        public static Font KreonBold =>
            PreloadManager.Cache.GetAsset<Font>("res://themes/kreon_bold_shared.tres");

        public static Font KreonButton =>
            PreloadManager.Cache.GetAsset<Font>("res://themes/kreon_bold_glyph_space_two.tres");

        public static PackedScene SelectionReticleScene =>
            PreloadManager.Cache.GetScene(SceneHelper.GetScenePath("ui/selection_reticle"));

        public static Texture2D SettingsButtonTexture =>
            PreloadManager.Cache.GetAsset<Texture2D>("res://images/ui/reward_screen/reward_skip_button.png");

        public static ShaderMaterial CreateToneMaterial(ModSettingsButtonTone tone)
        {
            return tone switch
            {
                ModSettingsButtonTone.Accent => MaterialUtils.CreateHsvShaderMaterial(0.82f, 1.4f, 0.8f),
                ModSettingsButtonTone.Danger => MaterialUtils.CreateHsvShaderMaterial(0.45f, 1.5f, 0.8f),
                _ => MaterialUtils.CreateHsvShaderMaterial(0.61f, 1.6f, 1.3f),
            };
        }

        public static Color GetToneOutlineColor(ModSettingsButtonTone tone)
        {
            return tone switch
            {
                ModSettingsButtonTone.Accent => new(0.1274f, 0.26f, 0.14066f),
                ModSettingsButtonTone.Danger => new(0.29f, 0.14703f, 0.1421f),
                _ => new(0.2f, 0.1575f, 0.098f),
            };
        }
    }
}
