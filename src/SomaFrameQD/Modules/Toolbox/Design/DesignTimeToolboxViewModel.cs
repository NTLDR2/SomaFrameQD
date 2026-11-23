using SomaFrameQD.Modules.Toolbox.ViewModels;

namespace SomaFrameQD.Modules.Toolbox.Design
{
    public class DesignTimeToolboxViewModel : ToolboxViewModel
    {
        public DesignTimeToolboxViewModel()
            : base(null, null)
        {
            Items.Add(new ToolboxItemViewModel(new Models.ToolboxItem { Name = "Foo", Category = "General" }));
            Items.Add(new ToolboxItemViewModel(new Models.ToolboxItem { Name = "Bar", Category = "General" }));
            Items.Add(new ToolboxItemViewModel(new Models.ToolboxItem { Name = "Baz", Category = "Misc" }));
        }
    }
}