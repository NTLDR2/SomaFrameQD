using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Properties;

namespace SomaFrameQD.Modules.Shell.Commands
{
    [CommandDefinition]
    public class ViewFullScreenCommandDefinition : CommandDefinition
    {
        public const string CommandName = "View.FullScreen";

        public override string Name
        {
            get { return CommandName; }
        }

        public override string Text
        {
            get { return Resources.ViewFullScreenCommandText; }
        }

        public override string ToolTip
        {
            get { return Resources.ViewFullScreenCommandToolTip; }
        }

        public override Uri IconSource
        {
            get { return new Uri("pack://application:,,,/SomaFrameQD;component/Resources/Icons/FullScreen.png"); }
        }

        [Export]
        public static CommandKeyboardShortcut KeyGesture = new CommandKeyboardShortcut<ViewFullScreenCommandDefinition>(new KeyGesture(Key.Enter, ModifierKeys.Shift | ModifierKeys.Alt));
    }
}