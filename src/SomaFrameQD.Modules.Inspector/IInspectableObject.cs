using System.Collections.Generic;
using SomaFrameQD.Modules.Inspector.Inspectors;

namespace SomaFrameQD.Modules.Inspector
{
    public interface IInspectableObject
    {
        IEnumerable<IInspector> Inspectors { get; }
    }
}