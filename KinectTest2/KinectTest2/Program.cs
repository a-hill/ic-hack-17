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
        static void Main(string[] args)
        {
            Console.WriteLine("hello, world");

            // The sensor objects.
            KinectSensor _sensor = null;

            /*
            // The color frame reader is used to display the RGB stream
            ColorFrameReader _colorReader = null;

            // The body frame reader is used to identify the bodies
            BodyFrameReader _bodyReader = null;

            // The list of bodies identified by the sensor
            IList<Body> _bodies = null;

            // The face frame source
            FaceFrameSource _faceSource = null;

            // The face frame reader
            FaceFrameReader _faceReader = null;
            */

            // The depth sensor
            DepthFrameReader _depthReader = null;


            // finally, start
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                Console.WriteLine("Sensor found!");

                // DO STUFF FINALLY

            }
            else
            {
                Console.WriteLine("Sensor not found");
            }
        }
    }
}
