using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Properties;

namespace SomaFrameQD.Modules.Shell.Commands
{
    [CommandDefinition]
    public class SaveFileAsCommandDefinition : CommandDefinition
    {
        public const string CommandName = "File.SaveFileAs";

        public override string Name
        {
            get { return CommandName; }
        }

        public override string Text
        {
            get { return Resources.FileSaveAsCommandText; }
        }

        public override string ToolTip
        {
            get { return Resources.FileSaveAsCommandToolTip; }
        }
    }
}