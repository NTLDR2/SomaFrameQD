using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Caliburn.Micro;
using SomaFrameQD.Demo.Modules.FilterDesigner.ViewModels;
using SomaFrameQD.Demo.Modules.SampleBrowser;
using SomaFrameQD.Framework.Services;
using SomaFrameQD.Modules.Inspector;
using SomaFrameQD.Modules.Toolbox;

namespace SomaFrameQD.Demo.Modules.FilterDesigner
{
    [Export(typeof(ISample))]
    public class Sample : ISample
    {
        public string Name => "Filter Designer";

        public async Task Activate(IShell shell)
        {
            await shell.OpenDocumentAsync(IoC.Get<GraphViewModel>());
            await shell.ShowToolAsync<IInspectorTool>();
            await shell.ShowToolAsync<IToolbox>();
        }
    }
}
