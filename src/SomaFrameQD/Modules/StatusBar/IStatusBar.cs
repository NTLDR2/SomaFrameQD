using System.Windows;
using Caliburn.Micro;
using SomaFrameQD.Modules.StatusBar.ViewModels;

namespace SomaFrameQD.Modules.StatusBar
{
    public interface IStatusBar
    {
        IObservableCollection<StatusBarItemViewModel> Items { get; }

        void AddItem(string message, GridLength width);
    }
}
