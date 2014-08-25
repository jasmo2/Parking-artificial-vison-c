using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using System.Net; //web client
using Newtonsoft.Json; //JSON

namespace ParkingApp.classes
{
    class webServiceInteraction
    {
        private string url = "http://192.168.0.17/parking/WSparking.php";
        //private string url = "http://www.eiaparking.tk/WSparking.php";

        public void persistDBLog(String plate, String slot)
        {
            WebClient client = new WebClient();
            System.Collections.Specialized.NameValueCollection data = new System.Collections.Specialized.NameValueCollection();
            data["action"] = "insertLog";
            data["plate"] = JsonConvert.SerializeObject(plate);
            data["slot"] = JsonConvert.SerializeObject(slot);

            var response = client.UploadValues(url, "POST", data);
            
            int id = Convert.ToInt32(Encoding.ASCII.GetString(response));
            if (id != 0)
            {
                //MessageBox.Show("Registro con id: " + id);
            }
            else
            {
                //MessageBox.Show("Registro falló");
            }
            
        }

        public void updateSlotStatus(String slot, String status)
        {
            WebClient client = new WebClient();
            System.Collections.Specialized.NameValueCollection data = new System.Collections.Specialized.NameValueCollection();
            data["action"] = "updateSlotStatus";
            data["slot"] = JsonConvert.SerializeObject(slot);
            data["status"] = JsonConvert.SerializeObject(status);

            var response = client.UploadValues(url, "POST", data);            
            int id = Convert.ToInt32(Encoding.ASCII.GetString(response));

        }

        public Hashtable getParkingStatus() {

            WebClient webClient = new WebClient();
            String query = "SELECT name, status FROM parking_lots;";

            System.Collections.Specialized.NameValueCollection data = new System.Collections.Specialized.NameValueCollection();
            data["action"] = "findReports";
            data["actionType"] = "parkingStatus";
            //data["query"] = JsonConvert.SerializeObject(query);
            data["query"] = query;

            var response = webClient.UploadValues(url, "POST", data);
            //var response = client.UploadValues(url, "POST", data);

            String str = Encoding.UTF8.GetString(response);
            List<string[]> data2 = JsonConvert.DeserializeObject<List<string[]>>(str);
            Hashtable lotsDB = new Hashtable();

            foreach (string[] lot in data2)
            {
                lotsDB.Add(lot[0].ToString(), lot[1].ToString());
            }

            return lotsDB;
        }
    }
}
