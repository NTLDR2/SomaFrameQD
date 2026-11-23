using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Caliburn.Micro;
using SomaFrameQD.Demo.Modules.Home.ViewModels;
using SomaFrameQD.Framework;
using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Framework.Services;
using SomaFrameQD.Framework.Threading;

namespace SomaFrameQD.Demo.Modules.Home.Commands
{
    [CommandHandler]
    public class ViewHelixCommandHandler : CommandHandlerBase<ViewHelixCommandDefinition>
    {
        private readonly IShell _shell;

        [ImportingConstructor]
        public ViewHelixCommandHandler(IShell shell)
        {
            _shell = shell;
        }

        public override Task Run(Command command)
        {
            return _shell.OpenDocumentAsync((IDocument) IoC.GetInstance(typeof(HelixViewModel), null));
        }
    }
}
