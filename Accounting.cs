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
    public partial class Accounting : Form
    {
        Inventory inventory;
        Sales sales;
        P_LReport report;
        Expenses ex;
        Dashboard dashboard;
        CashBook cashBook;
        FinancialReportForm financialReport;
        public Accounting()
        {
            InitializeComponent();
            this.inventory = new Inventory();
            this.sales = new Sales();
            this.report = new P_LReport();
            this.dashboard = new Dashboard();
            this.ex = new Expenses();
            this.cashBook = new CashBook();
            this.financialReport = new FinancialReportForm();
            this.content.Controls.Add(this.inventory);
            this.content.Controls.Add(this.sales);
            this.content.Controls.Add(this.report);
            this.content.Controls.Add(this.ex);
            this.content.Controls.Add(this.dashboard);
            this.content.Controls.Add(this.cashBook);
            this.content.Controls.Add(this.financialReport);
        }

        private void AccDash_Click(object sender, EventArgs e)
        {
            dashboard.Visible = true;
            foreach (Control control in content.Controls)
            {
                if (control != dashboard)
                {
                    control.Visible = false;
                }
            }
        }

        private void sales2_Load(object sender, EventArgs e)
        {

        }

        private void p_LReport1_Load(object sender, EventArgs e)
        {

        }

        private void accinv_Click(object sender, EventArgs e)
        {
            inventory.Visible = true;
            foreach (Control control in content.Controls)
            {
                if (control != inventory)
                {
                    control.Visible = false;
                }
            }
        }

        private void AccSales_Click(object sender, EventArgs e)
        {
            sales.Visible = true;
            foreach (Control control in content.Controls)
            {
                if (control != sales)
                {
                    control.Visible = false;
                }
            }
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            report.Visible = true;
            foreach (Control control in content.Controls)
            {
                if (control != report)
                {
                    control.Visible = false;
                }
            }
        }

        private void expenses1_Load(object sender, EventArgs e)
        {

        }

        private void guna2VScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
           // content.AutoScrollPosition = new Point(0, guna2VScrollBar1.Value);
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            ex.Visible = true;
            foreach (Control control in content.Controls)
            {
                if (control != ex)
                {
                    control.Visible = false;
                }
            }
        }

        private void dashboard2_Load(object sender, EventArgs e)
        {

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to singout ?", "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                frmLogin login = new frmLogin();
                login.Show();
                this.Hide();
            }
        }

        private void btnCashbook_Click(object sender, EventArgs e)
        {
            cashBook.Visible = true;
            foreach (Control control in content.Controls)
            {
                if (control != cashBook)
                {
                    control.Visible = false;
                }
            }
        }

        private void dashboard2_Load_1(object sender, EventArgs e)
        {

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

                            // Display report in a larger form for better readability
                           // ShowReportInTextBox(report.ToString());
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

        // Helper method to display report in a larger form
        
       

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
           // GenerateInventoryValuationReport();
        }
        private void GenerateCashFlowStatement()
        {
            using (MySqlConnection connect = new MySqlConnection("server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();

                    // Get total cash inflows from the cashbook
                    decimal totalCashInflows = 0;
                    string cashInflowsQuery = @"
                SELECT SUM(amount) AS total_amount
                FROM cashbook
                WHERE transaction_type = 'Cash'"; // Assuming cash transactions are inflows

                    using (MySqlCommand cmdCashInflows = new MySqlCommand(cashInflowsQuery, connect))
                    {
                        var result = cmdCashInflows.ExecuteScalar();
                        totalCashInflows = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                    }

                    // Get total cash outflows from the expenses table
                    decimal totalCashOutflows = 0;
                    string cashOutflowsQuery = @"
                SELECT SUM(Eamount) AS total_expenses
                FROM expenses"; // Assuming Eamount is the amount in the expenses table

                    using (MySqlCommand cmdCashOutflows = new MySqlCommand(cashOutflowsQuery, connect))
                    {
                        var result = cmdCashOutflows.ExecuteScalar();
                        totalCashOutflows = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                    }

                    // Calculate net cash flow
                    decimal netCashFlow = totalCashInflows - totalCashOutflows;

                    // Create report string
                    var cashFlowReport = new StringBuilder();
                    cashFlowReport.AppendLine("Cash Flow Statement");
                    cashFlowReport.AppendLine("--------------------------------------------------");
                    cashFlowReport.AppendLine($"Total Cash Inflows: {totalCashInflows:N0} UGX");
                    cashFlowReport.AppendLine($"Total Cash Outflows (Expenses): {totalCashOutflows:N0} UGX");
                    cashFlowReport.AppendLine($"Net Cash Flow: {netCashFlow:N0} UGX");

                    // Display the cash flow statement
                    MessageBox.Show(cashFlowReport.ToString(), "Cash Flow Statement", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error generating cash flow statement: " + ex.Message);
                }
                finally
                {
                    connect.Close();
                }
            }
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            
            
                GenerateCashFlowStatement();
            

        }
        public void Logout()
        {
            // Clear the session data (e.g., UserID, Role, etc.)
            Session.UserID = 0;
           // Session.UserRole = string.Empty;

            // Optionally, clear any UI components or user-specific data
           // ClearUserInterface();

            // Redirect to login form or login screen
            frmLogin loginForm = new frmLogin();
            loginForm.Show();
            this.Hide();
        }


        private void guna2Button2_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            InventoryEvaluation inventoryEvaluation = new InventoryEvaluation();
            inventoryEvaluation.Show();
        }

        private void financialyear_Click(object sender, EventArgs e)
        {
            financialReport.Visible = true;
            foreach (Control control in content.Controls)
            {
                if (control != financialReport)
                {
                    control.Visible = false;
                }
            }

        }
    }
}
