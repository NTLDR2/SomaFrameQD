using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SomaFrameQD.Modules.Inspector.Util;
using SomaFrameQD.Modules.Inspector.Win32;
using Color = System.Windows.Media.Color;

namespace SomaFrameQD.Modules.Inspector.Controls
{
    public class ScreenColorPicker : Control
    {
        static ScreenColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ScreenColorPicker),
                new FrameworkPropertyMetadata(typeof(ScreenColorPicker)));
        }

        public event EventHandler PickingStarted;
        public event EventHandler PickingCancelled;
        public event EventHandler<ColorEventArgs> ColorHovered;
        public event EventHandler<ColorEventArgs> ColorPicked;

        private readonly DispatcherTimer _timer;
        private Bitmap _bitmap;
        private Rectangle _virtualScreenBounds;
        private bool _endingCapture;

        public ScreenColorPicker()
        {
            _timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 50)
            };
            _timer.Tick += OnTimerTick;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            DisposeBitmap();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (IsMouseCaptured)
                RaiseColorHovered(GetEventArgs());
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                ReleaseMouseCapture();
            base.OnPreviewKeyDown(e);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            _timer.Stop();
            DisposeBitmap();

            if (!_endingCapture)
                RaisePickingCancelled(EventArgs.Empty);

            base.OnLostMouseCapture(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (!IsMouseCaptured)
            {
                DisposeBitmap();
                _virtualScreenBounds = ScreenShotUtility.GetVirtualScreenPhysicalBounds();

                try
                {
                    _bitmap = ScreenShotUtility.Take();
                }
                catch (Win32Exception)
                {
                    DisposeBitmap();
                    base.OnMouseDown(e);
                    return;
                }

                // Ensure both keyboard focus (for Escape cancellation) and mouse capture succeed;
                // otherwise release the GDI+ desktop bitmap immediately so HBITMAP handles never leak.
                if (Focus() && CaptureMouse())
                {
                    RaisePickingStarted(EventArgs.Empty);
                    _timer.Start();
                }
                else
                {
                    DisposeBitmap();
                }
            }
            else
            {
                RaiseColorPicked(GetEventArgs());

                _endingCapture = true;
                try
                {
                    ReleaseMouseCapture();
                }
                finally
                {
                    _endingCapture = false;
                }
            }

            base.OnMouseDown(e);
        }

        private ColorEventArgs GetEventArgs()
        {
            if (_bitmap != null &&
                NativeMethods.GetCursorPos(out var cursorPosition) &&
                TrySamplePixel(_bitmap, _virtualScreenBounds, cursorPosition.X, cursorPosition.Y, out var sampledColor))
            {
                return new ColorEventArgs(sampledColor);
            }

            return new ColorEventArgs(Colors.Black);
        }

        public static bool TrySamplePixel(
            Bitmap bitmap,
            Rectangle virtualScreenBounds,
            int physicalCursorX,
            int physicalCursorY,
            out Color color)
        {
            if (bitmap != null)
            {
                int bitmapX = physicalCursorX - virtualScreenBounds.Left;
                int bitmapY = physicalCursorY - virtualScreenBounds.Top;

                if (bitmapX >= 0 && bitmapX < bitmap.Width &&
                    bitmapY >= 0 && bitmapY < bitmap.Height)
                {
                    var pixel = bitmap.GetPixel(bitmapX, bitmapY);
                    color = Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B);
                    return true;
                }
            }

            color = Colors.Black;
            return false;
        }

        private void DisposeBitmap()
        {
            if (_bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }
        }

        private void RaisePickingStarted(EventArgs e)
        {
            var handler = PickingStarted;
            if (handler != null) handler(this, e);
        }

        private void RaisePickingCancelled(EventArgs e)
        {
            var handler = PickingCancelled;
            if (handler != null) handler(this, e);
        }

        private void RaiseColorHovered(ColorEventArgs e)
        {
            var handler = ColorHovered;
            if (handler != null) handler(this, e);
        }

        private void RaiseColorPicked(ColorEventArgs e)
        {
            var handler = ColorPicked;
            if (handler != null) handler(this, e);
        }
    }

    public class ColorEventArgs : EventArgs
    {
        public Color Color { get; private set; }

        public ColorEventArgs(Color color)
        {
            Color = color;
        }
    }
}
