namespace SomaFrameQD.Modules.UndoRedo
{
    public interface IUndoableAction
    {
        string Name { get; }

        void Execute();
        void Undo();
    }
}