using System.ComponentModel.Composition;
using SomaFrameQD.Framework.Menus;
using SomaFrameQD.Modules.Toolbox.Commands;

namespace SomaFrameQD.Modules.Toolbox
{
    public static class MenuDefinitions
    {
        [Export]
        public static readonly MenuItemDefinition ViewToolboxMenuItem = new CommandMenuItemDefinition<ViewToolboxCommandDefinition>(
            MainMenu.MenuDefinitions.ViewToolsMenuGroup, 4);
    }
}
