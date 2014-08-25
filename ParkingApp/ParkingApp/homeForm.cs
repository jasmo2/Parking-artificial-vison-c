using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;



using AForge;
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge.Video; //MJPEGStream 
using AForge.Video.DirectShow; //filterinfocollection, videocapturedevice


using ParkingApp.classes;
using System.Collections;
namespace ParkingApp
{
    public partial class homeForm : Form
    {

        private AsyncVideoSource asyncSource;
        private VideoCaptureDevice stream;
        //private MJPEGStream stream;
        public static parkingCamp parkingC;

        //public parkingCamp ParkingC
        //{
        //    get { return this.parkingC; }
        //    set { this.parkingC = value; }
        //}
        //public ble hashTable;
        public homeForm()
        {
            InitializeComponent();
        }       

        private void button1_Click(object sender, EventArgs e) //REGISTRO USUARUOS
        {
            this.Hide();
            registroUsuarios reg = new registroUsuarios();
            reg.Show();
                       
        }

        private void button3_Click(object sender, EventArgs e) //MONITOREO
        {
            this.Hide();
            monitoreo mon = new monitoreo();
            mon.Show();    
        }

        private void button5_Click(object sender, EventArgs e) //REPORTES
        {
            this.Hide();
            reportes rep = new reportes();
            rep.Show();  
        }

        private void button4_Click(object sender, EventArgs e) //SALIR
        {
            Application.Exit();
        }

        private void homeForm_Load(object sender, EventArgs e)
        {
            parkingC = new parkingCamp();           
            parkingC.zoneLimits();  //Se crea este objeto al pricipio del programa para obtener todos los puntos de los aparcamientos            

        }

       

       





       
    }
}
