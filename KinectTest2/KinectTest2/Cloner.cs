using AForge;
using System.Drawing;

namespace KinectTest2
{
    public static class Cloner
    {
        public static Bitmap cropAtRect(this Bitmap bitmap, OurRectangle rectangle)
        {
            IntPoint topLeft = rectangle.topLeft;
            Bitmap newBitmap = new Bitmap(rectangle.getWidth(), rectangle.getHeight());
            Graphics graphics = Graphics.FromImage(newBitmap);
            graphics.DrawImage(bitmap, -topLeft.X, -topLeft.Y);

            return newBitmap;
        }
    }
}
