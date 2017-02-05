﻿using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LightBuzz.Vitruvius;
using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using Microsoft.Kinect.Fusion;
using Microsoft.Kinect.Tools;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.IO;
using System.Net.Http;
using MyToolkit;
using MyToolkit.Networking;

namespace KinectTest2
{
    class Run
    {

        // Frame rate control
        private static int FRAME_RATE = 23; // TOOD: Set frame rate after based on light levels
        private static int SECS_BETWEEN_PROCESSING = 5;
        private static int INVOCATIONS_TO_IGNORE = SECS_BETWEEN_PROCESSING * FRAME_RATE;
        private static int CURR_INVOCATIONS = INVOCATIONS_TO_IGNORE;

        // Sensor and Reader
        private static KinectSensor sensor = null;
        private static MultiSourceFrameReader multiSourceFrameReader = null;

        // Depth 
        private static ushort[] depthFrameData = null;
        private static FrameDescription depthFrameDescription = null;

        // Color
        private static FrameDescription colorFrameDescription = null;
        //private static byte[] pixels = null;
        private static WriteableBitmap bitmap = null;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            // get the attached sensor
            sensor = KinectSensor.GetDefault();

            // allocate space that we need for each type of Frame
            SetUpFrames();

            // open the multi frame reader
            multiSourceFrameReader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Color);

            // attach handler to be invoked upon frame arrival
            multiSourceFrameReader.MultiSourceFrameArrived += MultiSourceFrameArrivedHandler;

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

            // don't just exit straight away
            Console.ReadKey();

            // all done
            sensor.Close();
        }

        private static void Sensor_IsAvailableChanged(KinectSensor sender, IsAvailableChangedEventArgs args)
        {
            Console.WriteLine(sensor.IsAvailable ? "Running" : "Not Available");
        }


        private static void SetUpFrames()
        {
            // 1) Depth Frame
            depthFrameDescription = sensor.DepthFrameSource.FrameDescription;
            depthFrameData = new ushort[depthFrameDescription.Width * depthFrameDescription.Height]; // checked with printf is 512x424

            // 2) Colour
            colorFrameDescription = sensor.ColorFrameSource.FrameDescription;
            int width = colorFrameDescription.Width;
            int height = colorFrameDescription.Height;
            int stride = width * PixelFormats.Bgr32.BitsPerPixel / 8;
            var pixels = new byte[width * height * ((PixelFormats.Bgr32.BitsPerPixel + 7) / 8) + 54]; // 54 byte header
            bitmap = new WriteableBitmap(BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixels, stride));
            //bitmap = new FastBitmap(width, height);
        }


        private static void MultiSourceFrameArrivedHandler(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            if (CURR_INVOCATIONS < INVOCATIONS_TO_IGNORE)
            {
                CURR_INVOCATIONS++;
                return;
            }
            CURR_INVOCATIONS = 0;

            Console.WriteLine("--- DEBUG: MultiSourceFrameArrivedHandler() running.");

            var frameRef = e.FrameReference.AcquireFrame();

            // get the new color frame and open it
            using (ColorFrame colorFrame = frameRef.ColorFrameReference.AcquireFrame())
            {
                #region
                if (colorFrame != null)
                {
                    // Convert to bitmap
                    bitmap = colorFrame.ToBitmap();

                    Console.WriteLine("Bitmap Image Data:");
                    Console.WriteLine(bitmap.ToString());

                    // Give the bitmap to the front end
                    //Dummy2(bitmap);

                    bitmap.Save("color.png");
                }
                #endregion
            }

            // get the new depth frame and open it
            using (DepthFrame depthFrame = frameRef.DepthFrameReference.AcquireFrame())
            {
                #region
                if (depthFrame != null)
                {
                    // get the frame
                    depthFrameDescription = depthFrame.FrameDescription;

                    // store the array to send to front end
                    //ushort[] frameData = new ushort[depthFrameDescription.Width * depthFrameDescription.Height];
                    depthFrame.CopyFrameDataToArray(depthFrameData);

                    // Convert to 2D array
                    int w = depthFrameDescription.Width;
                    int h = depthFrameDescription.Height;
                    // Console.WriteLine("{0} x {1}", w, h);
                    // Console.ReadKey();
                    UInt16?[,] frameData2D = new UInt16?[w, h];

                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            try // for debugging, catch outofbounds exceptions
                            {
                                // If data valid, write value, else write null
                                UInt16 depth = depthFrameData[i * h + j];
                                if (depth <= depthFrame.DepthMaxReliableDistance && depth >= depthFrame.DepthMinReliableDistance)
                                {
                                    frameData2D[i, j] = depth;
                                }
                                else
                                {
                                    frameData2D[i, j] = UInt16.MaxValue;
                                }
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine("failed at: {0} x {1}", i, j);
                                Console.WriteLine(ex);
                                break;
                            }
                        }
                    }

                    // FIND MIN INDEX, watsonStuff will use this later
                    UInt16? minValue = UInt16.MaxValue;
                    int minCol = -1;
                    int minRow = -1;
                    for (int row = 0; row < w; row++)
                    {
                        for (int col = 0; col < h; col++)
                        {
                            if (frameData2D[row, col] < minValue)
                            {
                                minValue = frameData2D[row, col];
                                minRow = row;
                                minCol = col;
                            }
                        }
                    }

                    int minX = minRow;
                    UInt16 minVal = minValue.Value;
                    //Console.WriteLine(minX); Console.WriteLine(minVal);




                    // Give the frame to the front end
                    //Dummy(frameData2D); // not quite yet - need depths for text to speech

                    // Also give the grayscale depth bitmap to the front end

                    #region
                    ushort[] smoothDepthFrameData = Smooth.smoother(depthFrameData, depthFrameDescription.Width, depthFrameDescription.Height);

                    //---------
                    // We multiply the product of width and height by 4 because each byte
                    // will represent a different color channel per pixel in the final iamge.
                    byte[] colorFrame = new byte[depthFrameDescription.Width * depthFrameDescription.Height * 4];

                    // Process each row in parallel
                    Parallel.For(0, 240, depthArrayRowIndex =>
                    {
                        // Process each pixel in the row
                        for (int depthArrayColumnIndex = 0; depthArrayColumnIndex < 320; depthArrayColumnIndex++)
                        {
                            var distanceIndex = depthArrayColumnIndex + (depthArrayRowIndex * 320);
                            // Because the colorFrame we are creating has four times as many bytes representing
                            // a pixel in the final image, we set the index to be the depth index * 4.
                            var index = distanceIndex * 4;

                            // Map the distance to an intesity that can be represented in RGB
                            var intensity = CalculateIntensityFromDistance((int) smoothDepthFrameData[distanceIndex]);

                            // Apply the intensity to the color channels
                            colorFrame[index + 0] = intensity;
                            colorFrame[index + 1] = intensity;
                            colorFrame[index + 2] = intensity;
                        }
                    });
                    //----------
                    BitmapSource bms = BitmapSource.Create(depthFrameDescription.Width, depthFrameDescription.Height, 96, 96, PixelFormats.Bgr32, null, colorFrame, depthFrameDescription.Width * PixelFormats.Bgr32.BitsPerPixel / 8);
                    //----------
                    
                
                
                    OurBlobsDetector blobsDetector = new OurBlobsDetector();
                    List<OurRectangle> rectangles = blobsDetector.ProcessImage(GetBitmap(bms));
                    GetBitmap(bms).Save("testSmooth.bmp");
                    //List<OurRectangle> rectangles = blobsDetector.ProcessImage(writableBitmapToBitmap(depthFrame.ToBitmap()));
                    //List<OurRectangle> rectangles = blobsDetector.ProcessImage(new Bitmap(Image.FromFile("C:\\Users\\Ruhi Choudhury\\Pictures\\ruhi.png")));
                    //List<OurRectangle> rectangles = blobsDetector.ProcessImage(new Bitmap(Image.FromFile("C:\\Users\\Ruhi Choudhury\\Pictures\\testColour.bmp")));

                    foreach (OurRectangle r in rectangles)
                    {
                        Console.WriteLine("Rectangle: " + r.topLeft.X + "," + r.topLeft.Y + " and " + r.bottomRight.X + "," + r.bottomRight.Y);
                    }
                    #endregion

                    Console.WriteLine();
                    depthFrame.ToBitmap().Save("testDepthGray.bmp");


                    // do watson api call with image and parse results and speak
                    //Console.WriteLine(minX); Console.WriteLine(minVal);
                    watsonStuff(minX, minVal);

                    

                }
                #endregion
            }

            
        }

        private async static void watsonStuff(int minX, UInt16 minVal) {

            var request = new HttpPostRequest("https://gateway-a.watsonplatform.net/visual-recognition/api/v3/classify?api_key=c819e57248e3e2a8f6e67c5663265e7660c54d0d&version=2016-05-19");
            Stream fileStream = new FileStream("C:/Users/Ruhi Choudhury/Documents/ic-hack-17/KinectTest2/KinectTest2/bin/Debug/color.png", FileMode.Open);
            request.Files.Add(new HttpPostFile("images_file", "color.png", fileStream));
            var response = await Http.PostAsync(request);

            // print response
            Console.WriteLine(response.Response);

            // parse json and SPEAK
            JsonHandler.jsonHandlerAndFilterAndSpeak(response.Response, minX, minVal);
        }

        private static short CalculateDistanceFromDepth(byte first, byte second)
        {
            // Please note that this would be different if you use Depth and User tracking rather than just depth
            return (short)(first | second << 8);
        }

        private static byte CalculateIntensityFromDistance(int distance)
        {
            int MinDepthDistance = 50;
            int MaxDepthDistanceOffset = 100;
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

        private static WriteableBitmap bitmapToWritableBitmap(Bitmap b)
        {
            // Convert Writable Bitmap to Bitmap
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            b.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            WriteableBitmap writeableBitmap = new WriteableBitmap(bitmapSource);
            return writeableBitmap;
        }

        private static Bitmap writableBitmapToBitmap(WriteableBitmap writeBmp)
        {
            Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
                enc.Save(outStream);
                bmp = new Bitmap(outStream);
            }
            return bmp;
        }

        private static Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap(
              source.PixelWidth,
              source.PixelHeight,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
              new Rectangle(System.Drawing.Point.Empty, bmp.Size),
              ImageLockMode.WriteOnly,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

    }
}