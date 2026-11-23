using System.ComponentModel.Composition;
using SomaFrameQD.Framework.Menus;
using SomaFrameQD.Modules.Inspector.Commands;

namespace SomaFrameQD.Modules.Inspector
{
    public static class MenuDefinitions
    {
        [Export]
        public static readonly MenuItemDefinition ViewInspectorMenuItem = new CommandMenuItemDefinition<ViewInspectorCommandDefinition>(
            MainMenu.MenuDefinitions.ViewPropertiesMenuGroup, 1);
    }
}
