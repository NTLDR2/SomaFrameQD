using System.Collections.Generic;
using SomaFrameQD.Modules.Inspector.Inspectors;

namespace SomaFrameQD.Modules.Inspector
{
    public class InspectableObject : IInspectableObject
    {
        public IEnumerable<IInspector> Inspectors { get; set; }

        public InspectableObject(IEnumerable<IInspector> inspectors)
        {
            Inspectors = inspectors;
        }
    }
}