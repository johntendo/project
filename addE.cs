using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace POS
{
    class addE
    {
        public int Id { get; set; }
        public string Expense_Name { get; set; }
        public string Expense_Amount { get; set; }
        public DateTime Date { get; set; }

        // Updated method to include UserID
        public List<addE> addExpense(int userId)
        {
            List<addE> listData = new List<addE>();

            using (MySqlConnection connect = new MySqlConnection("server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();
                    string selectData = "SELECT * FROM expenses WHERE UserID = @UserID";

                    using (MySqlCommand cmd = new MySqlCommand(selectData, connect))
                    {
                        // Adding UserID parameter to the query
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                addE odata = new addE
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Expense_Name = reader["Ename"].ToString(),
                                    Expense_Amount = reader["Eamount"].ToString(),
                                    Date = (DateTime)reader["date"]
                                };

                                listData.Add(odata);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Connection Failed: " + ex.Message);
                }
                finally
                {
                    connect.Close();
                }
            }

            return listData;
        }
    }
}
