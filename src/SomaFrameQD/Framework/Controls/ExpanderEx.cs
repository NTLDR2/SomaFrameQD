using System.Windows;
using System.Windows.Controls;

namespace SomaFrameQD.Framework.Controls
{
    public class ExpanderEx : Expander
    {
        static ExpanderEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpanderEx),
                new FrameworkPropertyMetadata(typeof(ExpanderEx)));
        } 
    }
}