using System.Threading.Tasks;
using SomaFrameQD.Framework.Services;

namespace SomaFrameQD.Demo.Modules.SampleBrowser
{
    public interface ISample
    {
        string Name { get; }
        Task Activate(IShell shell);
    }
}
