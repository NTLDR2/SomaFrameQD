using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Modules.Output.Properties;

namespace SomaFrameQD.Modules.Output.Commands
{
    [CommandDefinition]
    public class ViewOutputCommandDefinition : CommandDefinition
    {
        public const string CommandName = "View.Output";

        public override string Name
        {
            get { return CommandName; }
        }

        public override string Text
        {
            get { return Resources.ViewOutputCommandText; }
        }

        public override string ToolTip
        {
            get { return Resources.ViewOutputCommandToolTip; }
        }
    }
}