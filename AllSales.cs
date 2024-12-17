using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace POS
{
    class AllSales
    {
        public int ID { get; set; }
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public int QuantitySold { get; set; }
        public decimal SalePrice { get; set; }
        public DateTime SaleDate { get; set; }

        // Method to retrieve sales data filtered by UserID
        public List<AllSales> AllSalesData(int userId)
        {
            List<AllSales> listData = new List<AllSales>();

            using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();

                    // SQL query with UserID filtering
                    string selectdata = "SELECT * FROM sales WHERE UserID = @UserID";

                    using (MySqlCommand cmd = new MySqlCommand(selectdata, connect))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId); // Filter by UserID
                        MySqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            AllSales odata = new AllSales();

                            // Handle null values safely
                            odata.ID = reader["sale_id"] != DBNull.Value ? Convert.ToInt32(reader["sale_id"]) : 0;
                            odata.ProductID = reader["product_id"] != DBNull.Value ? reader["product_id"].ToString() : "N/A";
                            odata.ProductName = reader["product_name"] != DBNull.Value ? reader["product_name"].ToString() : "N/A";
                            odata.QuantitySold = reader["quantity_sold"] != DBNull.Value ? Convert.ToInt32(reader["quantity_sold"]) : 0;
                            odata.SalePrice = reader["sale_price"] != DBNull.Value ? Convert.ToDecimal(reader["sale_price"]) : 0;
                            odata.SaleDate = reader["sale_date"] != DBNull.Value ? Convert.ToDateTime(reader["sale_date"]) : DateTime.MinValue;

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
