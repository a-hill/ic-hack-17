/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Research.Kinect.Nui;
using Coding4Fun.Kinect.Wpf;

namespace KinectBlobCounter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // The Kinect runtime used to recieve depth data
        private Runtime runtime;
        private ImageFrame colorFrame = null;
        BlobsDetector detector = new BlobsDetector();
        
        public MainWindow()
        {
            InitializeComponent();
            cmbHighlight.SelectedIndex = 0;

// If you don't make sure the Kinect is plugged in and working before trying to use it, the app will crash
if (Runtime.Kinects.Count > 0)
{
    runtime = Runtime.Kinects[0];
    runtime.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseDepth);
    runtime.DepthFrameReady += (s, e) =>
        {
            if (colorFrame == null) return;

            if (sliderMin.Value > sliderMax.Value)
                sliderMin.Value = sliderMax.Value;

            detector.Highlighting = (HighlightType) cmbHighlight.SelectedIndex;
                        
            txtInfo.Text = detector.BlobCount + " items detected..";
            txtDistance.Text = "Detecting objects in the range " + sliderMin.Value + " and " + sliderMax.Value + " mm";

            var depthFrame=e.ImageFrame.SliceDepthImage((int)sliderMin.Value,(int)sliderMax.Value);
            var depthBmp =depthFrame.ToBitmap();
            var colorBmp = colorFrame.ToBitmapSource().ToBitmap();
            var outBmp=detector.ProcessImage(depthBmp,colorBmp);
            this.ImageColor.Source = outBmp.ToBitmapSource();

            depthBmp.Dispose();
            colorBmp.Dispose();
            outBmp.Dispose();
                        
        };

    runtime.VideoFrameReady += (s, e) =>
        {
            colorFrame = e.ImageFrame;
        };

    runtime.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.Depth);
    runtime.ViSXZdeoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
}
else
    MessageBox.Show("Oops, please check if your Kinect is connected?");
        }
    }
} */
