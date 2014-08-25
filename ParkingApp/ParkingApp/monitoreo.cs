using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

using tessnet2;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge;
using AForge.Math.Geometry;

using System.Threading;
using System.Timers;
using ParkingApp.classes;
using System.Collections;

namespace ParkingApp
{
    public partial class monitoreo : Form
    {
        private Bitmap bgFrame, currentFrame;

        //private BitmapData bmData;
        AsyncVideoSource asyncSource;
        
        private int i, cont;
        private Boolean mov, mov2;
        parkingCamp parkingC = homeForm.parkingC;
        private carPlate carPlate;

        public monitoreo()
        {
            InitializeComponent();
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            /*
            FilterInfoCollection CamCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            VideoCaptureDevice stream = new VideoCaptureDevice(CamCollection[0].MonikerString);
            */

            MJPEGStream stream = new MJPEGStream("http://192.168.0.16:8080/videofeed");
           stream.Login = "u";
           stream.Password = "p";           
           
            mov2 = false;
            mov = true;

            framesProcessing.fWidth = 640*2;
            framesProcessing.fHeight = 720;

            //Bitmap plate = new Bitmap("C:\\Users\\User\\Desktop\\tesis\\placa4.png");
            //getPlateLetters(plate);
            //persistDBLog("KFM276", "A-3");
            //Bitmap a = new Bitmap(@"placa1.jpg");
            //pictureBox3.Image = a;
            
            //pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
            //catchPlate(a);
            
            //Set Timer <-----------------
            
            timer1.Tick += new EventHandler(timer_tick);
            timer1.Interval = 1000; //miliseconds
            timer1.Start();

            cont = 0;
            asyncSource = new AsyncVideoSource(stream);

            // set NewFrame event handler
            asyncSource.NewFrame += new NewFrameEventHandler(video_NewFrame);

            // start the video source            
            asyncSource.Start();
        }

        public void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // get new frame
            CheckForIllegalCrossThreadCalls = false;
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            if (pictureBox1.Image == null)
            {
                bitmap.Save(@"modelo0.jpg");
                bgFrame = bitmap;
            }
            else
            {
                try
                {
                    bgFrame = (Bitmap)pictureBox1.Image.Clone();
                    //bitmap.Save(@"currImage.jpg");
                    currentFrame = (Bitmap)bitmap.Clone();

                    Bitmap gcurrent = Grayscale.CommonAlgorithms.BT709.Apply(bitmap);
                    Bitmap gbground = Grayscale.CommonAlgorithms.BT709.Apply(bgFrame);
                    
                    Difference differenceFilter = new Difference();
                    differenceFilter.OverlayImage = gbground;
                    Bitmap currentImg = differenceFilter.Apply(gcurrent);

                    //bloqueo temporal de current image
                    BitmapData currentData = currentImg.LockBits(new Rectangle(0, 0, framesProcessing.fWidth,
                                        framesProcessing.fHeight), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);


                    //**Thresholdfilter
                    Threshold thresholdFilter = new Threshold(15);
                    thresholdFilter.ApplyInPlace(currentData);
                    IFilter erosionFilter = new Erosion();
                    Bitmap tmp2 = erosionFilter.Apply(currentData);

                    currentImg.UnlockBits(currentData);                                      
                    i = framesProcessing.CalculateWhitePixels(tmp2);
                    
                    if (i > 300)
                    {
                        mov = mov2 = true;                                               
                    }
                    else
                        mov = false;                    

                    pictureBox2.Image = currentImg;
                }
                catch{//MessageBox.Show(ex.Message.ToString());
                }
               
            }
            pictureBox1.Image = bitmap;
            
        }        
                
        public void timer_tick(object source, EventArgs e)
        {
            if (mov2 == true)
            {
                if (mov == false) //verificación de cuando el movieento haya cesado
                {
                    cont++;
                    if (cont == 10)
                    {

                        cont = 0;
                        //catchPlate((Bitmap)pictureBox1.Image);                        
                        //MessageBox.Show("No Movimiento");

                        webServiceInteraction wServInt = new webServiceInteraction(); 
                        parkingC.LastParkingStatus = wServInt.getParkingStatus();// obtener estado anterior del parqueadero

                        Hashtable pCurrentList = OccupancyChecker();
                        Hashtable changedLots = parkingSlot.compareCurrentAndPrevious(pCurrentList, parkingC.LastParkingStatus);  //cuales aparcamientos cambiaron y su nuevo estado                    

                        foreach (String key in changedLots.Keys) {
                            wServInt.updateSlotStatus(key, changedLots[key].ToString());
                            if (changedLots[key].Equals("Ocupado"))
                            {
                                carPlate = new carPlate();
                                carPlate.PUsingLot = key;
                                LincensePlate(key);
                            }                           
                   
                        }
                        //
                        mov2 = false;
                    }
                }
                else { cont = 0; }
            }
        }

        private void catchPlateImg(Bitmap original)
        {

            framesProcessing fprocessing = new framesProcessing();

            Bitmap rgb = (Bitmap)original.Clone();
            Bitmap hsv = fprocessing.rgb2hsv((Bitmap)original.Clone());

            //Binarization:
            fprocessing.binarization(ref rgb, ref hsv);

            //Common bits bwn rgb and hsv
            Bitmap commImg = fprocessing.commonBits(rgb, hsv);

            //Image dilatation
            Bitmap dilatedImg = fprocessing.diamonDilatation(commImg);

            //get the plate n times ?????

            try
            {
                carPlate.PlateImg = fprocessing.getBiggestBloob(commImg, original);
                carPlate.PlateImg.Save(@"placa.jpg");
                //carPlate.PlateImg = fprocessing.getBiggestBloob(commImg, original);

                pictureBox7.Image = carPlate.PlateImg;
                //pictureBox7.SizeMode = PictureBoxSizeMode.StretchImage;

                //do image plate filter process
                carPlate.invertImgPixels();
                carPlate.setGrayScaleImg();
                carPlate.binarizeImg();
                
                carPlate.filterBlobs();

                //do tesseract process
                carPlate.obtainPlateLetters();
                webServiceInteraction wsint = new webServiceInteraction();
                wsint.persistDBLog(carPlate.PlateStr, carPlate.PUsingLot);
               
                //MessageBox.Show(carPlate.PlateStr);
                textBox1.Text = (carPlate.PlateStr);
            }
            catch (Exception)
            {
                
                

            }

        }         

        private void LincensePlate(String key) //obtener el aparcamiento ocupado
        {

            int upperCont = parkingC.UpperParkingPoint.Length / 4;
            int downerCont = parkingC.DownParkingPoints.Length / 4;

            Bitmap compoundImage = null;
            Bitmap bmFrame = parkingC.CurrentFrame.Clone() as Bitmap; //Clone the current Frame
            Bitmap blackCanva = new Bitmap(framesProcessing.fWidth, framesProcessing.fHeight);
            blackCanva = blackBackgound(blackCanva); // Creation of the black frame

            Bitmap blackCanvas = blackCanva.Clone(new Rectangle(0,
                0, blackCanva.Width, blackCanva.Height),
                PixelFormat.Format24bppRgb);

                    char[] arr = key.ToCharArray();
                    if (arr[0].Equals('A')) // upside
                    {
                        char a = arr[1];
                        int i = Convert.ToInt16(new string(a, 1));
                        i--;
                        compoundImage = cutLot(blackCanvas, parkingC.UpperParkingPoint, i);

                    }
                    else if (arr[0].Equals('B')) // Downside
                    {
                        char a = arr[1];
                        int i = Convert.ToInt16(new string(a, 1));
                        i--;
                        compoundImage = cutLot(blackCanvas, parkingC.DownParkingPoints, i); // crop the l
                    }
                    pictureBox3.Image = compoundImage;
                    
                    compoundImage.Save(@"iamgen.jpg");
                    // Aquí comienza la lectura de placa con "compoundImage"   
                    catchPlateImg(compoundImage);                      

                }
                    
        private Hashtable OccupancyChecker()
        {
            // Se pintan que aparcamientos estan ocupados.
           // parkingC = homeForm.parkingC;
            Bitmap cFrame = null;
            Bitmap mFrame = null;
            do
            {
                try {
                    cFrame = (Bitmap)currentFrame.Clone();
                    mFrame = new Bitmap(@"lastImage.jpg");
                }
                
                catch {
                    cFrame = null;
                    mFrame = null;
                }

            } while (cFrame == null || mFrame == null);
                     
 
            parkingC.rgb2hsv(cFrame);
            parkingC.rgb2hsv(mFrame);
            
            //MessageBox.Show("Compare process about to begin. The RGB image has been change to HSV colour space, which will shows only Saturation");

            int upperCont = parkingC.UpperParkingPoint.Length / 4;
            int downerCont = parkingC.DownParkingPoints.Length / 4;

            Bitmap[] upperSlotsFrames = new Bitmap[(upperCont)];
            parkingC.UpperParkingSlotsModel = new Bitmap[(upperCont)];

            Bitmap[] downParkingSlotsFrames = new Bitmap[(downerCont)];
            parkingC.DownParkingSlotsModel = new Bitmap[(downerCont)];

            //Bitmap paintedF = new Bitmap(@"currImage.jpg");
            Bitmap paintedF = (Bitmap)currentFrame.Clone();

            Graphics graphics = Graphics.FromImage(paintedF);
            parkingSlot parkings = new parkingSlot();

            Hashtable parkingList = parkingC.ParkingList;

            parkings.PaintSlots("A", ref parkingList, upperCont, mFrame, cFrame, upperSlotsFrames, parkingC.UpperParkingSlotsModel, parkingC.UpperParkingPoint, graphics, parkings);
            parkings.PaintSlots("B", ref parkingList, downerCont, mFrame, cFrame, downParkingSlotsFrames, parkingC.DownParkingSlotsModel, parkingC.DownParkingPoints, graphics, parkings);

            pictureBox4.Image = paintedF;
           // pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
            

            return parkingList;
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
        
        private Bitmap cutLot(Bitmap blackCanvas, System.Drawing.Point[] ParkingPoint, int i)
        {
            Bitmap compoundImage = null;
            System.Drawing.Point[] parkingLot = { ParkingPoint[0 + (4 * i)], ParkingPoint[1 + (4 * i)], 
                                         ParkingPoint[2 + (4 * i)], ParkingPoint[3 + (4 * i)] };

            List<AForge.IntPoint> corners = new List<AForge.IntPoint>();

            corners.Add(new AForge.IntPoint(ParkingPoint[3 + (4 * i)].X, ParkingPoint[3 + (4 * i)].Y));
            corners.Add(new AForge.IntPoint(ParkingPoint[0 + (4 * i)].X, ParkingPoint[0 + (4 * i)].Y));
            corners.Add(new AForge.IntPoint(ParkingPoint[1 + (4 * i)].X, ParkingPoint[1 + (4 * i)].Y));
            corners.Add(new AForge.IntPoint(ParkingPoint[2 + (4 * i)].X, ParkingPoint[2 + (4 * i)].Y));
            
            QuadrilateralTransformation Qfilter = new QuadrilateralTransformation(corners, 200, 200);
            
            //Bitmap frame = new Bitmap(@"currImage.jpg");
            Bitmap frame = (Bitmap)currentFrame.Clone();

            Bitmap nframe = Qfilter.Apply(frame);
            nframe.RotateFlip(RotateFlipType.Rotate180FlipY);

            BackwardQuadrilateralTransformation BQfilter = new BackwardQuadrilateralTransformation(nframe, corners);

            compoundImage = BQfilter.Apply(blackCanvas);
            return nframe;
        }

        
        private void dbParkingOccupancy() 
        {

        }


        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                asyncSource.Stop();
                timer1.Stop();
            }
            catch
            {
            }

            homeForm frm = new homeForm();
            this.Hide();
            frm.ShowDialog();
            this.Close();
        }

        private void monitoreo_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }             

    }    
}
