using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Modules.CodeEditor.Properties;
using System;

namespace SomaFrameQD.Modules.CodeEditor.Commands
{
    [CommandDefinition]
    public class ShowEndOfLineCommandDefinition : CommandDefinition
    {
        public const string CommandName = "CodeEditor.ShowEndOfLine";

        public override string Name => CommandName;

        public override string Text => Resources.ShowEndOfLineCommandText;

        public override string ToolTip => Resources.ShowEndOfLineCommandToolTip;

        public override Uri IconSource => new Uri("pack://application:,,,/SomaFrameQD.Modules.CodeEditor;component/Resources/Icons/ShowEndOfLine.png");
    }
}
