using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Caliburn.Micro;
using SomaFrameQD.Demo.Modules.SampleBrowser.ViewModels;
using SomaFrameQD.Framework;

namespace SomaFrameQD.Demo.Modules.SampleBrowser
{
    [Export(typeof(IModule))]
    public class Module : ModuleBase
    {
        public override Task PostInitializeAsync()
        {
            return Shell.OpenDocumentAsync(IoC.Get<SampleBrowserViewModel>());
        }
    }
}
