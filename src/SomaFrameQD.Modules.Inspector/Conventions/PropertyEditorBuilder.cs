using System.ComponentModel;
using SomaFrameQD.Modules.Inspector.Inspectors;

namespace SomaFrameQD.Modules.Inspector.Conventions
{
    public abstract class PropertyEditorBuilder
    {
        public abstract bool IsApplicable(PropertyDescriptor propertyDescriptor);
        public abstract IEditor BuildEditor(PropertyDescriptor propertyDescriptor);
    }
}