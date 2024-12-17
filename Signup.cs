using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace POS
{

    public partial class Signup : Form
    {
        //string connectionstring = "server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV";
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

        public Signup()
        {
            InitializeComponent();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            frmLogin frm = new frmLogin();
            frm.Show();
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


        private void btnsubmit_Click(object sender, EventArgs e)
        {

           
        }
        public void clear()
        {
            username.Text = "";
            passname.Text = "";
            Cpass.Text = "";
            Oname.Text = "";
            checkBox1.Text = null;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
           
        }

        private void guna2CirclePictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void uname_TextChanged(object sender, EventArgs e)
        {

        }

        private void Oname_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            if (username.Text == "" || passname.Text == "" || Cpass.Text == "" || Oname.Text == "")
            {
                MessageBox.Show("Fill Every Filled please", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (passname.Text != Cpass.Text)
            {
                MessageBox.Show("passwords do not match", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
                {
                    try

                    {
                        connect.Open();
                        string checkuser = "select * from user where username=@user";
                        using (MySqlCommand cmd = new MySqlCommand(checkuser, connect))
                        {
                            cmd.Parameters.AddWithValue("@user", username.Text.Trim());
                            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                            DataTable table = new DataTable();
                            adapter.Fill(table);
                            if (table.Rows.Count > 0)
                            {
                                MessageBox.Show(username.Text + " is already taken", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else if (Cpass.Text.Trim().Length < 8)
                            {
                                MessageBox.Show("Password should have more than 8 characters", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else

                            {


                                string sql = "insert into user(username,password,date,name) values (@user,@pass,@date,@name)";
                                using (MySqlCommand insert = new MySqlCommand(sql, connect))
                                {
                                    insert.Parameters.AddWithValue("@user", username.Text.Trim());
                                    insert.Parameters.AddWithValue("@pass", Cpass.Text.Trim());
                                    insert.Parameters.AddWithValue("@name", Oname.Text.Trim());

                                    //insert.Parameters.AddWithValue("@role", ComboBox1.SelectedItem.ToString());
                                    //insert.Parameters.AddWithValue("@status", ComboBox2.SelectedItem.ToString());

                                    DateTime time = DateTime.Now;
                                    insert.Parameters.AddWithValue("@date", time);
                                    insert.ExecuteNonQuery();
                                    MessageBox.Show("Saved successfully", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    clear();
                                    frmLogin frmLogin = new frmLogin();
                                    frmLogin.Show();
                                    this.Hide();
                                }

                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        connect.Close();
                    }
                }
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                passname.PasswordChar = '\0';
                Cpass.PasswordChar = '\0';
            }
            else
            {
                passname.PasswordChar = '*';
                Cpass.PasswordChar = '*';
            }
        }
    }
}
