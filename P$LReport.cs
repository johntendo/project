using ClosedXML.Excel;
using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;
using static POS.frmLogin;

namespace POS
{
    public partial class P_LReport : UserControl
    {
        public P_LReport()
        {
            InitializeComponent();
            CalculateAndDisplayProfit();
        }

        private void btnCalculateProfit_Click(object sender, EventArgs e)
        {
            CalculateAndDisplayProfit();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            // Clear all text fields
            txtrev.Text = "";
            txtCOS.Text = "";
            txtGP.Text = "";
            txtNP.Text = "";
            txtOE.Text = "";
        }

        

        private void ExportToExcel()
        {
            try
            {
                using (XLWorkbook workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Profit and Loss Report");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "Metric";
                    worksheet.Cell(1, 2).Value = "Amount";

                    // Add data
                    worksheet.Cell(2, 1).Value = "Total Revenue";
                    worksheet.Cell(2, 2).Value = txtrev.Text;

                    worksheet.Cell(3, 1).Value = "Cost of Sales";
                    worksheet.Cell(3, 2).Value = txtCOS.Text;

                    worksheet.Cell(4, 1).Value = "Gross Profit";
                    worksheet.Cell(4, 2).Value = txtGP.Text;

                    worksheet.Cell(5, 1).Value = "Operating Expenses";
                    worksheet.Cell(5, 2).Value = txtOE.Text;

                    worksheet.Cell(6, 1).Value = "Net Profit";
                    worksheet.Cell(6, 2).Value = txtNP.Text;

                    // Adjust column widths
                    worksheet.Columns().AdjustToContents();

                    // Save file
                    using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                    {
                        saveFileDialog.Filter = "Excel Files|*.xlsx";
                        saveFileDialog.FileName = "ProfitAndLossReport";

                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            workbook.SaveAs(saveFileDialog.FileName);
                            MessageBox.Show("Profit and Loss report exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting to Excel: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalculateAndDisplayProfit()
        {
            var (totalRevenue, totalCostOfSales, grossProfit, netProfit) = ProfitCalculator();

            txtrev.Text = totalRevenue.ToString("C0", new System.Globalization.CultureInfo("en-UG"));
            txtCOS.Text = totalCostOfSales.ToString("C0", new System.Globalization.CultureInfo("en-UG"));
            txtGP.Text = grossProfit.ToString("C0", new System.Globalization.CultureInfo("en-UG"));
            txtNP.Text = netProfit.ToString("C0", new System.Globalization.CultureInfo("en-UG"));
            txtOE.Text = GetOperatingExpenses().ToString("C0", new System.Globalization.CultureInfo("en-UG"));
        }

        public (decimal totalRevenue, decimal totalCostOfSales, decimal grossProfit, decimal netProfit) ProfitCalculator()
        {
            decimal totalRevenue = 0;
            decimal totalCostOfSales = 0;
            decimal grossProfit;
            decimal netProfit;

            int activeYearID = GetActiveFinancialYearID();

            if (activeYearID == 0)
            {
                MessageBox.Show("No active financial year found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return (0, 0, 0, 0);
            }

            int userId = Session.UserID;

            string query = @"
                SELECT 
                    i.prod_id, i.purchase_price, s.sale_price, s.quantity_sold 
                FROM sales s
                INNER JOIN inventory i ON s.product_id = i.prod_id
                WHERE s.FinancialYearID = @FinancialYearID AND s.UserID = @UserID";

            using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@FinancialYearID", activeYearID);
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                decimal salePrice = reader.GetDecimal("sale_price");
                                decimal purchasePrice = reader.GetDecimal("purchase_price");
                                int quantitySold = reader.GetInt32("quantity_sold");

                                totalRevenue += salePrice * quantitySold;
                                totalCostOfSales += purchasePrice * quantitySold;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error fetching sales data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            grossProfit = totalRevenue - totalCostOfSales;
            decimal operatingExpenses = (decimal)GetOperatingExpenses();
            netProfit = grossProfit - operatingExpenses;

            return (totalRevenue, totalCostOfSales, grossProfit, netProfit);
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
                        else
                        {
                            MessageBox.Show("No active financial year found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error retrieving active financial year: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return activeYearID;
        }

        public double GetOperatingExpenses()
        {
            double totalExpenses = 0.0;
            int activeYearID = GetActiveFinancialYearID();

            if (activeYearID == 0) return totalExpenses;

            int userId = Session.UserID;

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
                        cmd.Parameters.AddWithValue("@FinancialYearID", activeYearID);
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

        private void btnprofit_Click(object sender, EventArgs e)
        {
            CalculateAndDisplayProfit();
        }

        private void btnExportToExcel_Click_1(object sender, EventArgs e)
        {
            ExportToExcel();
        }
    }
}
