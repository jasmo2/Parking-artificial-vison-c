using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace ParkingApp.classes
{
    public class parkingCamp
    {
        private Grayscale filterGray;
        private Bitmap emptyLots, currentFrame;
        private parkingSlot parkingS;

        Point[] upperParkingPoint;
        Bitmap[] upperParkingSlotsModel;
        Point[] downParkingPoints;
        Bitmap[] downParkingSlotsModel;
        Hashtable parkingList;
        Hashtable lastParkingStatus;

        public Hashtable ParkingList
        {
            get { return this.parkingList; }
            set { this.parkingList = value; }
        }

        public Hashtable LastParkingStatus
        {
            get { return this.lastParkingStatus; }
            set { this.lastParkingStatus = value; }
        }

        public Bitmap[] UpperParkingSlotsModel
        {
            get { return this.upperParkingSlotsModel; }
            set { this.upperParkingSlotsModel = value; }
        }

        public Bitmap[] DownParkingSlotsModel
        {
            get { return this.downParkingSlotsModel; }
            set { this.downParkingSlotsModel = value; }
        }

        public Point[] UpperParkingPoint
        {
            get { return this.upperParkingPoint; }
            set { this.upperParkingPoint = value; }
        }

        public Point[] DownParkingPoints
        {
            get { return this.downParkingPoints; }
            set { this.downParkingPoints = value; }
        }

        public Grayscale FilterGray
        {
            get { return this.filterGray; }
            set { this.filterGray = value; }
        }

        public Bitmap EmptyLots
        {
            get { return this.emptyLots; }
            set { this.emptyLots = value; }
        }

        public Bitmap CurrentFrame
        {
            get { return this.currentFrame; }
            set { this.currentFrame = value; }
        }

        public parkingCamp() {

            this.parkingList = new Hashtable();
            this.emptyLots = new Bitmap(@"model0.jpg"); // ModeloCero
            this.currentFrame = new Bitmap(@"model0.jpg");// Modelo imagen sin modificar
            this.filterGray = new Grayscale(0.2125, 0.7154, 0.0721);

        }

        public void FillList(int counter, String name)
        {
            for (int i = 1; i < counter; i++)
                this.parkingList.Add(name + i, "Vacio");
        }


        public void zoneLimits() //buscar primer modelo
        {
            Bitmap bmLinesLimits;
            Bitmap originalImage = this.emptyLots.Clone() as Bitmap;

            originalImage = filterGray.Apply(originalImage);
            Binarization(originalImage);
            FiltersSequence filterBefore = new FiltersSequence(new Dilatation(), new Dilatation(), new Dilatation(),new Erosion());
            FiltersSequence filterAfter = new FiltersSequence(filterGray, new Erosion(), new Erosion());

            int width = 1280 ; int height = 350; int initialX = 0; int initialY = 0;

            bmLinesLimits = cropImage(initialX, initialY, width, height, originalImage);
            //this filters is been using to enlarge the separator lines

            bmLinesLimits = filterBefore.Apply(bmLinesLimits);
            parkingLotsMatrixes(bmLinesLimits, out upperParkingPoint);//, out upperParkingSlots);

            //The down limits
            width = 1280; height = 370; initialX = 0; initialY = 350;
            bmLinesLimits = cropImage(initialX, initialY, width, height, originalImage);
            bmLinesLimits = filterBefore.Apply(bmLinesLimits);
            
            parkingLotsMatrixes(bmLinesLimits, out downParkingPoints);//, out downParkingSlots);
            int visual = downParkingPoints.Length;
            for (int i = 0; i < (downParkingPoints.Length / 4); i++)
            {
                downParkingPoints[0 + (4 * i)].Y = downParkingPoints[0 + (4 * i)].Y + initialY;
                downParkingPoints[1 + (4 * i)].Y = downParkingPoints[1 + (4 * i)].Y + initialY;
                downParkingPoints[2 + (4 * i)].Y = downParkingPoints[2 + (4 * i)].Y + initialY;
                downParkingPoints[3 + (4 * i)].Y = downParkingPoints[3 + (4 * i)].Y + initialY;
            }

        }

        unsafe public string[,] upperLinesPointsFinder(string[,] matrix, int rows, int columns, int y, int x)
        {
            try
            {
                buttomPoints(ref matrix, rows, columns, y, x);
            }
            catch (Exception)
            {
                if (matrix.GetLength(1) - 1 < columns)
                {
                    String[,] tempMatrix = matrix;
                    matrix = ResizeArrayNxN(ref matrix, rows,
                                                columns + 1);

                    matrix[rows - 1, columns] = x + "," + y;
                    for (int i = 0; i < matrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < matrix.GetLength(1); j++)
                        {
                            try
                            {
                                matrix[i, j] = tempMatrix[i, j];
                            }
                            catch (IndexOutOfRangeException)
                            {
                                if (matrix.GetLength(0) - 1 != i || matrix.GetLength(1) - 1 != j)
                                {
                                    matrix[i, j] = null;
                                }

                            }
                        }
                    }
                }

            }
            return matrix;
        }

        unsafe public static void buttomPoints(ref string[,] matrix, int rows, int columns, int y, int x)
        {
            String[] compare;
            int intCompare;

            compare = Regex.Split(matrix[rows - 2, columns], ",");
            intCompare = Convert.ToInt16(compare[0]);

            //stringPoints(ref matrix, rows, columns, out compare, out intCompare);
            //buttom limit, check if the next line is near to the buttom than the previous one
            if (intCompare - 20 < x && x < intCompare + 20)
            {
                matrix[rows - 1, columns] = x + "," + y;
            }
            else
            {
                int n = 0;
                while (intCompare + 20 < x || x < intCompare - 20)
                {
                    n++;
                    compare = Regex.Split(matrix[rows - 2, columns + n], ",");
                    intCompare = Convert.ToInt16(compare[0]);
                }
                matrix[rows - 1, columns + n] = x + "," + y;
            }
        }
        
        unsafe public static Int16 firstValueNotNull(ref string[,] matrix, int i)
        {
            Int16 m = 0; int q = i;//just to visualize the value
            while (matrix[0 + m, i] == null /*|| i == matrix.GetLength(0)*/)
            { m++; }
            return m;
        }

        unsafe public static Int16 finalValueNotNull(ref string[,] matrix, int i)
        {
            Int16 n = 0; /*int m = matrix.GetLength(0);*///just to visualize the value
            while (matrix[matrix.GetLength(0) - 1 - n, i] == null)
            { n++; }
            return n;
        }

        public T[,] ResizeArrayNxN<T>(ref T[,] original, int column1, int column2)
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

        public void rgb2hsv(Bitmap imageinHSV)
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
        }

        public void ColorToHSV(Byte red, Byte green, Byte blue, out double hue, out double saturation, out double value)
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

        unsafe public void matrixCreation(ref string[,] matrix, ref int rows, ref int columns, ref Boolean boolean, int y, int x)
        {
            if (boolean == true)
            { rows++; boolean = false; }

            if (rows == 1)
            {
                matrix = ResizeArrayNxN(ref matrix, rows, matrix.GetLength(1) + 1);
                matrix[rows - 1, matrix.GetLength(1) - 1] = x + "," + y;
            }
            else
            {

                if (columns == 0)
                    matrix = ResizeArrayNxN(ref matrix, rows,
                                                    matrix.GetLength(1));
                matrix = upperLinesPointsFinder(matrix, rows, columns, y, x);
                columns++;

            }
        }

        public long[] getHistogram(Bitmap picture)
        {
            long[] myHistogram = new long[256];

            for (int i = 0; i < picture.Size.Width; i++)
                for (int j = 0; j < picture.Size.Height; j++)
                {
                    System.Drawing.Color c = picture.GetPixel(i, j);

                    long Temp = 0;
                    Temp += c.R;
                    Temp += c.G;
                    Temp += c.B;

                    Temp = (int)Temp / 3;
                    myHistogram[Temp]++;
                }

            return myHistogram;
        }

        public void parkingLotsMatrixes(Bitmap bmLinesLimits, out Point[] curvePoints)//, out Bitmap[] bmParkingSlots)
        {
            int parkingLotspaces = 1;
            curvePoints = new Point[0];
            BitmapData bmData = bmLinesLimits.LockBits(new Rectangle(0, 0, bmLinesLimits.Width,
                                bmLinesLimits.Height), ImageLockMode.ReadWrite,
                                PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;

            unsafe
            {
                byte* scan0 = (byte*)bmData.Scan0.ToPointer();
                // byte* p = (byte*)(void*)Scan0;
                int bytesPerPixel = 3;
                Byte pixel;
                Boolean boolean, changeBool;
                boolean = changeBool = false;

                string[,] matrixBlackToWhite = new string[0, 0];
                int rowsB2W = 0; int columnsB2W = 0; Boolean bB2W = true;
                string[,] matrixWhiteToBlack = new string[0, 0];
                int rowsW2B = 0; int columnsW2B = 0; Boolean bW2B = true;

                int nOffset = stride - bmLinesLimits.Width * 3;
                Int16 result;


                for (int y = 0; y < bmLinesLimits.Height; ++y)
                {
                    byte* p = scan0 + (y * stride);
                    for (int x = 0; x < bmLinesLimits.Width; ++x)
                    {
                        int index = x * bytesPerPixel;
                        pixel = p[index];
                        result = pixel;
                        //Check if it has change from Black to White
                        if (result == 0xff)
                        {
                            boolean = true;
                            if (boolean != changeBool /*|| x==0 && result == 0xff*/)
                            {
                                changeBool = boolean;
                                matrixCreation(ref matrixBlackToWhite, ref rowsB2W, ref columnsB2W, ref bB2W, y, x);

                            }
                        }
                        //Check if it has change from White to black
                        //x == bmLinesLimits.Width && result == 0xff) This checks whether at the end of the image there's no change, but the image has ended.
                        else if (result == 0x00)
                        {
                            boolean = false;
                            if (boolean != changeBool /*|| (x == bmLinesLimits.Width && result == 0xff)*/)
                            {
                                changeBool = boolean;
                                matrixCreation(ref matrixWhiteToBlack, ref rowsW2B, ref columnsW2B, ref bW2B, y, x);
                            }
                        }
                    }

                    bW2B = bB2W = true;
                    columnsW2B = columnsB2W = 0;
                    //  matrixWhiteToBlack = ResizeArrayNxN(ref matrixWhiteToBlack, 3,3);

                }

                for (int i = 0; i < matrixWhiteToBlack.GetLength(1) - 1; i++)
                {
                    Int16 m = 0;
                    Int16 n = 0;
                    //matrix[matrix.GetLength(0) - 1 - n, i] == null
                    m = firstValueNotNull(ref matrixWhiteToBlack, i);
                    String[] W2B_Points1 = Regex.Split(matrixWhiteToBlack[0 + m, i], ",");

                    n = finalValueNotNull(ref matrixWhiteToBlack, i);
                    String[] W2B_Points2 = Regex.Split(matrixWhiteToBlack[matrixWhiteToBlack.GetLength(0) - 1 - n, i], ",");

                    m = firstValueNotNull(ref matrixBlackToWhite, i + 1);
                    String[] B2W_Points1 = Regex.Split(matrixBlackToWhite[0 + m, i + 1], ",");

                    n = finalValueNotNull(ref matrixBlackToWhite, i + 1);
                    String[] B2W_Points2 = Regex.Split(matrixBlackToWhite[matrixBlackToWhite.GetLength(0) - 1 - n, i + 1], ",");

                    Point point1 = new Point(Convert.ToInt16(W2B_Points1[0]), Convert.ToInt16(W2B_Points1[1]));
                    Point point2 = new Point(Convert.ToInt16(W2B_Points2[0]), Convert.ToInt16(W2B_Points2[1]));

                    Point point3 = new Point(Convert.ToInt16(B2W_Points1[0]), Convert.ToInt16(B2W_Points1[1]));
                    Point point4 = new Point(Convert.ToInt16(B2W_Points2[0]), Convert.ToInt16(B2W_Points2[1]));

                    // The shown order is to contruct the delimited area in a parking lot
                    Array.Resize(ref curvePoints, 4 * (i + 1));
                    curvePoints[0 + (4 * i)] = point1;
                    curvePoints[1 + (4 * i)] = point2;
                    curvePoints[2 + (4 * i)] = point4;
                    curvePoints[3 + (4 * i)] = point3;
                }
                parkingLotspaces = matrixWhiteToBlack.GetLength(1) - 1;
            }

            bmLinesLimits.UnlockBits(bmData);

        }

        public Bitmap cropImage(int initialX, int initialY, int width, int height, Bitmap originalImage)
        {
            Bitmap cropImg;
            cropImg = new Bitmap(width, height);
            cropImg = originalImage.Clone(new Rectangle(initialX, initialY, width, height), originalImage.PixelFormat);
            return cropImg;
        }

        public static void Binarization(Bitmap imageBlack_N_White)
        {
            BitmapData bmData = imageBlack_N_White.LockBits(new Rectangle(0, 0, imageBlack_N_White.Width,
                                imageBlack_N_White.Height), ImageLockMode.ReadWrite,
                                PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - imageBlack_N_White.Width * 3;
                byte a;
                float result;
                for (int y = 0; y < imageBlack_N_White.Height; ++y)
                {
                    for (int x = 0; x < imageBlack_N_White.Width; ++x)
                    {
                        a = p[0];
                        result = a;
                        if (result >= 0x96)
                        {
                            p[0] = p[1] = p[2] = 0xFF;
                        }
                        else
                        {
                            p[0] = p[1] = p[2] = 0x00;
                        }
                        p += 3;
                    }
                    p += nOffset;
                }
                imageBlack_N_White.UnlockBits(bmData);
            }
        }

        public static Bitmap GrayScale1(Bitmap b)
        {
            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            Console.Write(stride);
            System.IntPtr Scan0 = bmData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - b.Width * 3;

                byte red, green, blue;

                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < b.Width; ++x)
                    {
                        blue = p[0];
                        green = p[1];
                        red = p[2];

                        p[0] = p[1] = p[2] = (byte)(.299 * red + .587 * green + .114 * blue);

                        p += 3;
                    }
                    p += nOffset;
                }
            }

            b.UnlockBits(bmData);

            return b;
        }

        private Bitmap cutLot(Bitmap blackCanvas, Point[] ParkingPoint, int i)
        {
            Bitmap compoundImage = null;


            Point[] parkingLot = { ParkingPoint[0 + (4 * i)], ParkingPoint[1 + (4 * i)], 
                                         ParkingPoint[2 + (4 * i)], ParkingPoint[3 + (4 * i)] };


            List<AForge.IntPoint> corners = new List<AForge.IntPoint>();

            corners.Add(new AForge.IntPoint(ParkingPoint[3 + (4 * i)].X, ParkingPoint[3 + (4 * i)].Y));
            corners.Add(new AForge.IntPoint(ParkingPoint[0 + (4 * i)].X, ParkingPoint[0 + (4 * i)].Y));
            corners.Add(new AForge.IntPoint(ParkingPoint[1 + (4 * i)].X, ParkingPoint[1 + (4 * i)].Y));
            corners.Add(new AForge.IntPoint(ParkingPoint[2 + (4 * i)].X, ParkingPoint[2 + (4 * i)].Y));

            QuadrilateralTransformation Qfilter = new QuadrilateralTransformation(corners, 200, 200);

            Bitmap frame = new Bitmap("C:\\Users\\usurio\\Documents\\MATLAB\\resources\\parkingfull.png");
            Bitmap nframe = Qfilter.Apply(frame);

            BackwardQuadrilateralTransformation BQfilter = new BackwardQuadrilateralTransformation(nframe, corners);

            compoundImage = BQfilter.Apply(blackCanvas);
            return compoundImage;
        }

        private static Bitmap blackBackgound(Bitmap blackCanvas_nXm)
        {
            Bitmap blackBackgound;
            blackBackgound = new Bitmap(blackCanvas_nXm.Width, blackCanvas_nXm.Height);
            BitmapData bmData = blackBackgound.LockBits(new Rectangle(0, 0, blackBackgound.Width, blackBackgound.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;

            unsafe
            {
                Byte* scan0 = (Byte*)bmData.Scan0.ToPointer();
                // byte* p = (byte*)(void*)Scan0;
                int bytesPerPixel = 3;
                Byte pixel = 0x00;
                for (int y = 0; y < blackBackgound.Height; ++y)
                {
                    Byte* p = scan0 + (y * stride);
                    for (int x = 0; x < blackBackgound.Width; ++x)
                    {
                        int index = x * bytesPerPixel;
                        p[index] = pixel;
                    }
                }
            }
            blackBackgound.UnlockBits(bmData);
            return blackBackgound;
        }

    }
}
