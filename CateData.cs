using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace POS
{
    class CateData
    {
        public int ID { get; set; }
        public string Category { get; set; }
        public string Date { get; set; }

        // Fetch all category data for the logged-in user
        public List<CateData> AllCategoryData(int userID)
        {
            List<CateData> cateDatas = new List<CateData>();
            using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                connect.Open();
                string selectData = "SELECT * FROM Category WHERE UserID = @userID"; // Assuming there's a user_id column in the Category table
                using (MySqlCommand cmd = new MySqlCommand(selectData, connect))
                {
                    cmd.Parameters.AddWithValue("@userID", userID);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        CateData uData = new CateData();
                        uData.ID = (int)reader["id"];
                        uData.Category = reader["category"].ToString();
                        uData.Date = reader["date"].ToString();
                        cateDatas.Add(uData);
                    }
                }
            }
            return cateDatas;
        }

        // Search category data by category name or date for the logged-in user
        public List<CateData> SearchCategoryData(string searchText, int userID)
        {
            List<CateData> searchResults = new List<CateData>();
            using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                connect.Open();

                string query = @"
                    SELECT id, category, date
                    FROM category
                    WHERE UserID = @userID
                      AND (category LIKE @searchText 
                           OR DATE_FORMAT(date, '%Y-%m-%d') LIKE @searchText)";

                using (MySqlCommand cmd = new MySqlCommand(query, connect))
                {
                    cmd.Parameters.AddWithValue("@userID", userID);
                    cmd.Parameters.AddWithValue("@searchText", "%" + searchText + "%");

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CateData uData = new CateData();
                            uData.ID = (int)reader["id"];
                            uData.Category = reader["category"].ToString();
                            uData.Date = reader["date"].ToString();
                            searchResults.Add(uData);
                        }
                    }
                }
            }
            return searchResults;
        }
    }
}
