using System.ComponentModel.Composition;
using SomaFrameQD.Framework.Menus;
using SomaFrameQD.Modules.ErrorList.Commands;

namespace SomaFrameQD.Modules.ErrorList
{
    public static class MenuDefinitions
    {
        [Export]
        public static readonly MenuItemDefinition ViewErrorListMenuItem = new CommandMenuItemDefinition<ViewErrorListCommandDefinition>(
            MainMenu.MenuDefinitions.ViewToolsMenuGroup, 0);
    }
}
