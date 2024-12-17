using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace POS
{
    public partial class frmLogin : Form
    {
        string connectionstring = "server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV";
        MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV");


        public bool checkconnection()
        {
            if (connect.State == ConnectionState.Closed)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public frmLogin()
        {
            InitializeComponent();
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            // Initialization code if needed
        }

        // Static class to store session information
        public static class Session
        {
            public static int UserID { get; set; }
            public static string Username { get; set; }

            // You can add other session-related data here as needed
            public static void ClearSession()
            {
                UserID = 0;
                Username = string.Empty;
            }
        }


        private void btnlogin_Click(object sender, EventArgs e)
        {
            // Check if username or password fields are empty
            if (string.IsNullOrWhiteSpace(txtname.Text) || string.IsNullOrWhiteSpace(txtpass.Text))
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Define the connection string
                using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
                {
                    connect.Open();

                    // Query to retrieve UserID for the given username and password
                    string query = "SELECT id FROM user WHERE username=@username AND password=@password";
                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        // Use parameters to prevent SQL injection
                        cmd.Parameters.AddWithValue("@username", txtname.Text.Trim());
                        cmd.Parameters.AddWithValue("@password", txtpass.Text.Trim()); // Ideally, use a hashed password here

                        // Execute the query
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            // Store the UserID in the session (or equivalent variable)
                            int userId = Convert.ToInt32(result);
                            Session.UserID = userId;
                            Session.Username = txtname.Text.Trim();

                            // Display success message and clear fields
                            MessageBox.Show("Login successful!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            clear();

                            // Open the main page after successful login
                            Year mainPage = new Year();
                            mainPage.Show();
                            this.Hide();
                        }
                        else
                        {
                            MessageBox.Show("Incorrect username or password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void clear()
        {
            txtname.Text = "";
            txtpass.Text = "";
        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Signup signup = new Signup();
            signup.Show();
        }

        private void guna2CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBox1.Checked)
            {
                txtpass.PasswordChar = '\0';
            }
            else
            {
                txtpass.PasswordChar = '*';
            }
        }

        private void txtpass_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
