using System.ComponentModel.Composition;
using System.Threading.Tasks;
using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Framework.Services;

namespace SomaFrameQD.Modules.UndoRedo.Commands
{
    [CommandHandler]
    public class ViewHistoryCommandHandler : CommandHandlerBase<ViewHistoryCommandDefinition>
    {
        private readonly IShell _shell;

        [ImportingConstructor]
        public ViewHistoryCommandHandler(IShell shell)
        {
            _shell = shell;
        }

        public override Task Run(Command command)
        {
            return _shell.ShowToolAsync<IHistoryTool>();
        }
    }
}