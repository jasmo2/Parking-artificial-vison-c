using System;
using System.Collections.Generic;
using System.Text;

namespace ParkingApp.classes
{
    class parkingUser
    {
        private String userName;
        private String userPlate;
        private int userIdent;
        private int userId;

        public int UserId
        {
            get { return userId; }
            set { userId = value; }
        }

        public String UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        public String UserPlate
        {
            get { return userPlate; }
            set { userPlate = value; }
        }

        public int UserIdent
        {
            get { return userIdent; }
            set { userIdent = value; }
        }
    }
}
