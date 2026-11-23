using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Properties;

namespace SomaFrameQD.Modules.Toolbox.Commands
{
    [CommandDefinition]
    public class ViewToolboxCommandDefinition : CommandDefinition
    {
        public const string CommandName = "View.Toolbox";

        public override string Name
        {
            get { return CommandName; }
        }

        public override string Text
        {
            get { return Resources.ViewToolboxCommandText; }
        }

        public override string ToolTip
        {
            get { return Resources.ViewToolboxCommandToolTip; }
        }
    }
}