using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ParkingApp.classes;

using System.Net; //web client
using System.Web; //httputility
using Newtonsoft.Json; //JSON

namespace ParkingApp
{
    public partial class reportes : Form
    {
        public reportes()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            homeForm frm = new homeForm();
            this.Hide();
            frm.ShowDialog();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            WebClient webClient = new WebClient();            
            String query = "";

            if (radioButton1.Checked == true) //multas
            {
                query = "SELECT users.identification, users.plate, users.name AS user_name, parking_lots.name as parking_lot " +
                        "FROM fines, users, parking_lots WHERE fines.id_parking_lot = parking_lots.id AND fines.id_user = users.id " +
                        "ORDER BY fines.date DESC LIMIT 15";
            }
            else if (radioButton2.Checked == true) //usuarios
            {
                query = "SELECT * FROM users";
            }
            else //accesos
            {
                query = "SELECT users.plate, users.name AS user_name, parking_lots.name as parking_lot, parkings.date as date " +
                        "FROM parkings, users, parking_lots WHERE parkings.id_parking_lot = parking_lots.id AND parkings.id_user = users.id " +
                        "ORDER BY parkings.date desc";
            }

            System.Collections.Specialized.NameValueCollection data = new System.Collections.Specialized.NameValueCollection();
            data["action"] = "findReports";
            data["query"] = JsonConvert.SerializeObject(query);

            var response = webClient.UploadValues("http://localhost/parking/WSparking.php", "POST", data);
            //var response = client.UploadValues("http://www.eiaparking.tk/WSparking.php", "POST", data);
            
            String str = Encoding.ASCII.GetString(response);
            MessageBox.Show(str);
            str = '[' + str.Remove(0, 1);
            str = str.Remove(str.Length - 1, 1) + ']';
            //MessageBox.Show(str);

           DataTable tester = (DataTable)JsonConvert.DeserializeObject(str, (typeof(DataTable)));

            MessageBox.Show(tester.ToString());
        }
    }
}
