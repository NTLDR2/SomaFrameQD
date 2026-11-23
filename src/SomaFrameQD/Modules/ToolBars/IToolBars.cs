using Caliburn.Micro;

namespace SomaFrameQD.Modules.ToolBars
{
    public interface IToolBars
    {
        IObservableCollection<IToolBar> Items {get;}
        bool Visible { get; set; }
    }
}