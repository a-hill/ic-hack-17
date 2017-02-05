using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge;

namespace KinectTest2
{
    public class OurRectangle
    {
        public IntPoint topLeft;
        public IntPoint topRight;
        public IntPoint bottomLeft;
        public IntPoint bottomRight;

        public OurRectangle(IntPoint topLeft,
                    IntPoint topRight,
                    IntPoint bottomLeft,
                    IntPoint bottomRight) 
        {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomLeft = bottomLeft;
            this.bottomRight = bottomRight;
        }

        public int getWidth()
        {
            return topRight.X - topLeft.X;
        }

        public int getHeight()
        {
            return topLeft.Y - bottomLeft.Y;
        }
    }
}
