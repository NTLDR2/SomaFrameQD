using System;
using System.ComponentModel;
using SomaFrameQD.Modules.Inspector.Inspectors;

namespace SomaFrameQD.Modules.Inspector.Conventions
{
    public class EnumPropertyEditorBuilder : PropertyEditorBuilder
    {
        public override bool IsApplicable(PropertyDescriptor propertyDescriptor)
        {
            return typeof(Enum).IsAssignableFrom(propertyDescriptor.PropertyType);
        }

        public override IEditor BuildEditor(PropertyDescriptor propertyDescriptor)
        {
            return new EnumEditorViewModel(propertyDescriptor.PropertyType);
        }
    }
}