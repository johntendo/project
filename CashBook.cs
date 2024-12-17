using ClosedXML.Excel;
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
    public partial class CashBook : UserControl
    {
        public CashBook()
        {
            InitializeComponent();
            CalculateTotals();
            DisplayCash();
            DisplayCredit();
        }
        private MySqlConnection GetDatabaseConnection()
        {
            return new MySqlConnection("server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV");
        }


        private void guna2HtmlLabel6_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel10_Click(object sender, EventArgs e)
        {

        }
        public void DisplayCash()
        {
            int userId = Session.UserID;

            // Fetch cash data for the current user
            List<cashbookk> cashData = new cashbookk().AllCashData(userId);
            dgvCashbook.DataSource = cashData;
        }
        public void DisplayCredit()
        {
            int userId = Session.UserID;
            //cashbookk odata = new cashbookk();
            List<cashbookk> creditData = new cashbookk().AllCreditData(userId);
            dgvBankTransactions.DataSource = creditData;
        }
        private void LoadCashTransactions(DateTime startDate, DateTime endDate, int userId)
        {
            using (var connection = GetDatabaseConnection())
            {
                connection.Open();
                // Modify the query to filter by UserID
                string query = @"SELECT id, transaction_date, transaction_type, description, amount 
                         FROM cashbook 
                         WHERE transaction_date >= @startDate 
                         AND transaction_date <= @endDate 
                         AND transaction_type = 'Cash' 
                         AND UserID = @UserID"; // Add UserID filter

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@startDate", startDate);
                cmd.Parameters.AddWithValue("@endDate", endDate);
                cmd.Parameters.AddWithValue("@UserID", userId);  // Pass the UserID

                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable cashTable = new DataTable();
                adapter.Fill(cashTable);

                dgvCashbook.DataSource = cashTable;
            }
        }

        private void LoadBankTransactions(DateTime startDate, DateTime endDate, int userId)
        {
            using (var connection = GetDatabaseConnection())
            {
                connection.Open();
                // Modify the query to filter by UserID
                string query = @"SELECT id, transaction_date, transaction_type, description, amount 
                         FROM cashbook 
                         WHERE transaction_date >= @startDate 
                         AND transaction_date <= @endDate 
                         AND transaction_type = 'Credit' 
                         AND UserID = @UserID"; // Add UserID filter

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@startDate", startDate);
                cmd.Parameters.AddWithValue("@endDate", endDate);
                cmd.Parameters.AddWithValue("@UserID", userId);  // Pass the UserID

                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable bankTable = new DataTable();
                adapter.Fill(bankTable);

                dgvBankTransactions.DataSource = bankTable;
            }
        }
        private void CalculateTotals()
        {
            decimal totalCash = 0, totalCredit = 0;
            decimal totalBankCash = 0, totalBankCredit = 0;

            foreach (DataGridViewRow row in dgvCashbook.Rows)
            {
                if (row.Cells["transaction_type"].Value?.ToString() == "Cash")
                {
                    totalCash += Convert.ToDecimal(row.Cells["amount"].Value ?? 0);
                }
                else if (row.Cells["transaction_type"].Value?.ToString() == "Credit")
                {
                    totalCredit += Convert.ToDecimal(row.Cells["amount"].Value ?? 0);
                }
            }

            foreach (DataGridViewRow row in dgvBankTransactions.Rows)
            {
                if (row.Cells["transaction_type"].Value?.ToString() == "Cash")
                {
                    totalBankCash += Convert.ToDecimal(row.Cells["amount"].Value ?? 0);
                }
                else if (row.Cells["transaction_type"].Value?.ToString() == "Credit")
                {
                    totalBankCredit += Convert.ToDecimal(row.Cells["amount"].Value ?? 0);
                }
            }
            lblTotalDebit.Text = $"Total Cash: UGX {(totalCash - totalCredit):N}";
            txtTotalBank.Text = $"Total Credit: UGX {(totalBankCash - totalBankCredit):N}";
            txtBalance.Text = $"Balance: UGX {(totalCash + totalBankCash) - (totalCredit + totalBankCredit):N}";

        }

        private void btnReset_Click(object sender, EventArgs e)
        {

        }




        private void btnFilter_Click(object sender, EventArgs e)
        {
            DateTime startDate = dtpStartDate.Value;
            DateTime endDate = dtpEndDate.Value;
            int userId = Session.UserID;

            // Load cash and bank transactions within the date range
            LoadCashTransactions(startDate, endDate, userId);
            LoadBankTransactions(startDate, endDate, userId);

            // Calculate totals for cash, bank, and balance after filtering
            CalculateTotals();
        }

        private void btnReset_Click_1(object sender, EventArgs e)
        {
            int userId = Session.UserID;
            LoadCashTransactions(DateTime.MinValue, DateTime.MaxValue, userId);
            LoadBankTransactions(DateTime.MinValue, DateTime.MaxValue, userId);
            CalculateTotals();
        }
        private void AddTransaction(string transactionType, decimal amount, string description)
        {

        }

        private void dgvCashbook_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void guna2HtmlLabel12_Click(object sender, EventArgs e)
        {

        }

        private void btnSettleCredit_Click(object sender, EventArgs e)
        {
            if (dgvBankTransactions.SelectedRows.Count > 0)
            {
                // Get selected row
                DataGridViewRow selectedRow = dgvBankTransactions.SelectedRows[0];
                int transactionId = Convert.ToInt32(selectedRow.Cells["id"].Value);

                using (MySqlConnection connection = GetDatabaseConnection())
                {
                    try
                    {
                        connection.Open();

                        // Update the transaction_type to 'Cash' for the selected transaction
                        string updateQuery = @"UPDATE cashbook 
                                       SET transaction_type = 'Cash' 
                                       WHERE id = @transactionId";
                        using (MySqlCommand cmd = new MySqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@transactionId", transactionId);
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Credit transaction settled successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh both DataGrids
                        DisplayCash();
                        DisplayCredit();
                        CalculateTotals();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error settling credit transaction: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a credit transaction to settle.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgvBankTransactions_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Ensure a valid row index
            {
                dgvBankTransactions.Rows[e.RowIndex].Selected = true;
            }
        }




        private void ExportToExcel(DataGridView dataGridView, string fileName)
        {
            try
            {
                using (XLWorkbook workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Sheet1");

                    // Add headers
                    for (int col = 0; col < dataGridView.Columns.Count; col++)
                    {
                        worksheet.Cell(1, col + 1).Value = dataGridView.Columns[col].HeaderText;
                    }

                    // Add rows
                    for (int row = 0; row < dataGridView.Rows.Count; row++)
                    {
                        for (int col = 0; col < dataGridView.Columns.Count; col++)
                        {
                            worksheet.Cell(row + 2, col + 1).Value = dataGridView.Rows[row].Cells[col].Value?.ToString();
                        }
                    }

                    // Save to file
                    using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                    {
                        saveFileDialog.Filter = "Excel Files|*.xlsx";
                        saveFileDialog.FileName = fileName;

                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            workbook.SaveAs(saveFileDialog.FileName);
                            MessageBox.Show("Data exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting to Excel: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void guna2Panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnExportCash_Click(object sender, EventArgs e)
        {
            ExportToExcel(dgvBankTransactions, "CreditTransactions");
        }

        private void btnExportCredit_Click(object sender, EventArgs e)
        {
            ExportToExcel(dgvCashbook, "CashTransactions");
        }
    }
}
