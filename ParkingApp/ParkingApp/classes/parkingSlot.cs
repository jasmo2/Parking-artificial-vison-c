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
    class parkingSlot
    {
        public void QuadrilateralSlotsTransformation(Point[] curvePoints, Bitmap[] bmParkingSlots, Bitmap bmBackgound, int i)
        {
            List<AForge.IntPoint> corners = new List<AForge.IntPoint>();

            corners.Add(new AForge.IntPoint(curvePoints[3 + (4 * i)].X, curvePoints[3 + (4 * i)].Y));
            corners.Add(new AForge.IntPoint(curvePoints[0 + (4 * i)].X, curvePoints[0 + (4 * i)].Y));
            corners.Add(new AForge.IntPoint(curvePoints[1 + (4 * i)].X, curvePoints[1 + (4 * i)].Y));
            corners.Add(new AForge.IntPoint(curvePoints[2 + (4 * i)].X, curvePoints[2 + (4 * i)].Y));

            QuadrilateralTransformation filter = new QuadrilateralTransformation(corners, 200, 200);
            bmParkingSlots[i] = filter.Apply(bmBackgound);
        }

        public void PaintSlots(String parkingName, ref Hashtable parkinglist, int counter, Bitmap bmModel, Bitmap bmFrame, Bitmap[] SlotsFrames, Bitmap[] ParkingSlotsModel, Point[] ParkingPoint, Graphics graphics, parkingSlot parkings)
        {
            for (int i = 0; i < (counter); i++)
            {
                parkings.QuadrilateralSlotsTransformation(ParkingPoint, ParkingSlotsModel, bmModel, i); // 
                parkings.QuadrilateralSlotsTransformation(ParkingPoint, SlotsFrames, bmFrame, i);

                Subtract filter = new Subtract(ParkingSlotsModel[i]);
                Bitmap resultImage = filter.Apply(SlotsFrames[i]);

                Boolean boolean = false;
                int valueToCompare = 130;

                boolean = parkings.emptyOrFull(resultImage, valueToCompare);                       

                if (boolean)
                {                    
                    int cont = i + 1;
                    
                    //MessageBox.Show(parkingName + " full lot #" + cont);
                    parkinglist[parkingName + cont] = "Ocupado";
                    Color color = new Color();
                    color = Color.FromArgb(0x30FF0000);
                    Point[] parkingLot = { ParkingPoint[0 + (4 * i)], ParkingPoint[1 + (4 * i)], 
                                         ParkingPoint[2 + (4 * i)], ParkingPoint[3 + (4 * i)] };
                    graphics.DrawPolygon(new Pen(Color.Gray, 3), parkingLot);
                    graphics.FillPolygon(new SolidBrush(color), parkingLot);
                }
                else
                {
                    int resu = i + 1;
                    parkinglist[parkingName + resu] = "Vacio";
                    Point[] parkingLot = { ParkingPoint[0 + (4 * i)], ParkingPoint[1 + (4 * i)], 
                                         ParkingPoint[2 + (4 * i)], ParkingPoint[3 + (4 * i)] };
                    graphics.DrawPolygon(new Pen(Color.Gray, 3), parkingLot);
                    graphics.FillPolygon(new SolidBrush(Color.FromArgb(0x3000ff00)), parkingLot);
                }

            }


        }

        private Boolean emptyOrFull(Bitmap resultImage, int valueToCompare)
        {

            Boolean boolean = false;
            BitmapData bitmapdata = resultImage.LockBits(new Rectangle(0, 0, resultImage.Width, resultImage.Height),
                                                ImageLockMode.ReadOnly, resultImage.PixelFormat);
            unsafe
            {
                byte* scan0 = (byte*)bitmapdata.Scan0.ToPointer();
                int stride = bitmapdata.Stride;
                int bytesPerPixel = 3;

                byte red, green, blue;

                for (int y = 0; y < resultImage.Height; ++y)
                {
                    byte* row = scan0 + (y * stride);
                    for (int x = 0; x < resultImage.Width; ++x)
                    {
                        int bIndex = x * bytesPerPixel;
                        int gIndex = bIndex + 1;
                        int rIndex = gIndex + 1;

                        blue = row[bIndex];
                        green = row[gIndex];
                        red = row[rIndex];

                        float Av = (blue + green + red) / 3;
                        if (Av > valueToCompare)
                            boolean = true;

                    }
                }
            }

            resultImage.UnlockBits(bitmapdata);
            return boolean;
        }

        public static Hashtable compareCurrentAndPrevious(Hashtable current, Hashtable previous) {

            Hashtable changedLots = new Hashtable();
            foreach (String key in current.Keys) { 

               if (!current[key].Equals(previous[key])){

                   if (current[key].Equals("Ocupado"))
                   {
                       changedLots.Add(key, "Ocupado");
                   }
                   else {
                       changedLots.Add(key, "Vacio");
                   }
               }
            }
            return changedLots;                
        }

    }
}
