using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Caliburn.Micro;
using SomaFrameQD.Demo.Modules.FilterDesigner.ViewModels;
using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Framework.Services;
using SomaFrameQD.Modules.Inspector;

namespace SomaFrameQD.Demo.Modules.FilterDesigner.Commands
{
    [CommandHandler]
    public class OpenGraphCommandHandler : CommandHandlerBase<OpenGraphCommandDefinition>
    {
        private readonly IShell _shell;

        [ImportingConstructor]
        public OpenGraphCommandHandler(IShell shell)
        {
            _shell = shell;
        }

        public override Task Run(Command command)
        {
            return _shell.OpenDocumentAsync(new GraphViewModel(IoC.Get<IInspectorTool>()));
        }
    }
}
