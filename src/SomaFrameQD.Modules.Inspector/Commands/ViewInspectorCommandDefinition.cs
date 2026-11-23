using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Modules.Inspector.Properties;

namespace SomaFrameQD.Modules.Inspector.Commands
{
    [CommandDefinition]
    public class ViewInspectorCommandDefinition : CommandDefinition
    {
        public const string CommandName = "View.Inspector";

        public override string Name
        {
            get { return CommandName; }
        }

        public override string Text
        {
            get { return Resources.ViewInspectorCommandText; }
        }

        public override string ToolTip
        {
            get { return Resources.ViewInspectorCommandToolTip; }
        }
    }
}