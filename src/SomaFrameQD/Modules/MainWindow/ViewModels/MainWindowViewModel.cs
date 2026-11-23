using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Framework.Services;
using SomaFrameQD.Properties;

namespace SomaFrameQD.Modules.MainWindow.ViewModels
{
    [Export(typeof(IMainWindow))]
    public class MainWindowViewModel : Conductor<IShell>, IMainWindow, IPartImportsSatisfiedNotification
    {
#pragma warning disable 649
        [Import]
        private IShell _shell;

        [Import]
        private IResourceManager _resourceManager;

        [Import]
        private ICommandKeyGestureService _commandKeyGestureService;
#pragma warning restore 649

        private WindowState _windowState = WindowState.Normal;
        public WindowState WindowState
        {
            get { return _windowState; }
            set
            {
                _windowState = value;
                NotifyOfPropertyChange(() => WindowState);
            }
        }

        private double _width = GetInitialWorkAreaClampedDimension(1000.0, isWidth: true);
        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                NotifyOfPropertyChange(() => Width);
            }
        }

        private double _height = GetInitialWorkAreaClampedDimension(800.0, isWidth: false);
        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                NotifyOfPropertyChange(() => Height);
            }
        }

        private static double GetInitialWorkAreaClampedDimension(double defaultLength, bool isWidth)
        {
            var workArea = SystemParameters.WorkArea;
            double available = isWidth ? workArea.Width : workArea.Height;
            return available > 0.0 && defaultLength > available
                ? available
                : defaultLength;
        }

        private string _title = Resources.MainWindowDefaultTitle;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        private ImageSource _icon;
        public ImageSource Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                NotifyOfPropertyChange(() => Icon);
            }
        }

        public IShell Shell
        {
            get { return _shell; }
        }

        public Task ShellActivationTask { get; private set; } = Task.CompletedTask;

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            if (_icon == null)
                _icon = _resourceManager.GetBitmap("Resources/Icons/SomaFrameQD-32.png");
            ShellActivationTask = Execute.OnUIThreadAsync(
                () => ActivateItemAsync(_shell, CancellationToken.None));
        }

        protected override void OnViewLoaded(object view)
        {
            _commandKeyGestureService.BindKeyGestures((UIElement) view);
            base.OnViewLoaded(view);
        }
    }
}
