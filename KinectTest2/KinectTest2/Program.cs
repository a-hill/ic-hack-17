using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using Microsoft.Kinect.Fusion;
using Microsoft.Kinect.Tools;

namespace KinectTest2
{
    class Run
    {
        public KinectSensor sensor = null;
        public DepthFrameReader depthFrameReader = null;
        private ushort[] depthFrameData = null;
        private byte[] depthPixels = null


        static void Main(string[] args)
        {
            Console.WriteLine("hello, world");

            // get the attached sensor
            this.sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                Console.WriteLine("Sensor found!");

                // get the depthFrameDescription from the DepthFrameSource
                FrameDescription depthFrameDescription = this.sensor.DepthFrameSource.FrameDescription;

                // open the reader for depth frames
                this.depthFrameReader = this.sensor.DepthFrameSource.OpenReader();

                // attach handler to be invoked upon frame arrival
                this.depthFrameReader.FrameArrived += DepthFrameArrivedHandler;

                // allocate space to put the pixels being received
                this.depthFrameData = new ushort[depthFrameDescription.Width * depthFrameDescription.Height];
                this.depthPixels = new byte[depthFrameDescription.Width * depthFrameDescription.Height * BytesPerPixel];

                

            }
            else
            {
                Console.WriteLine("Sensor not found");
            }
        }

        private static void DepthFrameArrivedHandler(object sender, DepthFrameArrivedEventArgs e)
        {

        }
    }
}
