using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static POS.frmLogin;

namespace POS
{
    public partial class FinancialReportForm : UserControl
    {
        public FinancialReportForm()
        {
            InitializeComponent();
            LoadFinancialYears(Session.UserID);
        }
        private void LoadFinancialYears(int userId)
        {
            using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();

                    // Updated query to filter by UserID
                    string query = "SELECT FinancialYearID, YearName FROM FinancialYears WHERE UserID = @UserID";

                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        // Add UserID as a parameter to prevent SQL injection
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            // Bind the results to the ComboBox
                            cmbReportYear.DataSource = dt;
                            cmbReportYear.DisplayMember = "YearName";
                            cmbReportYear.ValueMember = "FinancialYearID";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading financial years: " + ex.Message);
                }
            }
        }
        private void GenerateFinancialReport(int userId)
        {
            // Retrieve the selected Financial Year ID from the ComboBox
            if (cmbReportYear.SelectedValue == null)
            {
                MessageBox.Show("Please select a financial year.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int financialYearID = Convert.ToInt32(cmbReportYear.SelectedValue);

            using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();

                    // Query for Revenue and Cost of Sales
                    string reportQuery = @"
                SELECT 
                    IFNULL(SUM(s.quantity_sold * s.sale_price), 0) AS TotalRevenue,
                    IFNULL(SUM(s.quantity_sold * i.purchase_price), 0) AS TotalCostOfSales
                FROM sales s
                JOIN inventory i ON s.product_id = i.prod_id
                WHERE s.FinancialYearID = @FinancialYearID AND s.UserID = @UserID";

                    using (MySqlCommand cmdReport = new MySqlCommand(reportQuery, connect))
                    {
                        cmdReport.Parameters.AddWithValue("@FinancialYearID", financialYearID);
                        cmdReport.Parameters.AddWithValue("@UserID", userId);

                        using (MySqlDataReader reader = cmdReport.ExecuteReader())
                        {
                            double revenue = 0.0, costOfSales = 0.0, expenses = 0.0;

                            if (reader.Read())
                            {
                                revenue = reader.IsDBNull(reader.GetOrdinal("TotalRevenue")) ? 0.0 : reader.GetDouble("TotalRevenue");
                                costOfSales = reader.IsDBNull(reader.GetOrdinal("TotalCostOfSales")) ? 0.0 : reader.GetDouble("TotalCostOfSales");
                            }
                            reader.Close();

                            // Get total expenses using the GetOperatingExpenses method
                            expenses = GetOperatingExpenses(financialYearID, userId);

                            double grossProfit = revenue - costOfSales;
                            double netProfit = grossProfit - expenses;

                            // Display the results in a DataTable
                            DataTable dtReport = new DataTable();
                            dtReport.Columns.Add("Metric");
                            dtReport.Columns.Add("Amount (UGX)");

                            dtReport.Rows.Add("Total Revenue", $"{revenue:N0} UGX");
                            dtReport.Rows.Add("Total Cost of Sales", $"{costOfSales:N0} UGX");
                            dtReport.Rows.Add("Total Expenses", $"{expenses:N0} UGX");
                            dtReport.Rows.Add("Gross Profit", $"{grossProfit:N0} UGX");
                            dtReport.Rows.Add("Net Profit", $"{netProfit:N0} UGX");

                            dgvReport.DataSource = dtReport;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error generating report: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        // Method for retrieving operating expenses
        private double GetOperatingExpenses(int financialYearID, int userId)
        {
            double totalExpenses = 0.0;

            string query = @"
        SELECT IFNULL(SUM(Eamount), 0) AS TotalExpenses 
        FROM (
            SELECT Eamount FROM expenses WHERE FinancialYearID = @FinancialYearID AND UserID = @UserID
            UNION ALL
            SELECT Eamount FROM expenses_history WHERE FinancialYearID = @FinancialYearID AND UserID = @UserID
        ) AS E";

            using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@FinancialYearID", financialYearID);
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        object result = cmd.ExecuteScalar();
                        totalExpenses = result != null ? Convert.ToDouble(result) : 0.0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error retrieving operating expenses: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return totalExpenses;
        }
        // Stub for GetActiveFinancialYearID (replace with your implementation)
        private void btnGenerateReport_Click(object sender, EventArgs e)
        {
            try
            {
                // Ensure the current user ID is retrieved
                int currentUserID = Session.UserID; // Replace 'Session.UserID' with your actual logic for getting the logged-in user ID

                if (currentUserID <= 0)
                {
                    MessageBox.Show("User not authenticated. Please log in.", "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Validate the selected financial year
                if (cmbReportYear.SelectedValue != null)
                {
                    int selectedYearID = Convert.ToInt32(cmbReportYear.SelectedValue);

                    // Generate the report for the selected year and current user
                    GenerateFinancialReport(currentUserID);
                }
                else
                {
                    MessageBox.Show("Please select a financial year.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void dgvReport_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
