using System.ComponentModel.Composition;
using System.Threading.Tasks;
using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Framework.Services;

namespace SomaFrameQD.Modules.ErrorList.Commands
{
    [CommandHandler]
    public class ViewErrorListCommandHandler : CommandHandlerBase<ViewErrorListCommandDefinition>
    {
        private readonly IShell _shell;

        [ImportingConstructor]
        public ViewErrorListCommandHandler(IShell shell)
        {
            _shell = shell;
        }

        public override Task Run(Command command)
        {
            return _shell.ShowToolAsync<IErrorList>();
        }
    }
}