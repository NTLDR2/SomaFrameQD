using System;

namespace SomaFrameQD.Framework.Commands
{
    public abstract class CommandListDefinition : CommandDefinitionBase
    {
        public override sealed string Text
        {
            get { return "[NotUsed]"; }
        }

        public override sealed string ToolTip
        {
            get { return "[NotUsed]"; }
        }

        public override sealed Uri IconSource
        {
            get { return null; }
        }

        public override sealed bool IsList
        {
            get { return true; }
        }
    }
}