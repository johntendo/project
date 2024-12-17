using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using static POS.frmLogin;

namespace POS
{
    public partial class Expenses : UserControl
    {
        public Expenses()
        {
            InitializeComponent();
            ClearFields();
            DisplayExpenses();
            LoadExpensesHistory();
        }

        private void guna2HtmlLabel2_Click(object sender, EventArgs e)
        {
            // Placeholder for label click event
        }
        public void display()
        {
            addE expenseManager = new addE();
            int userId = Session.UserID; // Replace with the actual user ID retrieval logic
            List<addE> userExpenses = expenseManager.addExpense(userId);
            ex.DataSource = userExpenses;

        }

        private void DisplayExpenses()
        {
            try
            {
                using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
                {
                    connect.Open();
                    int userId = Session.UserID; // Replace with actual UserID from session

                    string query = @"
                        SELECT Ename, Eamount, date 
                        FROM expenses 
                        WHERE UserID = @UserID AND FinancialYearID = @FinancialYearID";

                    int activeYearID = GetActiveFinancialYearID();
                    if (activeYearID == 0)
                    {
                        MessageBox.Show("No active financial year found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    MySqlCommand cmd = new MySqlCommand(query, connect);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@FinancialYearID", activeYearID);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    ex.DataSource = dataTable; // `ex` assumed to be a DataGridView control
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error displaying expenses: " + ex.Message);
            }
        }

        private void ClearFields()
        {
            Eamount.Text = "";
            Ename.Text = "";
        }

        private int GetActiveFinancialYearID()
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
           
        }

        private void LoadExpensesHistory()
        {
            using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();

                    string query = "SELECT * FROM expenses_history WHERE UserID = @UserID";
                    MySqlCommand cmd = new MySqlCommand(query, connect);
                    cmd.Parameters.AddWithValue("@UserID", Session.UserID); // Replace with actual UserID retrieval

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    dataGridView1.DataSource = dataTable; // `dataGridView1` assumed to be a DataGridView control
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error fetching expense history: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
    

private void ex_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void guna2VScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            // This assumes you are scrolling the panel vertically
            //panelContent.AutoScrollPosition = new Point(0, guna2VScrollBar1.Value);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void panelContent_Paint(object sender, PaintEventArgs e)
        {

        }


        private void btnadd_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Ename.Text) || string.IsNullOrWhiteSpace(Eamount.Text))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(Eamount.Text.Trim(), out decimal expenseAmount))
            {
                MessageBox.Show("Invalid amount. Please enter a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();

                    int activeYearID = GetActiveFinancialYearID();
                    if (activeYearID == 0)
                    {
                        MessageBox.Show("No active financial year set. Please select an active financial year.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string query = @"
                        INSERT INTO expenses (Ename, Eamount, date, FinancialYearID, UserID) 
                        VALUES (@name, @amount, NOW(), @FinancialYearID, @UserID)";

                    MySqlCommand cmd = new MySqlCommand(query, connect);
                    cmd.Parameters.AddWithValue("@name", Ename.Text.Trim());
                    cmd.Parameters.AddWithValue("@amount", expenseAmount);
                    cmd.Parameters.AddWithValue("@FinancialYearID", activeYearID);
                    cmd.Parameters.AddWithValue("@UserID", Session.UserID); // Replace with actual UserID retrieval

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Expense saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ClearFields();
                    DisplayExpenses();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error adding expense: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
