using System;
using SomaFrameQD.Framework.Commands;

namespace SomaFrameQD.Modules.ErrorList.Commands
{
    [CommandDefinition]
    public class ToggleMessagesCommandDefinition : CommandDefinition
    {
        public const string CommandName = "ErrorList.ToggleMessages";

        public override string Name
        {
            get { return CommandName; }
        }

        public override string Text
        {
            get { return "[NotUsed]"; }
        }

        public override string ToolTip
        {
            get { return "[NotUsed]"; }
        }

        public override Uri IconSource
        {
            get { return new Uri("pack://application:,,,/SomaFrameQD.Modules.ErrorList;component/Resources/Message.png"); }
        }
    }
}