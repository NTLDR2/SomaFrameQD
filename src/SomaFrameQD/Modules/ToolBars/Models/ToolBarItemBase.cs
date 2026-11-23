using Caliburn.Micro;

namespace SomaFrameQD.Modules.ToolBars.Models
{
	public class ToolBarItemBase : PropertyChangedBase
	{
		public static ToolBarItemBase Separator
		{
			get { return new ToolBarItemSeparator(); }
		}

		public virtual string Name
		{
			get { return "-"; }
		}
	}
}