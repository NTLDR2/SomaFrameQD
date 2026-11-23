using System.ComponentModel.Composition;
using SomaFrameQD.Framework.Menus;
using SomaFrameQD.Modules.Output.Commands;

namespace SomaFrameQD.Modules.Output
{
    public static class MenuDefinitions
    {
        [Export]
        public static readonly MenuItemDefinition ViewOutputMenuItem = new CommandMenuItemDefinition<ViewOutputCommandDefinition>(
            MainMenu.MenuDefinitions.ViewToolsMenuGroup, 1);
    }
}
