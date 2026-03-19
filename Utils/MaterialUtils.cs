using Godot;

namespace STS2RitsuLib.Utils
{
    public class MaterialUtils
    {
        private static Shader? GameHsvShader => (Shader?)GD.Load<Shader>("res://shaders/hsv.gdshader")?.Duplicate();

        public static ShaderMaterial CreateHsvShaderMaterial(float h, float s, float v)
        {
            var shader = GameHsvShader;
            if (shader == null)
                throw new("Failed to load HSV shader.");

            var material = new ShaderMaterial
            {
                Shader = shader,
            };

            material.SetShaderParameter("h", h);
            material.SetShaderParameter("s", s);
            material.SetShaderParameter("v", v);

            return material;
        }
    }
}
