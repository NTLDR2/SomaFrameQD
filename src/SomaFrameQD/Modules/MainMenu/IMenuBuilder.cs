using SomaFrameQD.Framework.Menus;
using SomaFrameQD.Modules.MainMenu.Models;

namespace SomaFrameQD.Modules.MainMenu
{
    public interface IMenuBuilder
    {
        void BuildMenuBar(MenuBarDefinition menuBarDefinition, MenuModel result);
    }
}