using System.ComponentModel.Composition;
using System.Threading.Tasks;
using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Framework.Services;
using SomaFrameQD.Framework.Threading;

namespace SomaFrameQD.Modules.Shell.Commands
{
    [CommandHandler]
    public class ExitCommandHandler : CommandHandlerBase<ExitCommandDefinition>
    {
        private readonly IShell _shell;

        [ImportingConstructor]
        public ExitCommandHandler(IShell shell)
        {
            _shell = shell;
        }

        public override Task Run(Command command)
        {
            _shell.Close();
            return TaskUtility.Completed;
        }
    }
}