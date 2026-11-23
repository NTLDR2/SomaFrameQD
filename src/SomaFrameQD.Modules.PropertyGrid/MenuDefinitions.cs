using System.ComponentModel.Composition;
using SomaFrameQD.Framework.Menus;
using SomaFrameQD.Modules.PropertyGrid.Commands;

namespace SomaFrameQD.Modules.PropertyGrid
{
    public static class MenuDefinitions
    {
        [Export]
        public static readonly MenuItemDefinition ViewPropertyGridMenuItem = new CommandMenuItemDefinition<ViewPropertyGridCommandDefinition>(
            MainMenu.MenuDefinitions.ViewPropertiesMenuGroup, 0);
    }
}
