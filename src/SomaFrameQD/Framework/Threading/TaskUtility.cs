using System.Threading.Tasks;

namespace SomaFrameQD.Framework.Threading
{
    public class TaskUtility
    {
        public static readonly Task Completed = Task.FromResult(true);
    }
}