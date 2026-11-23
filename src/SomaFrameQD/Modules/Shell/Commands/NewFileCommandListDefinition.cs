using SomaFrameQD.Framework.Commands;

namespace SomaFrameQD.Modules.Shell.Commands
{
    [CommandDefinition]
    public class NewFileCommandListDefinition : CommandListDefinition
    {
        public const string CommandName = "File.NewFile";

        public override string Name
        {
            get { return CommandName; }
        }
    }
}