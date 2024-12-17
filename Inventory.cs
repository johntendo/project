using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using static POS.frmLogin;

namespace POS
{
    public partial class Inventory : UserControl
    {
        public Inventory()
        {
            InitializeComponent();
           //displayCategories();
            ClearFormFields();
            displayInventory();
            clearfield();
            displayallCate();
           // CheckLowInventory();
        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void txtprodn_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           // displayCategories();
        }

        private void guna2CircleButton2_Click(object sender, EventArgs e)
        {
            ClearFormFields();
        }
        public void displayCategories(int userId)
        {
            using (MySqlConnection connect = new MySqlConnection("server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();

                    // Query to select categories specific to the logged-in user
                    string selectdata = "SELECT category FROM category WHERE UserID = @userId";

                    using (MySqlCommand cmd = new MySqlCommand(selectdata, connect))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId); // Add user_id parameter

                        MySqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                // Assuming 'category' is a ComboBox
                                category.Items.Add(reader["category"].ToString());
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
        public void displayInventory()
        {
            Invent inventory = new Invent();
            List<Invent> userInventory = inventory.AllInventoryData(Session.UserID);

            // Example: Bind the inventory data to a DataGridView
            datainventory.DataSource = userInventory;
        }

        private void ClearFormFields()
        {
            prod_id.Clear();
            txtprodn.Clear();
            category.SelectedIndex = -1;
            qty.Value = 0;
            measure.SelectedIndex = -1; // Reset ComboBox
            Pprice.Clear();
            Sprice.Clear();
        }


        private void Btnadd_Click(object sender, EventArgs e)
        {
            int currentUserId = Session.UserID; // Get the current user ID from the session

            using (MySqlConnection connect = new MySqlConnection($"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();

                    // Step 1: Retrieve the active FinancialYearID
                    string fetchFinancialYearIdQuery = "SELECT FinancialYearID FROM financialyears WHERE IsActive = 1";
                    int financialYearId = 0;

                    using (MySqlCommand cmdFetchYear = new MySqlCommand(fetchFinancialYearIdQuery, connect))
                    {
                        object result = cmdFetchYear.ExecuteScalar();
                        if (result != null)
                        {
                            financialYearId = Convert.ToInt32(result);
                        }
                        else
                        {
                            MessageBox.Show("Error: No active financial year found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // Step 2: Check if the product already exists
                    string checkprod = "SELECT * FROM inventory WHERE prod_id = @prodid AND UserID = @userID"; // Check product for the current user
                    using (MySqlCommand cmdCheckProd = new MySqlCommand(checkprod, connect))
                    {
                        cmdCheckProd.Parameters.AddWithValue("@prodid", prod_id.Text.Trim());
                        cmdCheckProd.Parameters.AddWithValue("@userID", currentUserId); // Ensure the product is associated with the current user
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmdCheckProd);
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        if (table.Rows.Count > 0)
                        {
                            MessageBox.Show(prod_id.Text.Trim() + " is already taken for this user", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            // Step 3: Insert the new product into the inventory table with FinancialYearID and UserID
                            string query = @"INSERT INTO inventory (product_name, category, stock_quantity, unit_of_measure, purchase_price, selling_price, prod_id, FinancialYearID, UserID) 
                                     VALUES (@product_name, @category, @stock_quantity, @unit_of_measure, @purchase_price, @selling_price, @prod_id, @financialYearId, @userID)";

                            using (MySqlCommand cmdd = new MySqlCommand(query, connect))
                            {
                                cmdd.Parameters.AddWithValue("@product_name", txtprodn.Text.Trim());
                                cmdd.Parameters.AddWithValue("@category", category.SelectedItem.ToString());
                                cmdd.Parameters.AddWithValue("@stock_quantity", qty.Value);
                                cmdd.Parameters.AddWithValue("@unit_of_measure", measure.SelectedItem.ToString());
                                cmdd.Parameters.AddWithValue("@purchase_price", Pprice.Text.Trim());
                                cmdd.Parameters.AddWithValue("@selling_price", Sprice.Text.Trim());
                                cmdd.Parameters.AddWithValue("@prod_id", prod_id.Text.Trim());
                                cmdd.Parameters.AddWithValue("@financialYearId", financialYearId); // Associate with current financial year
                                cmdd.Parameters.AddWithValue("@userID", currentUserId); // Associate with current user

                                // Execute the insert command
                                cmdd.ExecuteNonQuery();

                                // Notify the user
                                MessageBox.Show("Product saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Clear the form (optional)
                                ClearFormFields();
                                displayInventory();
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

        private int Prodid = -1;

        private void datainventory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
           
        }

        private void btnupdate_Click(object sender, EventArgs e)
        {
            int currentUserId = Session.UserID; // Get the current user ID from the session

            if (txtprodn.Text == "" || category.Text == "" || qty.Value == 0 || measure.Text == "" || Pprice.Text == "" || Sprice.Text == "")
            {
                MessageBox.Show("All fields must be filled", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (MessageBox.Show("Are you sure you want to update " + txtprodn.Text.Trim() + "?", "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (MySqlConnection connect = new MySqlConnection($"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
                    {
                        try
                        {
                            connect.Open();
                            // Update query now checks for the UserID to ensure the product belongs to the current user
                            string updateQuery = @"UPDATE inventory 
                                           SET product_name = @prodName, 
                                               prod_id = @prodid,
                                               category = @category, 
                                               stock_quantity = @qty, 
                                               unit_of_measure = @measure, 
                                               purchase_price = @purchaseP, 
                                               selling_price = @saleP 
                                           WHERE product_id = @id 
                                           AND UserID = @userID";  // Ensure only the current user can update

                            using (MySqlCommand cmd = new MySqlCommand(updateQuery, connect))
                            {
                                cmd.Parameters.AddWithValue("@id", Prodid);  // Assuming Prodid is a valid product ID
                                cmd.Parameters.AddWithValue("@prodName", txtprodn.Text.Trim());
                                cmd.Parameters.AddWithValue("@prodid", prod_id.Text.Trim());
                                cmd.Parameters.AddWithValue("@category", category.SelectedItem.ToString());
                                cmd.Parameters.AddWithValue("@qty", qty.Value);
                                cmd.Parameters.AddWithValue("@measure", measure.SelectedItem.ToString());
                                cmd.Parameters.AddWithValue("@purchaseP", Pprice.Text.Trim());
                                cmd.Parameters.AddWithValue("@saleP", Sprice.Text.Trim());
                                cmd.Parameters.AddWithValue("@userID", currentUserId);  // Pass the current UserID

                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Inventory updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else
                                {
                                    MessageBox.Show("No inventory item found with the given ID or you do not have permission to update this item.",
                                        "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }

                                displayInventory();  // Refresh the inventory list
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
        }
        private void measure_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void CheckLowInventory()
        {
            using (MySqlConnection connect = new MySqlConnection($"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();
                    string query = "SELECT prod_id, product_name, stock_quantity, min_stock_level FROM inventory";
                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        MySqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            int stockQuantity = Convert.ToInt32(reader["stock_quantity"]);
                            int minStockLevel = Convert.ToInt32(reader["min_stock_level"]);

                            if (stockQuantity < minStockLevel)
                            {
                                string productName = reader["product_name"].ToString();
                                MessageBox.Show($"Alert: The stock for {productName} is below the minimum level!", "Low Stock Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error checking inventory levels: " + ex.Message);
                }
            }
        }


        private void btnremove_Click(object sender, EventArgs e)
        {
            int currentUserId = Session.UserID; // Get the current user ID from the session

            if (txtprodn.Text == "" || category.Text == "" || qty.Value == 0 || measure.Text == "" || Pprice.Text == "" || Sprice.Text == "")
            {
                MessageBox.Show("All fields must be filled", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (MessageBox.Show("Are you sure you want to delete " + txtprodn.Text.Trim() + "?", "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (MySqlConnection connect = new MySqlConnection($"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
                    {
                        try
                        {
                            connect.Open();

                            // Check if the product belongs to the current user
                            string checkOwnershipQuery = "SELECT UserID FROM inventory WHERE product_id = @id";
                            int productUserId = 0;

                            using (MySqlCommand cmdCheckOwner = new MySqlCommand(checkOwnershipQuery, connect))
                            {
                                cmdCheckOwner.Parameters.AddWithValue("@id", Prodid);
                                object result = cmdCheckOwner.ExecuteScalar();

                                if (result != null)
                                {
                                    productUserId = Convert.ToInt32(result);
                                }

                                // If the product is not owned by the current user
                                if (productUserId != currentUserId)
                                {
                                    MessageBox.Show("You do not have permission to delete this product.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                            }

                            // Proceed with deletion if the user is the owner
                            string deleteQuery = "DELETE FROM inventory WHERE product_id = @id";
                            using (MySqlCommand cmdDelete = new MySqlCommand(deleteQuery, connect))
                            {
                                cmdDelete.Parameters.AddWithValue("@id", Prodid);
                                cmdDelete.ExecuteNonQuery();

                                MessageBox.Show("Deleted Successfully", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                displayInventory();
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
        }

        private void datainventory_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = datainventory.Rows[e.RowIndex];

                // Fetch the product ID from the first cell (assuming it's stored in column 0)
                Prodid = Convert.ToInt32(row.Cells[0].Value);
                // Prodid = Convert.ToInt32(row.Cells[0].Value);
                string prodid = row.Cells[1].Value.ToString();
                string prodn = row.Cells[2].Value.ToString();
                string cate = row.Cells[3].Value.ToString();
                // Convert quantity to the appropriate type (decimal or int based on your design)
                int qtyValue = Convert.ToInt32(row.Cells[4].Value);
                string measuree = row.Cells[5].Value.ToString();
                decimal Ppricee = Convert.ToDecimal(row.Cells[6].Value);
                decimal Spricee = Convert.ToDecimal(row.Cells[7].Value);

                // Set text fields
                txtprodn.Text = prodn;
                prod_id.Text = prodid;
                category.Text = cate;
                qty.Value = qtyValue; // Set the numeric up/down value for quantity
                measure.Text = measuree;
                Pprice.Text = Ppricee.ToString("0.00"); // Format for display as a string
                Sprice.Text = Spricee.ToString("0.00"); // Format for display as a string

                // Optional: Display it for debugging
                MessageBox.Show("Selected Product ID: " + Prodid.ToString());
            }
        }
        public void displayallCate()
        {
            CateData cateData = new CateData();
            List<CateData> userCategories = cateData.AllCategoryData(Session.UserID);

            // Bind to DataGridView
            dataGridView1.DataSource = userCategories;

        }
        public void clearfield()
        {
            txt_username.Text = "";

        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            int currentUserId = Session.UserID;

            if (txt_username.Text == "")
            {
                MessageBox.Show("Empty Fields", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                using (MySqlConnection connect = new MySqlConnection("server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
                {
                    try
                    {
                        connect.Open();

                        // Check if the category exists for the current user
                        string checkCategory = "SELECT * FROM Category WHERE category = @cate AND UserID = @userID";
                        using (MySqlCommand cmd = new MySqlCommand(checkCategory, connect))
                        {
                            cmd.Parameters.AddWithValue("@cate", txt_username.Text.Trim());
                            cmd.Parameters.AddWithValue("@userID", currentUserId);

                            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                            DataTable table = new DataTable();
                            adapter.Fill(table);

                            if (table.Rows.Count > 0)
                            {
                                MessageBox.Show(txt_username.Text.Trim() + " is already taken", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                // Insert new category associated with the current user
                                string insertData = "INSERT INTO Category(category, date, UserID) VALUES (@catee, @date, @userID)";
                                using (MySqlCommand insertCmd = new MySqlCommand(insertData, connect))
                                {
                                    insertCmd.Parameters.AddWithValue("@catee", txt_username.Text.Trim());
                                    insertCmd.Parameters.AddWithValue("@date", DateTime.Now);
                                    insertCmd.Parameters.AddWithValue("@userID", currentUserId);

                                    insertCmd.ExecuteNonQuery();

                                    clearfield();
                                    displayallCate();

                                    MessageBox.Show("Added Successfully", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Connection Failed: " + ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        connect.Close();
                    }
                }
            }
        }
        private int Getid = 0;
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                Getid = (int)row.Cells[0].Value;
                string category = row.Cells[1].Value.ToString();



                txt_username.Text = category;



            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            int currentUserId = Session.UserID;

            if (txt_username.Text == "")
            {
                MessageBox.Show("Empty Fields", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (MessageBox.Show("Are you sure you want to update user ID " + Getid + "?", "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (MySqlConnection connect = new MySqlConnection("server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
                {
                    try
                    {
                        connect.Open();

                        // Update category for the current user
                        string updatedata = "UPDATE Category SET category = @cate WHERE id = @id AND UserID = @userID";
                        using (MySqlCommand updateCmd = new MySqlCommand(updatedata, connect))
                        {
                            updateCmd.Parameters.AddWithValue("@cate", txt_username.Text.Trim());
                            updateCmd.Parameters.AddWithValue("@id", Getid);
                            updateCmd.Parameters.AddWithValue("@userID", currentUserId);

                            int rowsAffected = updateCmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                clearfield();
                                displayallCate();
                                MessageBox.Show("Updated successfully", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Update failed. Category may not exist or does not belong to the current user.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Connection Failed: " + ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            int currentUserId = Session.UserID;

            if (txt_username.Text == "")
            {
                MessageBox.Show("Empty Fields", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (MessageBox.Show("Are you sure you want to delete user ID " + Getid + "?", "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (MySqlConnection connect = new MySqlConnection($"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
                {
                    try
                    {
                        connect.Open();

                        // Check if the category belongs to the current user before deleting
                        string checkCategory = "SELECT * FROM Category WHERE id = @id AND UserID = @userID";
                        using (MySqlCommand checkCmd = new MySqlCommand(checkCategory, connect))
                        {
                            checkCmd.Parameters.AddWithValue("@id", Getid);
                            checkCmd.Parameters.AddWithValue("@userID", currentUserId);

                            MySqlDataAdapter adapter = new MySqlDataAdapter(checkCmd);
                            DataTable table = new DataTable();
                            adapter.Fill(table);

                            if (table.Rows.Count > 0)
                            {
                                // Proceed to delete the category if it belongs to the current user
                                string deleteData = "DELETE FROM Category WHERE id = @id AND UserID = @userID";
                                using (MySqlCommand deleteCmd = new MySqlCommand(deleteData, connect))
                                {
                                    deleteCmd.Parameters.AddWithValue("@id", Getid);
                                    deleteCmd.Parameters.AddWithValue("@userID", currentUserId);
                                    deleteCmd.ExecuteNonQuery();

                                    clearfield();
                                    displayallCate();
                                    MessageBox.Show("Deleted successfully", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Category does not belong to the current user or does not exist.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Connection Failed: " + ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        connect.Close();
                    }
                }
            }
        }
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            clearfield();
        }

        private void category_Click(object sender, EventArgs e)
        {
            //displayCategories();
            int userId = Session.UserID; // Retrieve the logged-in user's ID from the session
            displayCategories(userId);

        }
        private void SearchCategory(string searchText)
        {
            using (MySqlConnection connect = new MySqlConnection("server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();

                    // Query to search categories based on name or date for the logged-in user
                    string categoryQuery = @"
                SELECT id, category, date
                FROM category
                WHERE (category LIKE @searchText OR DATE_FORMAT(date, '%Y-%m-%d') LIKE @searchText)
                  AND UserID= @userID"; // Include filtering for the logged-in user

                    // Load categories into dataGridView1
                    using (MySqlCommand cmdCategory = new MySqlCommand(categoryQuery, connect))
                    {
                        cmdCategory.Parameters.AddWithValue("@searchText", "%" + searchText + "%");
                        cmdCategory.Parameters.AddWithValue("@userID", Session.UserID); // Use the logged-in user's ID

                        using (MySqlDataAdapter categoryAdapter = new MySqlDataAdapter(cmdCategory))
                        {
                            DataTable categoryResults = new DataTable();
                            categoryAdapter.Fill(categoryResults);
                            dataGridView1.DataSource = categoryResults;
                        }
                    }

                    // Query to get products from inventory where category matches searchText and belongs to the user
                    string productsQuery = @"
                SELECT product_id, product_name, stock_quantity, unit_of_measure, purchase_price, selling_price
                FROM inventory
                WHERE category LIKE @searchText
                  AND UserID = @userID"; // Include filtering for the logged-in user

                    // Load products into datainventory
                    using (MySqlCommand cmdProducts = new MySqlCommand(productsQuery, connect))
                    {
                        cmdProducts.Parameters.AddWithValue("@searchText", "%" + searchText + "%");
                        cmdProducts.Parameters.AddWithValue("@userID", Session.UserID); // Use the logged-in user's ID

                        using (MySqlDataAdapter productsAdapter = new MySqlDataAdapter(cmdProducts))
                        {
                            DataTable productResults = new DataTable();
                            productsAdapter.Fill(productResults);
                            datainventory.DataSource = productResults;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error during search: " + ex.Message);
                }
            }
        }
        private void btnSearchCategory_Click(object sender, EventArgs e)
        {
            
        }

        private void txtSearchCategory_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {
            SearchCategory(txtSearchCategory.Text.Trim());
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnSearchCategory_Click_1(object sender, EventArgs e)
        {
            SearchCategory(txtSearchCategory.Text.Trim());
        }
    }
        }
                    
        
    

