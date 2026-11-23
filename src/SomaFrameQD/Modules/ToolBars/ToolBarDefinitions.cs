using System.ComponentModel.Composition;
using SomaFrameQD.Framework.ToolBars;
using SomaFrameQD.Properties;

namespace SomaFrameQD.Modules.ToolBars
{
    internal static class ToolBarDefinitions
    {
        [Export]
        public static ToolBarDefinition StandardToolBar = new ToolBarDefinition(0, Resources.ToolBarStandard);
    }
}