using SomaFrameQD.Framework;

namespace SomaFrameQD.Modules.UndoRedo
{
    public interface IHistoryTool : ITool
    {
        IUndoRedoManager UndoRedoManager { get; set; }
    }
}