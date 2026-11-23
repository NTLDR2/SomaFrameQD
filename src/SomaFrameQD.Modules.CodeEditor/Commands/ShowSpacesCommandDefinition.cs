using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Modules.CodeEditor.Properties;
using System;

namespace SomaFrameQD.Modules.CodeEditor.Commands
{
    [CommandDefinition]
    public class ShowSpacesCommandDefinition : CommandDefinition
    {
        public const string CommandName = "CodeEditor.ShowSpaces";

        public override string Name => CommandName;

        public override string Text => Resources.ShowSpacesCommandText;

        public override string ToolTip => Resources.ShowSpacesCommandToolTip;

        public override Uri IconSource => new Uri("pack://application:,,,/SomaFrameQD.Modules.CodeEditor;component/Resources/Icons/ShowSpaces.png");
    }
}
