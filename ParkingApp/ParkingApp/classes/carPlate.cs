using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using tessnet2;
using System.Windows.Forms;

using AForge.Imaging;

using AForge.Imaging.Filters;

namespace ParkingApp.classes
{
    class carPlate
    {
        Bitmap plateImg;
        String plateStr;
        String pUsingLot;

        public String PUsingLot
        {
            get { return this.pUsingLot; }
            set { this.pUsingLot = value; }
        }

        public Bitmap PlateImg
        {
            get { return this.plateImg; }
            set { this.plateImg = value; }
        }

        public String PlateStr
        {
            get { return this.plateStr; }
            set { this.plateStr = value; }
        }

        public void obtainPlateLetters()
        {            
            Bitmap image = AForge.Imaging.Image.Clone(this.plateImg, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            try
            {
                Tesseract ocr = new Tesseract();
                ocr.Init("tessdata", "eng", false);
                List<tessnet2.Word> result = ocr.DoOCR(image, Rectangle.Empty);
                foreach (Word word in result)
                {
                    this.plateStr = this.plateStr + word.Text;                    
                }
                //persistDB(plate,"A-3");
            }
            catch {}
        }

        public void setGrayScaleImg(){
            this.plateImg.Save(@"placa.jpg");
            ResizeBilinear filter = new ResizeBilinear(Convert.ToInt32(this.plateImg.Width * 3), 200);
            this.plateImg = filter.Apply(this.plateImg);           
            this.plateImg = Grayscale.CommonAlgorithms.BT709.Apply(this.plateImg);
        }

        public void binarizeImg() 
        {
           
            //Threshold filter = new Threshold(170 + 30);
            //filter.ApplyInPlace(this.plateImg);
           
        }

        public void invertImgPixels() {
            Invert filter = new Invert();
            filter.ApplyInPlace(this.plateImg);
            this.plateImg.Save(@"placaaa.jpg");
        }

        public void filterBlobs() {

            Rectangle[] rects;
            int cont = 0;
            Tesseract t;

            do
            {
                Threshold filtert = new Threshold(170 + cont++);
                filtert.ApplyInPlace(this.plateImg);

                int minW = 50;
                int minH = 60;
                int maxW = 150;

                BlobsFiltering filter = new BlobsFiltering();
                filter.MinWidth = minW;
                filter.MinHeight = minH;
                filter.MaxWidth = maxW;
                //filter.MaxHeight = c4;            

                filter.ApplyInPlace(this.plateImg);
                BlobCounter bc = new BlobCounter();
                bc.ProcessImage(this.plateImg);

                rects = bc.GetObjectsRectangles();

                this.plateStr = "";
                foreach (Rectangle r in rects) {
                    try
                    {
                        Tesseract ocr = new Tesseract();
                        ocr.Init("tessdata", "eng", false);
                        Bitmap letter = this.plateImg.Clone(r, PixelFormat.Format24bppRgb);

                        List<tessnet2.Word> result = ocr.DoOCR(letter, Rectangle.Empty);
                        
                        foreach (Word word in result)
                        {                            
                            char l = word.Text.ToString()[0];
                            if ((l > 64 && l < 91) || (l > 47 && l < 58))
                            {
                                this.plateStr = this.plateStr + word.Text;
                            }                            
                        }
                        //persistDB(plate,"A-3");
                    }
                    catch { }
                }
                

            } while (!(rects.Length == 6) && cont < 31 );
            MessageBox.Show("cont " + cont + " rects " + rects.Length);
            this.plateImg.Save(@"placa.jpg");
            
        }
    }
}
