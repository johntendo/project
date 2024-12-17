using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;
using static POS.frmLogin;

namespace POS
{
    public partial class Year : Form
    {
        private const string ConnectionString = @"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV";

        public Year()
        {
            InitializeComponent();
            LoadFinancialYears(Session.UserID);
        }

        private void LoadFinancialYears(int userId)
        {
            try
            {
                using (MySqlConnection connect = new MySqlConnection(ConnectionString))
                {
                    connect.Open();
                    string query = "SELECT FinancialYearID, YearName FROM FinancialYears WHERE UserID = @UserID";
                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            cmbFinancialYea.DataSource = dt;
                            cmbFinancialYea.DisplayMember = "YearName";
                            cmbFinancialYea.ValueMember = "FinancialYearID";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading financial years: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSelectYear_Click(object sender, EventArgs e)
        {
            if (cmbFinancialYea.SelectedValue == null)
            {
                MessageBox.Show("Please select a financial year.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int selectedYearID = Convert.ToInt32(cmbFinancialYea.SelectedValue);
            SetActiveFinancialYear(selectedYearID, Session.UserID);
        }

        private void SetActiveFinancialYear(int yearID, int userId)
        {
            try
            {
                using (MySqlConnection connect = new MySqlConnection(ConnectionString))
                {
                    connect.Open();

                    // Deactivate current year
                    string deactivateQuery = "UPDATE FinancialYears SET IsActive = FALSE WHERE IsActive = TRUE AND UserID = @UserID";
                    using (MySqlCommand cmdDeactivate = new MySqlCommand(deactivateQuery, connect))
                    {
                        cmdDeactivate.Parameters.AddWithValue("@UserID", userId);
                        cmdDeactivate.ExecuteNonQuery();
                    }

                    // Activate selected year
                    string activateQuery = "UPDATE FinancialYears SET IsActive = TRUE WHERE FinancialYearID = @YearID AND UserID = @UserID";
                    using (MySqlCommand cmdActivate = new MySqlCommand(activateQuery, connect))
                    {
                        cmdActivate.Parameters.AddWithValue("@YearID", yearID);
                        cmdActivate.Parameters.AddWithValue("@UserID", userId);
                        cmdActivate.ExecuteNonQuery();
                    }

                    MessageBox.Show("Financial Year set as active.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Navigate to Accounting screen
                    new Accounting().Show();
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting active financial year: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCreateYear_Click(object sender, EventArgs e)
        {
            string yearName = $"{dtpStartDate.Value.Year}-{dtpEndDate.Value.Year}";
            DateTime startDate = dtpStartDate.Value.Date;
            DateTime endDate = dtpEndDate.Value.Date;

            if (startDate >= endDate)
            {
                MessageBox.Show("Start Date must be before End Date.", "Invalid Dates", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection connect = new MySqlConnection(ConnectionString))
                {
                    connect.Open();
                    string query = @"
                        INSERT INTO FinancialYears (YearName, StartDate, EndDate, IsActive, UserID) 
                        VALUES (@YearName, @StartDate, @EndDate, @IsActive, @UserID)";

                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@YearName", yearName);
                        cmd.Parameters.AddWithValue("@StartDate", startDate);
                        cmd.Parameters.AddWithValue("@EndDate", endDate);
                        cmd.Parameters.AddWithValue("@IsActive", false);
                        cmd.Parameters.AddWithValue("@UserID", Session.UserID);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Financial Year created successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Reload financial years
                        LoadFinancialYears(Session.UserID);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating financial year: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public int GetActiveFinancialYearID(int userId)
        {
            try
            {
                using (MySqlConnection connect = new MySqlConnection(ConnectionString))
                {
                    connect.Open();
                    string query = "SELECT FinancialYearID FROM FinancialYears WHERE IsActive = TRUE AND UserID = @UserID LIMIT 1";

                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        object result = cmd.ExecuteScalar();
                        return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving active financial year: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }

        private void dtpEndDate_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
