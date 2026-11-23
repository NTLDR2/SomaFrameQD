using System;
using System.Windows;
using System.Windows.Media;

namespace SomaFrameQD.Modules.MainWindow.Views
{
    public partial class MainWindowView
    {
        private const double MinimumClampedWindowDimension = 320.0;

        public MainWindowView()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Under System-DPI awareness (Q2 = 2A), system DPI is constant for the Windows login session.
            // Use pixel-sharp Display ClearType hinting at 100% (96 DPI) and smooth Ideal metrics on High-DPI displays.
            double dpiScaleX = 1.0;
            var presentationSource = PresentationSource.FromVisual(this);
            if (presentationSource?.CompositionTarget != null)
            {
                dpiScaleX = presentationSource.CompositionTarget.TransformToDevice.M11;
            }

            TextOptions.SetTextFormattingMode(this, SelectTextFormattingModeForDpiScale(dpiScaleX));
            TextOptions.SetTextRenderingMode(this, TextRenderingMode.Auto);

            // Prevent initial normal-state window bounds (1000x800 default) from overflowing smaller displays
            // such as 1024x768 Windows 7 SP1 VMs/consoles where the taskbar reduces WorkArea height to ~728px.
            if (WindowState == WindowState.Normal)
            {
                var workArea = SystemParameters.WorkArea;
                var clamped = ClampSizeToWorkArea(new Size(Width, Height), workArea.Size);
                bool resized = false;

                if (!double.IsNaN(clamped.Width) && clamped.Width < Width)
                {
                    Width = clamped.Width;
                    resized = true;
                }

                if (!double.IsNaN(clamped.Height) && clamped.Height < Height)
                {
                    Height = clamped.Height;
                    resized = true;
                }

                if (resized || Left < workArea.Left || Top < workArea.Top ||
                    Left + Width > workArea.Right || Top + Height > workArea.Bottom)
                {
                    Left = workArea.Left + Math.Max(0.0, (workArea.Width - Width) / 2.0);
                    Top = workArea.Top + Math.Max(0.0, (workArea.Height - Height) / 2.0);
                }
            }
        }

        internal static TextFormattingMode SelectTextFormattingModeForDpiScale(double dpiScale)
        {
            return dpiScale <= 1.05
                ? TextFormattingMode.Display
                : TextFormattingMode.Ideal;
        }

        internal static Size ClampSizeToWorkArea(Size requestedSize, Size workAreaSize)
        {
            double width = requestedSize.Width;
            double height = requestedSize.Height;

            if (!double.IsNaN(width) && !double.IsInfinity(width) && workAreaSize.Width > 0.0 && width > workAreaSize.Width)
            {
                width = Math.Max(Math.Min(MinimumClampedWindowDimension, workAreaSize.Width), workAreaSize.Width);
            }

            if (!double.IsNaN(height) && !double.IsInfinity(height) && workAreaSize.Height > 0.0 && height > workAreaSize.Height)
            {
                height = Math.Max(Math.Min(MinimumClampedWindowDimension, workAreaSize.Height), workAreaSize.Height);
            }

            return new Size(width, height);
        }
    }
}
