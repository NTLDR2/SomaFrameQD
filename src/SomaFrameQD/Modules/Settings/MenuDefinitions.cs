using System.ComponentModel.Composition;
using SomaFrameQD.Framework.Menus;
using SomaFrameQD.Modules.Settings.Commands;

namespace SomaFrameQD.Modules.Settings
{
    public static class MenuDefinitions
    {
        [Export]
        public static readonly MenuItemDefinition OpenSettingsMenuItem = new CommandMenuItemDefinition<OpenSettingsCommandDefinition>(
            MainMenu.MenuDefinitions.ToolsOptionsMenuGroup, 0);
    }
}
