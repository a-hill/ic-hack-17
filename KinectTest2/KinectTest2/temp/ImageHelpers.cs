using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using Microsoft.Research.Kinect.Nui;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows;
using System.ComponentModel;
using System.Drawing;
using System.IO;

namespace KinectBlobCounter
{


    /// <summary>
    /// Credits: 
    /// Portions of code below from
    /// (1) http://www.codeproject.com/Articles/317974/KinectDepthSmoothing though I'm not really smoothing here :)
    /// (2) http://stackoverflow.com/questions/94456/load-a-wpf-bitmapimage-from-a-system-drawing-bitmap
    /// 
    /// -- Anoop
    /// </summary>
    public static class ImageHelpers
    {
        // Constants used to address the individual color pixels for generating images
        private const int RedIndex = 2;
        private const int GreenIndex = 1;
        private const int BlueIndex = 0;

        // Constants used to map value ranges for distance to pixel intensity conversions
        private const int MaxDepthDistance = 4000;
        private const int MinDepthDistance = 850;
        private const int MaxDepthDistanceOffset = 3150;


        public static BitmapSource SliceDepthImage(this ImageFrame image, int min=20, int max=1000)
        {
            int width = image.Image.Width;
            int height = image.Image.Height;

            var depthFrame = image.Image.Bits;
            // We multiply the product of width and height by 4 because each byte
            // will represent a different color channel per pixel in the final iamge.
            var colorFrame = new byte[height * width * 4];

            // Process each row in parallel
            Parallel.For(0, 240, depthRowIndex =>
            {
                //  Within each row, we then iterate over each 2 indexs to be combined into a single depth value
                for (int depthColumnIndex = 0; depthColumnIndex < 640; depthColumnIndex += 2)
                {
                    var depthIndex = depthColumnIndex + (depthRowIndex * 640);

                    // Because the colorFrame we are creating has twice as many bytes representing
                    // a pixel in the final image, we set the index to be twice of the depth index.
                    var index = depthIndex * 2;

                    // Calculate the distance represented by the two depth bytes
                    var distance = CalculateDistanceFromDepth(depthFrame[depthIndex], depthFrame[depthIndex + 1]);

                    // Map the distance to an intesity that can be represented in RGB
                    var intensity = CalculateIntensityFromDistance(distance);

                    if (distance > min && distance < max)
                    {
                        // Apply the intensity to the color channels
                        colorFrame[index + 0] = intensity; //blue
                        colorFrame[index + 1] = intensity; //green
                        colorFrame[index + 2] = intensity; //red
                        colorFrame[index + 3] = 255; //alpha
                    }
                }
            });

            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, colorFrame, width * PixelFormats.Pbgra32.BitsPerPixel / 8);
        }


        private static short CalculateDistanceFromDepth(byte first, byte second)
        {
            // Please note that this would be different if you use Depth and User tracking rather than just depth
            return (short)(first | second << 8);
        }

        private static byte CalculateIntensityFromDistance(int distance)
        {
            // This will map a distance value to a 0 - 255 range
            // for the purposes of applying the resulting value
            // to RGB pixels.
            int newMax = distance - MinDepthDistance;
            if (newMax > 0)
                return (byte)(255 - (255 * newMax
                / (MaxDepthDistanceOffset)));
            else
                return (byte)255;
        }


        /// <summary>
        /// Converts a <see cref="System.Drawing.Image"/> into a WPF <see cref="BitmapSource"/>.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <returns>A BitmapSource</returns>
        public static BitmapSource ToBitmapSource(this System.Drawing.Image source)
        {
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(source);

            var bitSrc = bitmap.ToBitmapSource();

            bitmap.Dispose();
            bitmap = null;

            return bitSrc;
        }

        /// <summary>
        /// Converts a <see cref="System.Drawing.Bitmap"/> into a WPF <see cref="BitmapSource"/>.
        /// </summary>
        /// <remarks>Uses GDI to do the conversion. Hence the call to the marshalled DeleteObject.
        /// </remarks>
        /// <param name="source">The source bitmap.</param>
        /// <returns>A BitmapSource</returns>
        public static BitmapSource ToBitmapSource(this System.Drawing.Bitmap source)
        {
            BitmapSource bitSrc = null;

            var hBitmap = source.GetHbitmap();

            try
            {
                bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Win32Exception)
            {
                bitSrc = null;
            }
            finally
            {
                NativeMethods.DeleteObject(hBitmap);
            }

            return bitSrc;
        }

        /// <summary>
        /// Convert the bitmapsource to bitmap
        /// </summary>
        /// <param name="bitmapsource"></param>
        /// <returns></returns>
        public static System.Drawing.Bitmap ToBitmap(this BitmapSource bitmapsource)
        {
            System.Drawing.Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                // from System.Media.BitmapImage to System.Drawing.Bitmap
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
                return bitmap;
            }
        }

    }




    /// <summary>
    /// FxCop requires all Marshalled functions to be in a class called NativeMethods.
    /// </summary>
    internal static class NativeMethods
    {
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);
    }
}
