using System.Threading.Tasks;
using Caliburn.Micro;
using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Framework.Results;

namespace SomaFrameQD.Modules.Toolbox.Commands
{
    [CommandHandler]
    public class ViewToolboxCommandHandler : CommandHandlerBase<ViewToolboxCommandDefinition>
    {
        public override async Task Run(Command command)
        {
            await Show.Tool<IToolbox>().ExecuteAsync();
        }
    }
}
