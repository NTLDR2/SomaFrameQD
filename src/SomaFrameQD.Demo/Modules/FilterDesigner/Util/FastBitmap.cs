using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SomaFrameQD.Demo.Modules.FilterDesigner.Util
{
    public class FastBitmap : IDisposable
    {
        #region Variables

        private const int BitsPerPixel = 4;
        private Stream _stream;
        private WriteableBitmap _bitmap;
        private int _width;
        private int _height;
        private int _strideWidth;
        private IntPtr _startingPosition;
        private bool _locked;
        private bool _disposed;

        #endregion

        #region Properties / Indexer

        public WriteableBitmap InnerBitmap
        {
            get { return _bitmap; }
            set
            {
                _bitmap = value;

                if (value != null)
                {
                    _width = value.PixelWidth;
                    _height = value.PixelHeight;
                }
                else
                {
                    _width = 0;
                    _height = 0;
                }
            }
        }

        public int Width
        {
            get { return _width; }
        }

        public int Height
        {
            get { return _height; }
        }

        public unsafe Color this[int x, int y]
        {
            get
            {
                if (!_locked)
                    throw new InvalidOperationException("Must call Lock() before getting pixel values");

                if ((uint)x >= (uint)_width || (uint)y >= (uint)_height)
                    throw new ArgumentOutOfRangeException();

                byte* b = (byte*)_startingPosition + ((long)y * _strideWidth) + ((long)x * BitsPerPixel);
                return Color.FromArgb(*(b + 3), *(b + 2), *(b + 1), *b);
            }

            set
            {
                if (!_locked)
                    throw new InvalidOperationException("Must call Lock() before setting pixel values");

                if ((uint)x >= (uint)_width || (uint)y >= (uint)_height)
                    throw new ArgumentOutOfRangeException();

                byte* b = (byte*)_startingPosition + ((long)y * _strideWidth) + ((long)x * BitsPerPixel);
                *b = value.B;
                *(b + 1) = value.G;
                *(b + 2) = value.R;
                *(b + 3) = value.A;
            }
        }

        #endregion

        #region Constructors

        public FastBitmap(int width, int height)
        {
            InnerBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
        }

        public FastBitmap(string filename, UriKind uriKind = UriKind.Absolute)
        {
            InnerBitmap = new WriteableBitmap(ConvertFormat(new BitmapImage(new Uri(filename, uriKind))));
        }

        public FastBitmap(byte[] bytes)
        {
            _stream = new MemoryStream(bytes);
            lock (_stream)
            {
                if (!_stream.CanRead || _stream.Length == 0)
                    return;

                try
                {
                    var decoder = BitmapDecoder.Create(
                        _stream,
                        BitmapCreateOptions.PreservePixelFormat,
                        BitmapCacheOption.OnLoad);
                    InnerBitmap = new WriteableBitmap(ConvertFormat(decoder.Frames[0]));
                }
                catch (ArgumentException ex)
                {
                    throw new Exception("Specified byte array does not contain an image", ex);
                }
            }
        }

        public FastBitmap(BitmapSource bitmap)
        {
            InnerBitmap = new WriteableBitmap(ConvertFormat(bitmap));
        }

        // Note: No ~FastBitmap() finalizer is defined because WriteableBitmap is a UI-thread DispatcherObject.
        // Calling WriteableBitmap.Unlock() from the CLR 4.8 background GC Finalizer Thread throws an unhandled
        // cross-thread InvalidOperationException that terminates the process.

        #endregion

        private static BitmapSource ConvertFormat(BitmapSource source)
        {
            // If bitmap is anything other than Pbgra32, convert it to Pbgra32 so that we can deal with all images consistently.
            if (source != null && source.Format != PixelFormats.Pbgra32)
            {
                source = new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0);
            }

            return source;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed || !disposing)
                return;

            _disposed = true;

            if (_bitmap != null && _bitmap.CheckAccess())
            {
                Unlock();
            }

            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
        }

        #region Methods

        public void Lock()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(FastBitmap));

            _bitmap.Lock();
            _startingPosition = _bitmap.BackBuffer;
            _strideWidth = _bitmap.BackBufferStride;
            _locked = true;
        }

        public void Unlock()
        {
            if (_locked)
            {
                _bitmap.Unlock();
                _locked = false;
            }
        }

        /// <summary>
        /// Only used for testing.
        /// </summary>
        public void Save(string filename)
        {
            using (FileStream stream = File.OpenWrite(filename))
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(_bitmap));
                encoder.Save(stream);
            }
        }

        #endregion
    }
}
