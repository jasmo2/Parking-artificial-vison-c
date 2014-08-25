using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ParkingApp.classes;

using System.Net; //web client
//using System.Web; //httputility
using Newtonsoft.Json; //JSON

using System.IO.Ports;

namespace ParkingApp
{
    public partial class registroUsuarios : Form
    {
        public registroUsuarios()
        {
            InitializeComponent();
        }

        int con = 0;
        SerialPort serialPort1;
        bool bFlag = false;
        byte[] commandLogin = new byte[12] { 0xBA, 0x0A, 0x02, 0x01
                                        , 0xAA, 0xFF, 0xFF, 0xFF,
                                          0xFF, 0xFF, 0xFF, 0x00 };
        //Vector actual para conectarse a un sector que se crea con base al predeterminado
        byte[] commandConect = new byte[12];
        //Comando predeterminado para leer un bloque
        byte[] commandReadBlock = { 0xBA, 0x03, 0x03, 0x04, 0x00 };
        //Vector actual para leer que se crea con base al predeterminado
        byte[] commandRead = new byte[5];
        //Comando predeterminado para escribir en un bloque
        byte[] commandWriteBlock = { 0xBA, 0x13, 0x04, 0x04,
                           0x00, 0x00, 0x00, 0x00,
                           0x00, 0x00, 0x00, 0x00,
                           0x00, 0x00, 0x00, 0x00,
                           0x00, 0x00, 0x00, 0x00, 0x00 };
        //Vector actual para escribir que se crea con base al predeterminado
        byte[] commandWrite = new byte[21];
        // RFID needed registers

        private void button1_Click(object sender, EventArgs e)
        {
            homeForm frm = new homeForm();
            this.Hide();
            frm.ShowDialog();
            this.Close();
        }
        private void sectorLogin(int sector)
        {
            commandConect = commandLogin;
            commandConect[3] = (byte)sector;
            //check sum hecho para BA0A02_03_AAFFFFFFFFFFFF_Cs
            commandConect[commandConect.Length - 1] = Checkesum(commandConect);
            //commandConect[commandConect.Length - 1] = 0x19;//25=0x19  
            //Se escribe para acceder al sector
            serialPort1.Write(commandConect, 0, commandConect.Length);
            System.Threading.Thread.Sleep(20);
        }
        private void resetCard()
        {
            for (int i = 4; i < commandWrite.Length - 1 - 1; i++)
            {
                commandWrite[i] = 0x00;
            }
        }
        private void givingInfo(String sID)
        {
            char[] cID = sID.ToCharArray(0, sID.Length);
            for (int i = 0; i < cID.Length; i++)
            {
                byte bID = (byte)Convert.ToInt16(cID[i]);
                commandWrite[i + 4] = bID;
            }
            commandWrite[commandWrite.Length - 1] = Checkesum(commandWrite);
        }
        private byte Checkesum(byte[] array)
        {
            byte result = array[0];
            int length = array.Length - 1;
            for (int i = 1; i < length; i++)
            {
                result = X_OR(result, array[i]);
            }


            return result;
        }
        private byte X_OR(byte result, byte p)
        {
            byte bNum;
            String sResult = Convert.ToString(result ^ p, 10);
            bNum = (byte)Convert.ToInt16(sResult);
            return bNum;
        }

        private void registroUsuarios_Load(object sender, EventArgs e)
        {
            
        }
        private void serialport_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                con++;
                //vector to recieve data
                int bytesInBuff = serialPort1.BytesToRead;
                byte[] dataBuff = new byte[bytesInBuff];
                serialPort1.Read(dataBuff, 0, bytesInBuff);
                this.BeginInvoke(new LineReceivedEvent(LineReceived), dataBuff);
            }
            catch (Exception)
            { }
        }
        private delegate void LineReceivedEvent(byte[] dataInHex);
        private void LineReceived(byte[] dataInHEX)
        {
            if (dataInHEX.Length >= 21 && bFlag)
            {
                //OBTAIN the total value of the vector
                int lenght = dataInHEX.Length;
                lenght -= 5;
                Char[] readebleByte = new Char[lenght];
                for (int i = 0; i < lenght; i++)
                    readebleByte[i] = (Char)dataInHEX[i + 5 - 1];

                String dataRFID;
                dataRFID = new String(readebleByte);
                // StudetTextBox.Text = dataRFID;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {            
            WebClient client = new WebClient();
            parkingUser pUser = new parkingUser();
            pUser.UserName = (String)textBox1.Text; ;
            pUser.UserPlate = (String)textBox3.Text;
            pUser.UserIdent = Convert.ToInt32((String)textBox2.Text); ;

            System.Collections.Specialized.NameValueCollection data = new System.Collections.Specialized.NameValueCollection();
            data["action"] = "insertUser";
            data["user"] = JsonConvert.SerializeObject(pUser);

            var response = client.UploadValues("http://localhost/parking/WSparking.php", "POST", data);
            //var response = client.UploadValues("http://www.eiaparking.tk/WSparking.php", "POST", data);
            pUser.UserId = Convert.ToInt32(Encoding.ASCII.GetString(response));


            if (pUser.UserId != 0)
            {
                MessageBox.Show("Usuario registrado con éxito con id: " + pUser.UserId);
            }
            else {
                MessageBox.Show("Falló al registrar usuario");
            }
                                    
            /////-------------iD RETORNADO POR LA BASE DE DATOS------------------////////

            openSerialConn();
            String sID = pUser.UserId + "";
            //MessageBox.Show("new id is: ", sID);
            sectorLogin(0x01);
            commandWrite = commandWriteBlock;
            commandWrite[3] = 0x04;
            resetCard();
            givingInfo(sID);
            bFlag = false;
            serialPort1.Write(commandWrite, 0, commandWrite.Length);
            serialPort1.Close();

        }

        public void openSerialConn()
        {
            serialPort1 = new SerialPort();
            try
            {
                string[] ports = SerialPort.GetPortNames();
                serialPort1.PortName = ports[0];//cuando esta con pruebas con Arduino para visualizar revizar al conectar los dipositivos
                serialPort1.BaudRate = 115200;
                serialPort1.Open();

                serialPort1.DataReceived += serialport_DataReceived;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Holy Cr..." + ex.ToString());

            }
        }

    }
}
