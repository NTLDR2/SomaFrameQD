using SomaFrameQD.Modules.UndoRedo;

namespace SomaFrameQD.Framework
{
	public interface IDocument : ILayoutItem
	{
        IUndoRedoManager UndoRedoManager { get; }
	}
}