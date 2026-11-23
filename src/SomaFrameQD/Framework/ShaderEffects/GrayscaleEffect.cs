using System.Windows;
using System.Windows.Media;

namespace SomaFrameQD.Framework.ShaderEffects
{
    // Compiled against DirectX Shader Model ps_2_0 (GrayscaleEffect.ps, header 0xFFFF0200) so disabled
    // menu and toolbar icons render identically in both hardware Direct3D9Ex and WPF software fallback
    // modes from Windows 7 SP1 x64 through Windows 11 x64.
    public class GrayscaleEffect : ShaderEffectBase<GrayscaleEffect>
    {
        public static readonly DependencyProperty InputProperty =
            RegisterPixelShaderSamplerProperty("Input", typeof(GrayscaleEffect), 0);

        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public GrayscaleEffect()
        {
            UpdateShaderValue(InputProperty);
        }
    }
}
