using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using AForge;
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge.Math.Geometry;

namespace ParkingApp.classes
{
    class framesProcessing
    {

        static public int fHeight, fWidth;
               
        public framesProcessing() { 
               
        }

        public static int CalculateWhitePixels(Bitmap image)
        {
            int count = 0;
            // lock difference image            
            BitmapData data = image.LockBits(new Rectangle(0, 0, fWidth, fHeight),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            int offset = data.Stride - fWidth;
            byte[,] bitsMatrix = new byte[fHeight, fWidth];

            unsafe
            {
                byte* ptr = (byte*)data.Scan0.ToPointer();

                for (int y = 0; y < fHeight; y++)
                {
                    for (int x = 0; x < fWidth; x++, ptr++)
                    {
                        if (*ptr != 0)
                        { bitsMatrix[y, x] = 0; }
                        else { bitsMatrix[y, x] = 1; }
                        count += ((*ptr) >> 7);
                    }
                    ptr += offset;
                }
            }
            // unlock image
            image.UnlockBits(data);
            return count;
        }

        public Bitmap rgb2hsv(Bitmap imageinHSV)
        {
            BitmapData bitmapdata = imageinHSV.LockBits(new Rectangle(0, 0, imageinHSV.Width, imageinHSV.Height),
                                                        ImageLockMode.ReadWrite, imageinHSV.PixelFormat);
            unsafe
            {

                byte* scan0 = (byte*)bitmapdata.Scan0.ToPointer();
                int stride = bitmapdata.Stride;
                int bytesPerPixel = 3;

                byte red, green, blue;

                for (int y = 0; y < imageinHSV.Height; ++y)
                {
                    byte* row = scan0 + (y * stride);
                    for (int x = 0; x < imageinHSV.Width; ++x)
                    {
                        int bIndex = x * bytesPerPixel;
                        int gIndex = bIndex + 1;
                        int rIndex = gIndex + 1;

                        blue = row[bIndex];
                        green = row[gIndex];
                        red = row[rIndex];

                        double hue;
                        double saturation;
                        double value;
                        ColorToHSV(red, blue, green, out hue, out saturation, out value);


                        row[rIndex] = (Byte)(saturation * 255); /*(Byte)(255 * hue / 360);*/
                        row[gIndex] = (Byte)(saturation * 255); /*(Byte)(saturation * 255);*/
                        row[bIndex] = (Byte)(saturation * 255); /*(Byte)(255 * value);*/

                    }
                }
            }
            imageinHSV.UnlockBits(bitmapdata);
            return imageinHSV;
        }       

        public void binarization(ref Bitmap rgb, ref Bitmap hsv)
        {
            //rgb image
            BitmapData bitmapdata = rgb.LockBits(new Rectangle(0, 0, rgb.Width, rgb.Height),
                                                        ImageLockMode.ReadWrite, rgb.PixelFormat);
            unsafe
            {
                byte* scan0 = (byte*)bitmapdata.Scan0.ToPointer();
                int stride = bitmapdata.Stride;
                int bytesPerPixel = 3;

                byte red, green, blue;

                for (int y = 0; y < rgb.Height; ++y)
                {
                    byte* row = scan0 + (y * stride);
                    for (int x = 0; x < rgb.Width; ++x)
                    {
                        int bIndex = x * bytesPerPixel;
                        int gIndex = bIndex + 1;
                        int rIndex = gIndex + 1;

                        blue = row[bIndex];
                        green = row[gIndex];
                        red = row[rIndex];
                        if ((red > 130 && red < 255) && (green > 80 && green < 255) && (blue < 100/*85*/))
                            row[bIndex] = row[gIndex] = row[rIndex] = 255;
                        else
                            row[bIndex] = row[gIndex] = row[rIndex] = 0;
                    }
                }
            }
            rgb.UnlockBits(bitmapdata);

            bitmapdata = hsv.LockBits(new Rectangle(0, 0, hsv.Width, hsv.Height),
                                                     ImageLockMode.ReadWrite, hsv.PixelFormat);
            unsafe
            {
                byte* scan0 = (byte*)bitmapdata.Scan0.ToPointer();
                int stride = bitmapdata.Stride;
                int bytesPerPixel = 3;

                byte pixelbyte;

                for (int y = 0; y < hsv.Height; ++y)
                {
                    byte* row = scan0 + (y * stride);
                    for (int x = 0; x < hsv.Width; ++x)
                    {
                        int bIndex = x * bytesPerPixel;
                        int gIndex = bIndex + 1;
                        int rIndex = gIndex + 1;

                        pixelbyte = row[bIndex];

                        if (pixelbyte > 255 * 0.4/*0.7*/)
                            row[bIndex] = row[gIndex] = row[rIndex] = 255;
                        else
                            row[bIndex] = row[gIndex] = row[rIndex] = 0;
                    }
                }
            }
            hsv.UnlockBits(bitmapdata);

        }

        public Bitmap commonBits(Bitmap rgb, Bitmap hsv)
        {
            BitmapData bitmapdata = rgb.LockBits(new Rectangle(0, 0, rgb.Width, rgb.Height),
                                            ImageLockMode.ReadOnly, rgb.PixelFormat);
            byte[,] rgbMatrix = new byte[rgb.Width, rgb.Height];

            unsafe
            {
                byte* scan0 = (byte*)bitmapdata.Scan0.ToPointer();
                int stride = bitmapdata.Stride;
                int bytesPerPixel = 3;

                byte pixelbyte;

                for (int y = 0; y < rgb.Height; ++y)
                {
                    byte* row = scan0 + (y * stride);
                    for (int x = 0; x < rgb.Width; ++x)
                    {
                        int bIndex = x * bytesPerPixel;
                        int gIndex = bIndex + 1;
                        int rIndex = gIndex + 1;

                        pixelbyte = row[bIndex];
                        rgbMatrix[x, y] = pixelbyte;
                    }
                }
            }
            rgb.UnlockBits(bitmapdata);

            bitmapdata = hsv.LockBits(new Rectangle(0, 0, hsv.Width, hsv.Height),
                                                    ImageLockMode.ReadWrite, hsv.PixelFormat);
            unsafe
            {
                byte* scan0 = (byte*)bitmapdata.Scan0.ToPointer();
                int stride = bitmapdata.Stride;
                int bytesPerPixel = 3;

                byte pixelbyte;

                for (int y = 0; y < hsv.Height; ++y)
                {
                    byte* row = scan0 + (y * stride);
                    for (int x = 0; x < hsv.Width; ++x)
                    {
                        int bIndex = x * bytesPerPixel;
                        int gIndex = bIndex + 1;
                        int rIndex = gIndex + 1;

                        pixelbyte = row[bIndex];
                        if (pixelbyte == 255 && rgbMatrix[x, y] == 255)
                            row[bIndex] = row[gIndex] = row[rIndex] = 255;
                        else
                            row[bIndex] = row[gIndex] = row[rIndex] = 0;
                    }
                }
            }

            hsv.UnlockBits(bitmapdata);
            rgb.Dispose();
            return hsv;

        }

        public Bitmap diamonDilatation(Bitmap hsv)
        {
            int kernelSize = 9; // odd number
            short[,] diamonKernel = diamondKernel(kernelSize);
            Dilatation dfilter = new Dilatation(diamonKernel);
            dfilter.ApplyInPlace(hsv);
            Erosion efilter = new Erosion(diamonKernel);
            efilter.ApplyInPlace(hsv);
            return hsv;
        }


        private static void fix(ref int[,] points)
        {

            //Puntos Arriba Y Abajo
            if (points[0, 1] < points[1, 1])
                points[1, 1] = points[0, 1];
            else
                points[0, 1] = points[1, 1];

            if (points[3, 1] > points[2, 1])
                points[2, 1] = points[3, 1];
            else
                points[3, 1] = points[2, 1];

            //Puntos Laterales
            if (points[0, 0] < points[3, 0])
                points[3, 0] = points[0, 0];
            else
                points[0, 0] = points[3, 0];

            if (points[1, 0] > points[2, 0])
                points[2, 0] = points[1, 0];
            else
                points[1, 0] = points[2, 0];
        }

        public Bitmap getBiggestBloob(Bitmap hsv, Bitmap original)
        {
            Bitmap biggestBlobsImage = hsv;
            int maximum = 0;
            BitmapData imageData = biggestBlobsImage.LockBits(
                new Rectangle(0, 0, biggestBlobsImage.Width, biggestBlobsImage.Height),
                    ImageLockMode.ReadWrite, biggestBlobsImage.PixelFormat);

            // locate blobs in the source image
            BlobCounter blobCounter = new BlobCounter(imageData);
            // get information about blobs
            Blob[] blobs = blobCounter.GetObjectsInformation();
            // find the biggest blob
            int maxSize = 0;
            Blob biggestBlob = null;

            List<Blob> blobs2 = new List<Blob>();
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();

            foreach (Blob blob in blobs)
            {
                List<IntPoint> edgePoints2 = blobCounter.GetBlobsEdgePoints(blob);
                List<IntPoint> corners2;
                try
                {
                    if (shapeChecker.IsQuadrilateral(edgePoints2, out corners2))
                    {
                        blobs2.Add(blob);
                    }
                }
                catch { }
            }
            if (blobs2.Count == 0)
            {
                foreach (Blob blob in blobs)
                {
                    blobs2.Add(blob);
                }
            }

            int i = 0;
            foreach (Blob blob in blobs)
            {
                int size = blob.Rectangle.Width * blob.Rectangle.Height;

                if (size > maxSize)
                {
                    maxSize = size;
                    biggestBlob = blob;
                    maximum = i;
                }
                i++;
            }

            biggestBlobsImage.UnlockBits(imageData);

            blobCounter = new BlobCounter();
            blobCounter.ProcessImage(biggestBlobsImage);
            //blobs = blobCounter.GetObjectsInformation();

            List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[maximum]);
            List<IntPoint> corners = PointsCloud.FindQuadrilateralCorners(edgePoints);
            List<IntPoint> corn2 = new List<IntPoint>();
            int[,] points = new int[4, 2];
            int j = 0;
            foreach (IntPoint ip in corners)
            {
                points[j, 0] = ip.X;
                points[j++, 1] = ip.Y;
            }

            fix(ref points);
            int aR;
            int a = points[0, 0];
            aR = points[0, 0];
            points[0, 0] = (Int16)(Math.Ceiling(points[0, 0] * (0.93)));
            aR -= points[0, 0];
            aR = Math.Abs(points[0, 0] - a);
            points[3, 0] = points[0, 0];

            points[1, 0] = points[1, 0] + aR;
            points[2, 0] = points[1, 0];
                        
            j = 0;
            foreach (IntPoint ip in corners)
            {
                IntPoint ipp = new IntPoint();
                ipp.X = points[j, 0];
                ipp.Y = points[j++, 1];
                corn2.Add(ipp);
            }

            int imgHeight = original.Height;
            int imgWidth = original.Width;

            ResizeBilinear resizer = new ResizeBilinear(imgWidth, imgHeight);
            QuadrilateralTransformation qua = new QuadrilateralTransformation(corn2);

            Bitmap blobImage = qua.Apply(original);

            if (imgWidth > imgHeight)
                blobImage.RotateFlip(RotateFlipType.Rotate270FlipNone);

            blobImage = resizer.Apply(blobImage);

            return blobImage;

        }               

        private void ColorToHSV(Byte red, Byte green, Byte blue, out double hue, out double saturation, out double value)
        {
            double r = red / 255d;
            double g = (green / 255d);
            double b = blue / 255d;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            value = max;
            double delta = max - min;

            if (max != 0)
                saturation = delta / max;		// s
            else
            {
                // r = g = b = 0		// s = 0, v is undefined
                saturation = 0;
                hue = -1;
                return;
            }
            if (r == max)
            {
                /*double ans*/
                hue = (g - b) / delta;		// between yellow & magenta
                //hue = (double)(ans % 6d);
            }
            else if (g == max)
                hue = 2 + (b - r) / delta;	// between cyan & yellow
            else
                hue = 4 + (r - g) / delta;	// between magenta & cyan

            hue *= 60;				// degrees
            if (hue < 0)
                hue += 360;

        }

        private short[,] diamondKernel(int kernelSize)
        {
            short[,] diamonKernel = new short[0, 0];
            ResizeArrayNxN(ref diamonKernel, kernelSize, kernelSize);
            int m = ((diamonKernel.GetLength(1) + 1) / 2);
            int mm = m * 2;
            for (int j = 0; j < m; j++)
            {
                int n = 0;
                diamonKernel[j, (m - 1)] = 1;
                diamonKernel[((mm - 2) - j), (m - 1)] = 1;
                do
                {
                    //upper Diamond
                    diamonKernel[j, (n + (m - 1))] = 1;
                    diamonKernel[j, ((m - 1) - n)] = 1;
                    //lower Diamond
                    diamonKernel[((mm - 2) - j), (n + (m - 1))] = 1;
                    diamonKernel[((mm - 2) - j), ((m - 1) - n)] = 1;
                    //counter
                    n++;
                } while (n <= j);
            }
            return diamonKernel;
        }

        private T[,] ResizeArrayNxN<T>(ref T[,] original, int column1, int column2)
        {
            //create a new 2 dimensional array with
            //the size we want
            T[,] newArray = new T[column1, column2];
            //copy the contents of the old array to the new one
            Array.Copy(original, newArray, original.Length);
            //set the original to the new array
            original = newArray;
            return original;
        }   


    }
}
