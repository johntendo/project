using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace POS
{
    class Invent
    {
        MySqlConnection connect = new MySqlConnection("server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV");

        public int ID { get; set; }
        public string Product_Id { get; set; }
        public string ProdName { get; set; }
        public string Category { get; set; }
        public int QTY { get; set; }
        public string Measure { get; set; }
        public decimal PurchaseP { get; set; }
        public decimal SaleP { get; set; }

        // Method to fetch all inventory data for a specific user
        public List<Invent> AllInventoryData(int userId)
        {
            List<Invent> listData = new List<Invent>();

            using (MySqlConnection connect = new MySqlConnection("server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();
                    string selectData = "SELECT * FROM inventory WHERE UserID = @userId"; // Filter by user_id
                    using (MySqlCommand cmd = new MySqlCommand(selectData, connect))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId); // Add user_id as a parameter

                        MySqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            Invent odata = new Invent();
                            odata.ID = Convert.ToInt32(reader["product_id"]);
                            odata.Product_Id = reader["prod_id"].ToString();
                            odata.ProdName = reader["product_name"].ToString();
                            odata.Category = reader["category"].ToString();
                            odata.QTY = Convert.ToInt32(reader["stock_quantity"]);
                            odata.Measure = reader["unit_of_measure"].ToString();
                            odata.PurchaseP = Convert.ToDecimal(reader["purchase_price"]);
                            odata.SaleP = Convert.ToDecimal(reader["selling_price"]);

                            listData.Add(odata);
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
