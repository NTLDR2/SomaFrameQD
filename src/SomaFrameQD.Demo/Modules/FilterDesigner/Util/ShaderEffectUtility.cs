using System;
using System.IO;
using System.Windows.Media.Effects;

namespace SomaFrameQD.Demo.Modules.FilterDesigner.Util
{
    internal static class ShaderEffectUtility
    {
        public static PixelShader GetPixelShader(string name)
        {
            var shaderPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Modules",
                "FilterDesigner",
                "ShaderEffects",
                name + ".ps");

            var pixelShader = new PixelShader
            {
                UriSource = new Uri(shaderPath, UriKind.Absolute),
                ShaderRenderMode = ShaderRenderMode.Auto
            };

            if (pixelShader.CanFreeze)
            {
                pixelShader.Freeze();
            }

            return pixelShader;
        }
    }
}
