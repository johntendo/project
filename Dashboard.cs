using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static POS.frmLogin;

namespace POS
{
    public partial class Dashboard : UserControl
    {
        private P_LReport profitReport; // Declare the profitReport instance variable

        public Dashboard()
        {
            InitializeComponent();
            GetOperatingExpenses();
            LoadFinancialRatiosChart();
            LoadExpenses();
            Loadtochart(); // Load chart data
            InitializeProfitReport(); // Initialize the profit report
           // LoadGrossProfitChart();
        }

        private void InitializeProfitReport()
        {
            // Create and configure the P_LReport instance
            profitReport = new P_LReport();
            profitReport.Dock = DockStyle.Fill; // Example of filling the control space
            this.Controls.Add(profitReport); // Add it to the Dashboard user control
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            // You can handle any actions needed on load here
        }

        public void Loadtochart()
        {
            using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();

                    // Modify the query to filter by Session.UserID if your inventory data is user-specific
                    string query = "SELECT product_name, SUM(stock_quantity) AS TotalStock " +
                                   "FROM inventory " +
                                   "WHERE UserID = @UserID " +  // Filter by UserID
                                   "GROUP BY product_name";

                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        // Add parameter for the UserID
                        cmd.Parameters.AddWithValue("@UserID", Session.UserID);

                        MySqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            string category = reader["product_name"].ToString();
                            int totalStock = Convert.ToInt32(reader["TotalStock"]);

                            // Add data points to the chart
                            chart2.Series["Stock Level"].Points.AddXY(category, totalStock);
                        }
                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connect.Close(); // Ensure the connection is closed in all cases
                }
            }
        }
        private void guna2HtmlLabel2_Click(object sender, EventArgs e)
        {
            // Handle label click if needed
        }
        private double GetOperatingExpenses()
        {
            // Query the 'expenses' table to calculate total operating expenses
            double totalExpenses = 0.0;
            string queryy= @"
        SELECT (SELECT IFNULL(SUM(Eamount), 0) FROM expenses) + 
               (SELECT IFNULL(SUM(Eamount), 0) FROM expenses_history) AS TotalExpenses";

            using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                connect.Open();
                MySqlCommand cmd = new MySqlCommand(queryy, connect);

                object result = cmd.ExecuteScalar();
                totalExpenses = result != DBNull.Value ? Convert.ToDouble(result) : 0.0;
                //txtOE.Text = totalExpenses.ToString("C0", CultureInfo.CreateSpecificCulture("en-UG"));

            }

            return totalExpenses;
        }
        public int GetActiveFinancialYearID()
        {
            int activeYearID = 0;
            string query = "SELECT FinancialYearID FROM FinancialYears WHERE IsActive = TRUE LIMIT 1";

            using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            activeYearID = Convert.ToInt32(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error retrieving active financial year: " + ex.Message);
                }
            }

            return activeYearID;
        }





        public void LoadFinancialRatiosChart()
        {
            using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();
                    int activeYearID = GetActiveFinancialYearID();
                    int userID = Session.UserID; // Get the logged-in user's ID

                    string query = @"
            SELECT 
                SUM(s.quantity_sold * s.sale_price) AS TotalRevenue, 
                SUM(s.quantity_sold * i.purchase_price) AS TotalCostOfSales,
                (SUM(s.quantity_sold * s.sale_price) - SUM(s.quantity_sold * i.purchase_price)) AS GrossProfit
            FROM sales s
            JOIN inventory i ON s.product_id = i.prod_id
            WHERE s.FinancialYearID = @FinancialYearID
            AND s.UserID = @UserID"; // Filter by UserID

                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@FinancialYearID", activeYearID);
                        cmd.Parameters.AddWithValue("@UserID", userID); // Add user filter

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                double revenue = reader.GetDouble("TotalRevenue");
                                double costOfSales = reader.GetDouble("TotalCostOfSales");
                                double grossProfit = reader.GetDouble("GrossProfit");

                                // Calculate Gross Profit Margin
                                double grossProfitMargin = (grossProfit / revenue) * 100;

                                // For Net Profit, subtract expenses
                                double operatingExpenses = GetOperatingExpenses();
                                double netProfit = grossProfit - operatingExpenses;
                                double netProfitMargin = (netProfit / revenue) * 100;

                                // Assuming current assets and liabilities are entered manually or stored elsewhere
                                double currentAssets = 50000; // Example value
                                double currentLiabilities = 30000; // Example value
                                double currentRatio = currentAssets / currentLiabilities;

                                // Populate the line graph
                                chartRatios.Series["Gross Profit Margin"].Points.AddXY("Period", grossProfitMargin);
                                chartRatios.Series["Net Profit Margin"].Points.AddXY("Period", netProfitMargin);
                                chartRatios.Series["Current Ratio"].Points.AddXY("Period", currentRatio);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    connect.Close();
                }
            }
        }
        public void LoadExpenses()
        {
            using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();
                    int userID = Session.UserID; // Get the logged-in user's ID

                    string query = @"
            SELECT Ename, Eamount 
            FROM expenses
            WHERE UserID = @UserID"; // Filter by UserID

                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userID); // Add user filter

                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        chart1.Series["Expenses"].Points.Clear();
                        foreach (DataRow row in dt.Rows)
                        {
                            chart1.Series["Expenses"].Points.AddXY(row["Ename"].ToString(), Convert.ToDouble(row["Eamount"]));
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error" + ex);
                }
                finally
                {
                    connect.Close();
                }
            }
        }
        private void chart1_Click(object sender, EventArgs e)
        {
            // Handle chart1 click if needed
        }

        private void chart2_Click(object sender, EventArgs e)
        {
            // Handle chart2 click if needed
        }

        private void chart1_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2Panel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2HtmlLabel18_Click(object sender, EventArgs e)
        {

        }

        private void chartRatios_Click(object sender, EventArgs e)
        {
            //Loadtochart();
            LoadFinancialRatiosChart();
        }

        private void guna2HtmlLabel10_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel11_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void chart1_Click_2(object sender, EventArgs e)
        {
            LoadExpenses();
        }

        private void txtOE_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel7_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
