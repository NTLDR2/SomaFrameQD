using System.ComponentModel.Composition;
using System.Threading.Tasks;
using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Framework.Services;

namespace SomaFrameQD.Modules.Output.Commands
{
    [CommandHandler]
    public class ViewOutputCommandHandler : CommandHandlerBase<ViewOutputCommandDefinition>
    {
        private readonly IShell _shell;

        [ImportingConstructor]
        public ViewOutputCommandHandler(IShell shell)
        {
            _shell = shell;
        }

        public override Task Run(Command command)
        {
            return _shell.ShowToolAsync<IOutput>();
        }
    }
}