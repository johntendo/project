using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using static POS.frmLogin;

namespace POS
{
    public partial class Categoriesadd : UserControl
    {
       // string connectionstring = "server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV";
        MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV");

        public Categoriesadd()
        {
            InitializeComponent();
            displayallCate();
        }
        public void clearfield()
        {
            txt_username.Text = "";

        }
        public void displayallCate()
        {
            CateData cateData = new CateData();
            List<CateData> userCategories = cateData.AllCategoryData(Session.UserID);

            // Bind to DataGridView
            dataGridView1.DataSource = userCategories;

        }


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


        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (txt_username.Text == "")
            {
                MessageBox.Show("Empty Fields", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
                if (checkconnection())
            {
                try
                {
                    connect.Open();
                    string checkCategory = "select * from Category where category =@cate";
                    using (MySqlCommand cmd = new MySqlCommand(checkCategory, connect))
                    {
                        cmd.Parameters.AddWithValue("@cate", txt_username.Text.Trim());
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        if (table.Rows.Count > 0)
                        {
                            MessageBox.Show(txt_username.Text.Trim() + " Is already taken", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        }
                        else
                        {
                            string insertdata = "insert into Category(category,date) values (@catee,@date)";
                            using (MySqlCommand insertD = new MySqlCommand(insertdata, connect))
                            {
                                insertD.Parameters.AddWithValue("@catee", txt_username.Text.Trim());
                                DateTime today = DateTime.Now;
                                insertD.Parameters.AddWithValue("@date", today);
                                insertD.ExecuteNonQuery();
                                clearfield();
                                displayallCate();
                                MessageBox.Show("Added Successfully", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            }

                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Connection Failed:" + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connect.Close();
                }
            }
        }
        private int Getid = 0;

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                Getid = (int)row.Cells[0].Value;
                string category = row.Cells[1].Value.ToString();



                txt_username.Text = category;



            }

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (txt_username.Text == "")
            {
                MessageBox.Show("Empty Fields", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
             if (MessageBox.Show("Are Sure you want to update user ID" + Getid + "?", "Corfirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {


                if (checkconnection())
                {
                    try
                    {
                        connect.Open();
                        string updatedata = "update Category set category=@cate where id =@id";
                        using (MySqlCommand updateD = new MySqlCommand(updatedata, connect))
                        {
                            updateD.Parameters.AddWithValue("@cate", txt_username.Text.Trim());
                            updateD.Parameters.AddWithValue("@id", Getid);
                            updateD.ExecuteNonQuery();
                            clearfield();
                            displayallCate();
                            MessageBox.Show("Updated successfully", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        }


                    }


                    catch (Exception ex)
                    {
                        MessageBox.Show("Connection Failed:" + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        connect.Close();
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (txt_username.Text == "")
            {
                MessageBox.Show("Empty Fields", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            if (MessageBox.Show("Are Sure you want to delete user ID" + Getid + "?", "Corfirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {


                if (checkconnection())
                {
                    try
                    {
                        connect.Open();
                        string updatedata = "delete from Category where id =@id";
                        using (MySqlCommand updateD = new MySqlCommand(updatedata, connect))
                        {
                            updateD.Parameters.AddWithValue("@cate", txt_username.Text.Trim());
                            updateD.Parameters.AddWithValue("@id", Getid);
                            updateD.ExecuteNonQuery();
                            clearfield();
                            displayallCate();
                            MessageBox.Show("Deletion successfully", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        }


                    }


                    catch (Exception ex)
                    {
                        MessageBox.Show("Connection Failed:" + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        connect.Close();
                    }
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            clearfield();
        }
    }



}


