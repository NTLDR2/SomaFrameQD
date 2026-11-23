using System.Threading.Tasks;
using Caliburn.Micro;
using SomaFrameQD.Demo.Modules.Home.ViewModels;
using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Framework.Results;

namespace SomaFrameQD.Demo.Modules.Home.Commands
{
    [CommandHandler]
    public class ViewHomeCommandHandler : CommandHandlerBase<ViewHomeCommandDefinition>
    {
        public override async Task Run(Command command)
        {
            await Show.Document<HomeViewModel>().ExecuteAsync();
        }
    }
}
