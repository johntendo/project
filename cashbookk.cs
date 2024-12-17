using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS
{
    class cashbookk
    {
        public int id { get; set; }
        public DateTime Transaction_date { get; set; }
        public string Transaction_type { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }

        // Assuming session.UserID is available somehow in this context, possibly passed to this method.
        public int UserID { get; set; }  // You can set this from your session

        public List<cashbookk> AllCashData(int userId)
        {
            List<cashbookk> cateDatas = new List<cashbookk>();
            using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                connect.Open();
                // Adjust the SQL to filter by UserID
                string selectData = "select * from cashbook where transaction_type='Cash' and UserID=@UserID";
                using (MySqlCommand cmd = new MySqlCommand(selectData, connect))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);  // Passing UserID into the query

                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        cashbookk uData = new cashbookk();
                        uData.id = (int)reader["id"];
                        uData.Transaction_date = (DateTime)reader["transaction_date"];
                        uData.Transaction_type = reader["transaction_type"].ToString();
                        uData.Description = reader["description"].ToString();
                        uData.Amount = (decimal)reader["amount"];
                        cateDatas.Add(uData);
                    }
                }
                return cateDatas;
            }
        }

        public List<cashbookk> AllCreditData(int userId)
        {
            List<cashbookk> cateDatas = new List<cashbookk>();
            using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                connect.Open();
                // Adjust the SQL to filter by UserID
                string selectData = "select * from cashbook where transaction_type='Credit' and UserID=@UserID";
                using (MySqlCommand cmd = new MySqlCommand(selectData, connect))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);  // Passing UserID into the query

                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        cashbookk uData = new cashbookk();
                        uData.id = (int)reader["id"];
                        uData.Transaction_date = (DateTime)reader["transaction_date"];
                        uData.Transaction_type = reader["transaction_type"].ToString();
                        uData.Description = reader["description"].ToString();
                        uData.Amount = (decimal)reader["amount"];
                        cateDatas.Add(uData);
                    }
                }
                return cateDatas;
            }
        }
    }
}
