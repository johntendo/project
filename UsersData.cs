using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace POS
{
    class UsersData
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public string  Date { get; set; }
        public List<UsersData> AllUsersData()
        {
            List<UsersData> listdata = new List<UsersData>();
            using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                connect.Open();
                string selectData = "SELECT * FROM user";
                using(MySqlCommand cmd=new MySqlCommand(selectData, connect))
                {
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        UsersData uData = new UsersData();
                        uData.ID = (int)reader["id"];
                        uData.Username = reader["username"].ToString();
                        uData.Password= reader["password"].ToString();
                        uData.Role = reader["role"].ToString();
                        uData.Status = reader["status"].ToString();
                        uData.Date = reader["date"].ToString();
                        listdata.Add(uData);
                    }

                }
            }
            return listdata;
        }
    }
}
