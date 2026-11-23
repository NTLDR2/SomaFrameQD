using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Modules.CodeEditor.Properties;
using System;

namespace SomaFrameQD.Modules.CodeEditor.Commands
{
    [CommandDefinition]
    public class ShowTabsCommandDefinition : CommandDefinition
    {
        public const string CommandName = "CodeEditor.ShowTabs";

        public override string Name => CommandName;

        public override string Text => Resources.ShowTabsCommandText;

        public override string ToolTip => Resources.ShowTabsCommandToolTip;

        public override Uri IconSource => new Uri("pack://application:,,,/SomaFrameQD.Modules.CodeEditor;component/Resources/Icons/ShowTabs.png");
    }
}
