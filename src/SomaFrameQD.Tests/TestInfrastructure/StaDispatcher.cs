using System.Threading.Tasks;
using System.Windows.Threading;

namespace SomaFrameQD.Tests.TestInfrastructure
{
    internal static class StaDispatcher
    {
        internal static Task DrainAsync()
        {
            return Dispatcher.CurrentDispatcher.InvokeAsync(
                delegate { },
                DispatcherPriority.Background).Task;
        }
    }
}
