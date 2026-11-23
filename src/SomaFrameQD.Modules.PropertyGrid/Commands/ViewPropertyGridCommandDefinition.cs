using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Modules.PropertyGrid.Properties;

namespace SomaFrameQD.Modules.PropertyGrid.Commands
{
    [CommandDefinition]
    public class ViewPropertyGridCommandDefinition : CommandDefinition
    {
        public const string CommandName = "View.PropertiesWindow";

        public override string Name
        {
            get { return CommandName; }
        }

        public override string Text
        {
            get { return Resources.ViewPropertyGridCommandText; }
        }

        public override string ToolTip
        {
            get { return Resources.ViewPropertyGridCommandToolTip; }
        }
    }
}