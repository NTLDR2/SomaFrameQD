using System.Drawing;
using System.Drawing.Imaging;
using SomaFrameQD.Modules.Inspector.Win32;

namespace SomaFrameQD.Modules.Inspector.Util
{
    public class ScreenShotUtility
    {
        public static Rectangle GetVirtualScreenPhysicalBounds()
        {
            return NativeMethods.GetVirtualScreenPhysicalBounds();
        }

        public static Bitmap Take()
        {
            var bounds = NativeMethods.GetVirtualScreenPhysicalBounds();
            var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppRgb);

            try
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(
                        bounds.Left,
                        bounds.Top,
                        0,
                        0,
                        bounds.Size,
                        CopyPixelOperation.SourceCopy);
                }

                return bitmap;
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }
        }
    }
}
