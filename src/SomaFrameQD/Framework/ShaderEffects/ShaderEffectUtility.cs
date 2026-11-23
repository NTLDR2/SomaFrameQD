using System;
using System.Windows.Media.Effects;

namespace SomaFrameQD.Framework.ShaderEffects
{
    internal static class ShaderEffectUtility
    {
        public static PixelShader GetPixelShader(string name)
        {
            // ShaderRenderMode.Auto runs ps_2_0 bytecode on GPU Direct3D9Ex when available (Win7 Aero -> Win11)
            // and seamlessly falls back to WPF's SSE2 CPU software rasterizer on Win7 Basic/Classic, VMs, or RDP.
            // Freezing the cached PixelShader prevents static Freezable.Changed event roots from retaining
            // short-lived GrayscaleEffect instances across menu/toolbar template recycling.
            var pixelShader = new PixelShader
            {
                UriSource = new Uri(@"pack://application:,,,/SomaFrameQD;component/Framework/ShaderEffects/" + name + ".ps"),
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
