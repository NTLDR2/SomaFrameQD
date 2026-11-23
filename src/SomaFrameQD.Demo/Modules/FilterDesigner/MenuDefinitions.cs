using System.ComponentModel.Composition;
using SomaFrameQD.Demo.Modules.FilterDesigner.Commands;
using SomaFrameQD.Framework.Menus;

namespace SomaFrameQD.Demo.Modules.FilterDesigner
{
    public static class MenuDefinitions
    {
        [Export]
        public static readonly MenuItemDefinition OpenGraphMenuItem = new CommandMenuItemDefinition<OpenGraphCommandDefinition>(
            SomaFrameQD.Modules.MainMenu.MenuDefinitions.FileNewOpenMenuGroup, 2);
    }
}
