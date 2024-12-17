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
    public partial class InventoryEvaluation : Form
    {
        public InventoryEvaluation()
        {
            InitializeComponent();
            GenerateInventoryValuationReport();
        }

        private void GenerateInventoryValuationReport()
        {

            int activeYearID = GetActiveFinancialYearID(); // Ensure financial year is considered
            if (activeYearID == 0)
            {
                MessageBox.Show("No active financial year found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int userId = Session.UserID; // Retrieve UserID from the session

            using (MySqlConnection connect = new MySqlConnection("server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();

                    string query = @"
                SELECT product_id, product_name, category, stock_quantity, purchase_price, 
                       (stock_quantity * purchase_price) AS total_value 
                FROM inventory
                WHERE UserID = @UserID AND FinancialYearID = @FinancialYearID";

                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        // Bind parameters for filtering
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@FinancialYearID", activeYearID);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            decimal totalInventoryValue = 0;

                            // Define UGX as currency
                            var ugxFormat = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
                            ugxFormat.CurrencySymbol = "UGX";


                            var report = new StringBuilder();
                            report.AppendLine("Inventory Valuation Report");
                            report.AppendLine("-------------------------------------------------------------------------------");
                            report.AppendLine($"{"ID",-10}{"Name",-20}{"Category",-15}{"Quantity",-10}{"Unit Price",-15}{"Total Value",-15}");
                            report.AppendLine("-------------------------------------------------------------------------------");

                            while (reader.Read())
                            {
                                int productId = reader.GetInt32("product_id");
                                string productName = reader.GetString("product_name");
                                string category = reader.GetString("category");
                                decimal stockQuantity = reader.GetDecimal("stock_quantity");
                                decimal purchasePrice = reader.GetDecimal("purchase_price");
                                decimal totalValue = reader.GetDecimal("total_value");

                                totalInventoryValue += totalValue;

                                // Format each line with specific column widths
                                report.AppendLine(String.Format("{0,-10}{1,-20}{2,-15}{3,-10}{4,-15}{5,-15}",
                                    productId,
                                    productName,
                                    category,
                                    stockQuantity,
                                    purchasePrice.ToString("C", ugxFormat),
                                    totalValue.ToString("C", ugxFormat)
                                ));
                            }

                            report.AppendLine("-------------------------------------------------------------------------------");
                            report.AppendLine($"Total Inventory Value: {totalInventoryValue.ToString("C", ugxFormat)}");

                            // Debug: Check the content before displaying
                            Console.WriteLine(report.ToString());

                            // Display the report in the RichTextBox
                            richTextBoxReport.Text = report.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error generating inventory valuation report: " + ex.Message);
                }
                finally
                {
                    connect.Close();
                }
            }
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


        private void ExportToExcel(DataTable dataTable, string filePath)
        {
            var excelApp = new Microsoft.Office.Interop.Excel.Application();
            var workbook = excelApp.Workbooks.Add(Type.Missing);
            var worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.ActiveSheet;
            worksheet.Name = "Inventory Valuation";

            try
            {
                // Add headers
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    worksheet.Cells[1, i + 1] = dataTable.Columns[i].ColumnName;
                }

                // Add rows
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        worksheet.Cells[i + 2, j + 1] = dataTable.Rows[i][j];
                    }
                }

                // Save the file
                workbook.SaveAs(filePath);
                workbook.Close();
                excelApp.Quit();
                MessageBox.Show("Report exported successfully to Excel.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting to Excel: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                workbook = null;
                worksheet = null;
                excelApp = null;
            }

        }



        private DataTable ConvertReportToDataTable(string reportContent)
        {
            // Create a DataTable to hold the report data
            DataTable dataTable = new DataTable();

            // Split the report into lines
            string[] lines = reportContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            // Ensure there are enough lines to parse
            if (lines.Length > 3)
            {
                // Parse headers (Assuming headers are in line 2)
                string headerLine = lines[1]; // Assuming the header is on the second line
                string[] headerColumns = headerLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var column in headerColumns)
                {
                    dataTable.Columns.Add(column);
                }

                // Parse rows (Skip headers and divider lines)
                for (int i = 3; i < lines.Length; i++) // Starting after the header and divider
                {
                    string line = lines[i];
                    if (!line.Contains("----")) // Skip divider lines
                    {
                        string[] rowValues = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // Ensure the row matches the number of columns
                        if (rowValues.Length == dataTable.Columns.Count)
                        {
                            dataTable.Rows.Add(rowValues);
                        }
                    }
                }
            }

            return dataTable;
        }

        private void guna2HtmlLabel1_Click(object sender, EventArgs e)
        {

        }

        private void ExportButton_Click_1(object sender, EventArgs e)
        {
            // Assuming the report is displayed in a RichTextBox
            string reportContent = richTextBoxReport.Text;

            if (!string.IsNullOrEmpty(reportContent))
            {
                // Convert the report content to a DataTable
                DataTable reportTable = ConvertReportToDataTable(reportContent);

                // Show SaveFileDialog to let the user choose the file location
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Excel Files|*.xlsx";
                    saveFileDialog.Title = "Save Inventory Valuation Report";
                    saveFileDialog.FileName = "InventoryValuationReport.xlsx"; // Default filename

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = saveFileDialog.FileName;
                        ExportToExcel(reportTable, filePath); // Call the export method
                    }
                }
            }
            else
            {
                MessageBox.Show("No report to export.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
