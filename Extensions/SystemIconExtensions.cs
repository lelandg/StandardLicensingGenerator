using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StandardLicensingGenerator
{
    public static class SystemIconExtensions
    {
        /// <summary>
        /// Converts a System.Drawing.Icon to a WPF ImageSource
        /// </summary>
        /// <param name="icon">System icon to convert</param>
        /// <returns>ImageSource for WPF use</returns>
        public static ImageSource ToImageSource(this Icon icon)
        {
            if (icon == null) throw new ArgumentNullException(nameof(icon));

            // Use Interop to convert System.Drawing.Icon to WPF ImageSource
            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            // Clean up unmanaged resources
            DeleteObject(hBitmap);
            bitmap.Dispose();

            return wpfBitmap;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
    }
}
