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
        private static void Dummy(UInt16?[,] frameData) { return; }

        public static KinectSensor sensor = null;
        public static DepthFrameReader depthFrameReader = null;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            // get the attached sensor
            sensor = KinectSensor.GetDefault();

            // get the depthFrameDescription from the DepthFrameSource
            FrameDescription depthFrameDescription = sensor.DepthFrameSource.FrameDescription;

            // open the reader for depth frames
            depthFrameReader = sensor.DepthFrameSource.OpenReader();

            // attach handler to be invoked upon frame arrival
            depthFrameReader.FrameArrived += DepthFrameArrivedHandler;

            // allocate space to put the pixels being received
            // TODO: WTF IS THIS FOR?
            ushort[] depthFrameData = new ushort[depthFrameDescription.Width * depthFrameDescription.Height];

            // Open the sensor
            sensor.Open();

            // TODO: Implement IsAvailableChanged delegate
            if (sensor.IsAvailable)
            {
                Console.WriteLine("Sensor found!");
            }
            else
            {
                Console.WriteLine("Sensor not found");
            }

            Console.ReadKey();

            sensor.Close();
        }

        private void Sensor_IsAvailableChanged(KinectSensor sender, IsAvailableChangedEventArgs args)
        {
            Console.WriteLine(sensor.IsAvailable ? "Running" : "Not Available");
        }

        // TODO: 
        private static void DepthFrameArrivedHandler(object sender, DepthFrameArrivedEventArgs e)
        {
            Console.WriteLine("--- DEBUG: DepthFrameArrivedHandler() called.");
            
            // get the new depth frame and open it
            using (DepthFrame depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    // get the frame
                    FrameDescription depthFrameDescription = depthFrame.FrameDescription;

                    // store the array to send to front end
                    ushort[] frameData = new ushort[depthFrameDescription.Width * depthFrameDescription.Height];
                    depthFrame.CopyFrameDataToArray(frameData);

                    // Convert to 2D array
                    int w = depthFrameDescription.Width;
                    int h = depthFrameDescription.Height;
                    UInt16?[,] frameData2D = new UInt16?[w, h];

                    // Print frame data for debugging
                    Console.WriteLine("Frame Data:");
                    Console.WriteLine(frameData.ToString());

                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            // If data valid, write value, else write null
                            UInt16 depth = frameData[i * w + j];
                            if (depth <= depthFrame.DepthMaxReliableDistance && depth >= depthFrame.DepthMinReliableDistance)
                            {
                                frameData2D[i, j] = (UInt16) frameData[i * w + j];
                            }
                            else
                            {
                                frameData2D[i, j] = null;
                            }
                        }
                    }

                    // Give the frame to the front end
                    Dummy(frameData2D);
                }
            }
        }
    }
}