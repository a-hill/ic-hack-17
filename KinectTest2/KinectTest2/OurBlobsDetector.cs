using System.Collections.Generic;
using System.Linq;
using AForge;
using AForge.Imaging;
using System.Drawing;
using System.Drawing.Imaging;

namespace KinectTest2
{

    // Blobs' highlight types enumeration
    public enum HighlightType
    {
        ConvexHull = 0,
        LeftAndRightEdges = 1,
        TopAndBottomEdges = 2,
        Quadrilateral = 3
    }

    public class OurBlobsDetector
    {
        private Bitmap image = null;
        private int imageWidth, imageHeight;


        private BlobCounter blobCounter = new BlobCounter();
        private Blob[] blobs;
        private int selectedBlobID;

        List<OurRectangle> listOfRectangles = new List<OurRectangle>();


        private HighlightType highlighting = HighlightType.ConvexHull;

        // Blobs' highlight type
        public HighlightType Highlighting
        {
            get { return highlighting; }
            set
            {
                highlighting = value;
            }
        }


        public OurBlobsDetector()
        {
        }

        // Set image to display by the control
        public List<OurRectangle> ProcessImage(Bitmap depthImage)
        {
            /*
            leftEdges.Clear();
            rightEdges.Clear();
            topEdges.Clear();
            bottomEdges.Clear();
            hulls.Clear();
            quadrilaterals.Clear();
            */
            selectedBlobID = 0;

            this.image = AForge.Imaging.Image.Clone(depthImage, PixelFormat.Format24bppRgb);
            imageWidth = this.image.Width;
            imageHeight = this.image.Height;

            blobCounter.ProcessImage(this.image);
            blobs = blobCounter.GetObjectsInformation();

            //ResizeNearestNeighbor filter = new ResizeNearestNeighbor(depthImage.Width, depthImage.Height);
            //var outImage = filter.Apply(colorImage);
            //outImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
           
            BlobCount = blobs.Count();


            foreach (Blob blob in blobs)
            {
                // BOX BOX HERE HERE
                List<IntPoint> leftEdge = new List<IntPoint>();
                List<IntPoint> rightEdge = new List<IntPoint>();
                List<IntPoint> topEdge = new List<IntPoint>();
                List<IntPoint> bottomEdge = new List<IntPoint>();

                // collect edge points
                blobCounter.GetBlobsLeftAndRightEdges(blob, out leftEdge, out rightEdge);
                blobCounter.GetBlobsTopAndBottomEdges(blob, out topEdge, out bottomEdge);

                // Get the corner coordinates
                IntPoint topEdgePoint = topEdge[0];
                int topY = topEdgePoint.Y;

                IntPoint bottomEdgePoint = bottomEdge[0];
                int bottomY = bottomEdgePoint.Y;

                IntPoint leftEdgePoint = leftEdge[0];
                int leftX = leftEdgePoint.X;

                IntPoint rightEdgePoint = rightEdge[0];
                int rightX = rightEdgePoint.X;

                IntPoint topLeft = new IntPoint(leftX, topY);
                IntPoint topRight = new IntPoint(rightX, topY);
                IntPoint bottomLeft = new IntPoint(leftX, bottomY);
                IntPoint bottomRight = new IntPoint(rightX, bottomY);

                OurRectangle ourRectangle = new OurRectangle(topLeft, topRight, bottomLeft, bottomRight);
                listOfRectangles.Add(ourRectangle);

                // shift all points for vizualization
                //IntPoint shift = new IntPoint(1, 1);

               // PointsCloud.Shift(leftEdge, shift);
                //PointsCloud.Shift(rightEdge, shift);
                //PointsCloud.Shift(topEdge, shift);
                //PointsCloud.Shift(bottomEdge, shift);
                //PointsCloud.Shift(hull, shift);
                //PointsCloud.Shift(quadrilateral, shift);
            }

            return listOfRectangles;
        }

        public int BlobCount { get; set; }

    }
}
