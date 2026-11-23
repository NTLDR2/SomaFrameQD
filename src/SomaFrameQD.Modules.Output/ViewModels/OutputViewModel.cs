using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using SomaFrameQD.Framework;
using SomaFrameQD.Framework.Services;
using SomaFrameQD.Modules.Output.Properties;
using SomaFrameQD.Modules.Output.Views;

namespace SomaFrameQD.Modules.Output.ViewModels
{
    [Export(typeof(IOutput))]
    public class OutputViewModel : Tool, IOutput
    {
        private readonly object _syncRoot = new object();
        private readonly StringBuilder _stringBuilder;
        private readonly OutputWriter _writer;
        private IOutputView _view;
        private int _uiRefreshPending;

        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Bottom; }
        }

        public TextWriter Writer
        {
            get { return _writer; }
        }

        public OutputViewModel()
        {
            DisplayName = Resources.OutputDisplayName;
            _stringBuilder = new StringBuilder();
            _writer = new OutputWriter(this);
        }

        public void Clear()
        {
            lock (_syncRoot)
            {
                _stringBuilder.Clear();
            }

            var view = _view;
            if (view == null)
                return;

            var dispatcher = (view as DispatcherObject)?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                ScheduleViewRefresh();
                return;
            }

            Execute.OnUIThread(() =>
            {
                Interlocked.Exchange(ref _uiRefreshPending, 0);
                string snapshot = GetSnapshotText();
                if (snapshot.Length == 0)
                    view.Clear();
                else
                    view.SetText(snapshot);
            });
        }

        public void AppendLine(string text)
        {
            lock (_syncRoot)
            {
                if (!string.IsNullOrEmpty(text))
                    _stringBuilder.Append(text);
                _stringBuilder.Append(Environment.NewLine);
            }

            ScheduleViewRefresh();
        }

        public void Append(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            lock (_syncRoot)
            {
                _stringBuilder.Append(text);
            }

            ScheduleViewRefresh();
        }

        protected override void OnViewLoaded(object view)
        {
            _view = (IOutputView)view;
            _view.SetText(GetSnapshotText());
            _view.ScrollToEnd();
        }

        private string GetSnapshotText()
        {
            lock (_syncRoot)
            {
                return _stringBuilder.ToString();
            }
        }

        private void ScheduleViewRefresh()
        {
            var view = _view;
            if (view == null)
                return;

            if (Interlocked.CompareExchange(ref _uiRefreshPending, 1, 0) != 0)
                return;

            var dispatcher = (view as DispatcherObject)?.Dispatcher;
            if (dispatcher != null)
            {
                dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new System.Action(FlushToView));
            }
            else
            {
                Execute.OnUIThread(FlushToView);
            }
        }

        private void FlushToView()
        {
            // Reset pending flag before reading the locked snapshot so any concurrent append occurring
            // after GetSnapshotText() is guaranteed to schedule one follow-up UI refresh.
            Interlocked.Exchange(ref _uiRefreshPending, 0);

            var view = _view;
            if (view == null)
                return;

            view.SetText(GetSnapshotText());
        }
    }
}
