using SomaFrameQD.Framework;

namespace SomaFrameQD.Modules.PropertyGrid
{
	public interface IPropertyGrid : ITool
	{
		object SelectedObject { get; set; }
	}
}