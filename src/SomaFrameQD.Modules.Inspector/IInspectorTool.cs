using System;
using SomaFrameQD.Framework;

namespace SomaFrameQD.Modules.Inspector
{
	public interface IInspectorTool : ITool
	{
	    event EventHandler SelectedObjectChanged;
        IInspectableObject SelectedObject { get; set; }
	}
}