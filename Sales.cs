using ClosedXML.Excel;
using MySql.Data.MySqlClient;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using static POS.frmLogin;

namespace POS
{
    public partial class Sales : UserControl
    {
        public Sales()
        {
            InitializeComponent();
            LoadProductIDs();
            Displayallsales();
            ClearFields();
            ProdId();
            // ProcessSale(int selectedProductID);


        }

        private void btnadd_Click(object sender, EventArgs e)
        {
            SaveSale(Session.UserID);

            Displayallsales();
            ClearFields();
            LoadProductIDs();

        }

        private void LoadProductIDs()
        {
            // Retrieve the current user's ID (adjust this according to your session management)
            int userId = Session.UserID; // Replace this with actual method to get the logged-in user's ID
            if (userId <= 0)
            {
                MessageBox.Show("User session has expired. Please log in again.", "Session Expired", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Define the connection string (ideally, store it in a configuration file)
            string connectionString = "server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV";

            // Query to fetch product IDs for the specific user
            string query = "SELECT prod_id FROM inventory WHERE UserID = @user_id";

            using (MySqlConnection connect = new MySqlConnection(connectionString))
            {
                try
                {
                    connect.Open();

                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        // Add parameter for user ID
                        cmd.Parameters.AddWithValue("@user_id", userId);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Clear existing items to avoid duplicates
                            prodid.Items.Clear();

                            // Populate the ComboBox with filtered product IDs
                            while (reader.Read())
                            {
                                prodid.Items.Add(reader["prod_id"].ToString());
                            }
                        }
                    }
                }
                catch (MySqlException sqlEx)
                {
                    MessageBox.Show("Database error: " + sqlEx.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Example of a method to retrieve the current user ID

        public int GetActiveFinancialYearID(int userId)
        {
            int activeYearID = Session.UserID;


            // Query to retrieve the active financial year for a specific user
            string query = @"
        SELECT FinancialYearID 
        FROM FinancialYears 
        WHERE IsActive = TRUE AND UserID = @userId 
        LIMIT 1";

            // Define the connection string (use a configuration file in production)
            string connectionString = @"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV";

            using (MySqlConnection connect = new MySqlConnection(connectionString))
            {
                try
                {
                    connect.Open();

                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        // Add the UserID parameter to filter financial years
                        cmd.Parameters.AddWithValue("@userId", userId);

                        // Execute the query and retrieve the result
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            activeYearID = Convert.ToInt32(result);
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


        private void SaveSale(int userId)
        {
            using (MySqlConnection connect = new MySqlConnection($"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();
                    MySqlTransaction transaction = connect.BeginTransaction();

                    // Retrieve the active financial year ID
                    int activeYearID = GetActiveFinancialYearID(userId);
                    if (activeYearID == 0)
                    {
                        MessageBox.Show("No active financial year set. Please select an active financial year.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Step 1: Fetch the cost price of the product from the inventory table
                    string fetchCostPriceQuery = @"SELECT purchase_price FROM inventory WHERE prod_id = @product_id AND UserID = @user_id";
                    decimal costPrice = 0;

                    using (MySqlCommand cmdFetchCost = new MySqlCommand(fetchCostPriceQuery, connect, transaction))
                    {
                        cmdFetchCost.Parameters.AddWithValue("@product_id", prodid.SelectedItem.ToString());
                        cmdFetchCost.Parameters.AddWithValue("@user_id", userId); // Added user_id parameter
                        object result = cmdFetchCost.ExecuteScalar();
                        if (result != null)
                        {
                            costPrice = Convert.ToDecimal(result);
                        }
                        else
                        {
                            MessageBox.Show("Error: Product not found in inventory for this user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // Step 2: Determine the sale price
                    decimal salePrice;
                    if (chkAboveBelowSalePrice.Checked)
                    {
                        salePrice = decimal.TryParse(txtCSP.Text.Trim(), out var customPrice) ? customPrice : 0;
                        txtprice.Enabled = false;
                    }
                    else
                    {
                        salePrice = decimal.TryParse(txtprice.Text.Trim(), out var originalPrice) ? originalPrice : 0;
                        txtCSP.Enabled = false;
                    }

                    // Step 3: Calculate cost of sales (COGS)
                    decimal quantitySold = qty.Value;
                    decimal costOfSales = costPrice * quantitySold;

                    // Step 4: Insert sale data into the sales table
                    string insertSaleQuery = @"INSERT INTO sales (UserID, product_id, product_name, quantity_sold, sale_price, sale_date, cost_of_sales, sale_type, discount, due_date, FinancialYearID) 
                                       VALUES (@user_id, @product_id, @name, @quantity_sold, @sale_price, NOW(), @cost_of_sales, @sale_type, @discount, @due_date, @FinancialYearID)";
                    using (MySqlCommand cmdInsertSale = new MySqlCommand(insertSaleQuery, connect, transaction))
                    {
                        cmdInsertSale.Parameters.AddWithValue("@user_id", userId); // Added user_id
                        cmdInsertSale.Parameters.AddWithValue("@product_id", prodid.SelectedItem.ToString());
                        cmdInsertSale.Parameters.AddWithValue("@name", prodname.Text.Trim());
                        cmdInsertSale.Parameters.AddWithValue("@quantity_sold", quantitySold);
                        cmdInsertSale.Parameters.AddWithValue("@sale_price", salePrice);
                        cmdInsertSale.Parameters.AddWithValue("@cost_of_sales", costOfSales);
                        cmdInsertSale.Parameters.AddWithValue("@sale_type", btnCash.Checked ? "Cash" : "Credit");
                        cmdInsertSale.Parameters.AddWithValue("@discount", decimal.TryParse(txtdiscount.Text.Trim(), out var discount) ? discount : 0);
                        cmdInsertSale.Parameters.AddWithValue("@due_date", btnCredit.Checked ? (object)duedate.Value : DBNull.Value);
                        cmdInsertSale.Parameters.AddWithValue("@FinancialYearID", activeYearID);

                        cmdInsertSale.ExecuteNonQuery();
                    }

                    // Step 5: Update the inventory (reduce stock quantity)
                    string updateInventoryQuery = @"UPDATE inventory SET stock_quantity = stock_quantity - @quantity_sold WHERE prod_id = @product_id AND UserID = @user_id";
                    using (MySqlCommand cmdUpdateInventory = new MySqlCommand(updateInventoryQuery, connect, transaction))
                    {
                        cmdUpdateInventory.Parameters.AddWithValue("@quantity_sold", quantitySold);
                        cmdUpdateInventory.Parameters.AddWithValue("@product_id", prodid.SelectedItem.ToString());
                        cmdUpdateInventory.Parameters.AddWithValue("@user_id", userId); // Added user_id

                        cmdUpdateInventory.ExecuteNonQuery();
                    }

                    // Step 6: Insert transaction into cashbook
                    decimal totalSaleAmount = salePrice * quantitySold;
                    string insertCashbookQuery = @"INSERT INTO cashbook (UserID, transaction_date, description, amount, transaction_type, FinancialYearID) 
                                           VALUES (@user_id, NOW(), @description, @amount, @transaction_type, @FinancialYearID)";
                    using (MySqlCommand cmdInsertCashbook = new MySqlCommand(insertCashbookQuery, connect, transaction))
                    {
                        cmdInsertCashbook.Parameters.AddWithValue("@user_id", userId); // Added user_id
                        cmdInsertCashbook.Parameters.AddWithValue("@description", $"Sale of {prodname.Text.Trim()}");
                        cmdInsertCashbook.Parameters.AddWithValue("@amount", totalSaleAmount);
                        cmdInsertCashbook.Parameters.AddWithValue("@transaction_type", btnCash.Checked ? "Cash" : "Credit");
                        cmdInsertCashbook.Parameters.AddWithValue("@FinancialYearID", activeYearID);

                        cmdInsertCashbook.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("Sale saved successfully with cost of sales and recorded in cashbook!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving sale: " + ex.Message);
                }
                finally
                {
                    connect.Close();
                }
            }
        }
        // Checkbox event to ensure only one is checked at a time
        private void btnCash_CheckedChanged(object sender, EventArgs e)
        {
            if (btnCash.Checked) btnCredit.Checked = false;
        }

        private void btnCredit_CheckedChanged(object sender, EventArgs e)
        {
            if (btnCredit.Checked) btnCash.Checked = false;
        }
        private void UpdateSale(int userId)
        {
            if (prodid.SelectedItem == null || string.IsNullOrEmpty(txtsale.Text) || qty.Value <= 0 || string.IsNullOrEmpty(txtprice.Text))
            {
                MessageBox.Show("Please fill in all fields before updating.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (MySqlConnection connect = new MySqlConnection($"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                connect.Open();
                MySqlTransaction transaction = connect.BeginTransaction();

                try
                {
                    // Step 1: Get the current stock quantity from the inventory for this user
                    string getStockQuery = @"SELECT stock_quantity FROM inventory WHERE prod_id = @product_id AND UserID = @user_id";
                    decimal currentStock = 0;

                    using (MySqlCommand cmdGetStock = new MySqlCommand(getStockQuery, connect, transaction))
                    {
                        cmdGetStock.Parameters.AddWithValue("@product_id", prodid.SelectedItem.ToString());
                        cmdGetStock.Parameters.AddWithValue("@user_id", userId); // Filter by UserID
                        object result = cmdGetStock.ExecuteScalar();

                        if (result != null)
                        {
                            currentStock = Convert.ToDecimal(result);
                        }
                        else
                        {
                            MessageBox.Show("Product not found in inventory for this user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // Step 2: Update sale data for this user
                    string updateSaleQuery = @"UPDATE sales 
                                       SET product_id = @product_id, product_name = @name, quantity_sold = @quantity_sold, sale_price = @sale_price, sale_date = NOW() 
                                       WHERE sale_id = @sale_id AND UserID = @user_id";
                    using (MySqlCommand cmdUpdateSale = new MySqlCommand(updateSaleQuery, connect, transaction))
                    {
                        cmdUpdateSale.Parameters.AddWithValue("@product_id", prodid.SelectedItem.ToString());
                        cmdUpdateSale.Parameters.AddWithValue("@name", prodname.Text.Trim());
                        cmdUpdateSale.Parameters.AddWithValue("@quantity_sold", qty.Value);
                        cmdUpdateSale.Parameters.AddWithValue("@sale_price", txtprice.Text.Trim());
                        cmdUpdateSale.Parameters.AddWithValue("@sale_id", txtsale.Text.Trim());
                        cmdUpdateSale.Parameters.AddWithValue("@user_id", userId); // Filter by UserID

                        cmdUpdateSale.ExecuteNonQuery();
                    }

                    // Step 3: Update inventory for this user
                    decimal newQuantitySold = qty.Value;
                    decimal newStockQuantity = currentStock - newQuantitySold;

                    string updateInventoryQuery = @"UPDATE inventory 
                                            SET stock_quantity = @new_stock_quantity 
                                            WHERE prod_id = @product_id AND UserID = @user_id";
                    using (MySqlCommand cmdUpdateInventory = new MySqlCommand(updateInventoryQuery, connect, transaction))
                    {
                        cmdUpdateInventory.Parameters.AddWithValue("@new_stock_quantity", newStockQuantity);
                        cmdUpdateInventory.Parameters.AddWithValue("@product_id", prodid.SelectedItem.ToString());
                        cmdUpdateInventory.Parameters.AddWithValue("@user_id", userId); // Filter by UserID

                        cmdUpdateInventory.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("Sale updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Error updating sale: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connect.Close();
                }
            }
        }

        private void Displayallsales()
        {
            int userId = Session.UserID; // Replace with your method to get the current UserID
            AllSales sales = new AllSales();
            List<AllSales> userSales = sales.AllSalesData(userId);

            datasales.DataSource = userSales;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            UpdateSale(Session.UserID);
            Displayallsales();
            ClearFields();
        }
        private void deletesale_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtsale.Text))
            {
                MessageBox.Show("Please select a sale to delete.");
                return;
            }

            int userId = Session.UserID; // Replace with your method to get the current user's ID

            using (MySqlConnection connect = new MySqlConnection($"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();

                    string deleteSaleQuery = @"DELETE FROM sales WHERE sale_id = @sale_id AND UserID = @user_id";
                    using (MySqlCommand cmdDeleteSale = new MySqlCommand(deleteSaleQuery, connect))
                    {
                        cmdDeleteSale.Parameters.AddWithValue("@sale_id", txtsale.Text);
                        cmdDeleteSale.Parameters.AddWithValue("@user_id", userId); // Filter by userId
                        int rowsAffected = cmdDeleteSale.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Sale deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            Displayallsales();
                        }
                        else
                        {
                            MessageBox.Show("Sale not found or not authorized for deletion.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting sale: " + ex.Message);
                }
                finally
                {
                    connect.Close();
                }
            }
        }
        private void ClearFields()
        {
            // Clear all the textboxes
            txtsale.Text = string.Empty;      // Clear Sale ID TextBox
            prodid.SelectedItem = null;       // Clear the selected Product ID in ComboBox
            prodname.Text = string.Empty;     // Clear Product Name TextBox
            qty.Value = 0;                    // Reset the quantity to 0 in NumericUpDown
            txtprice.Text = string.Empty;     // Clear Sale Price TextBox
            txtsaledate.Text = string.Empty;  // Clear Sale Date TextBox
        }


        private void clear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void datasales_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = datasales.Rows[e.RowIndex];

                if (row.Cells["ID"].Value != null)
                {
                    txtsale.Text = row.Cells["ID"].Value.ToString();
                }

                if (row.Cells["ProductID"].Value != null)
                {
                    prodid.Text = row.Cells["ProductID"].Value.ToString();
                }

                if (row.Cells["ProductName"].Value != null)
                {
                    prodname.Text = row.Cells["ProductName"].Value.ToString();
                }

                if (row.Cells["QuantitySold"].Value != null)
                {
                    qty.Text = row.Cells["QuantitySold"].Value.ToString();
                }

                if (row.Cells["SalePrice"].Value != null)
                {
                    txtprice.Text = row.Cells["SalePrice"].Value.ToString();
                }

                if (row.Cells["SaleDate"].Value != null && row.Cells["SaleDate"].Value != DBNull.Value)
                {
                    txtsaledate.Text = Convert.ToDateTime(row.Cells["SaleDate"].Value).ToString("yyyy-MM-dd HH:mm:ss");
                }

                // Fetch the Purchase Price from the Inventory table
                string productID = prodid.Text;
                decimal purchasePrice = 0;
                using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
                {
                    try
                    {
                        connect.Open();
                        string query = "SELECT purchase_price FROM inventory WHERE prod_id = @prod_id";
                        using (MySqlCommand cmd = new MySqlCommand(query, connect))
                        {
                            cmd.Parameters.AddWithValue("@prod_id", productID);
                            object result = cmd.ExecuteScalar();
                            if (result != null)
                            {
                                purchasePrice = Convert.ToDecimal(result);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error fetching purchase price: " + ex.Message);
                    }
                }

                // Calculate Cost of Sale (Quantity Sold * Purchase Price)
                if (!string.IsNullOrEmpty(qty.Text) && purchasePrice > 0)
                {
                    decimal quantitySold = Convert.ToDecimal(qty.Text);
                    decimal costOfSale = quantitySold * purchasePrice;
                    cos.Text = costOfSale.ToString("F2");  // Display cost of sale
                }
            }
        }
        private void ProcessSale(int selectedProductID)
        {
            int userId = Session.UserID;
            decimal discount = 0;
            decimal customSalePrice = 0;
            decimal finalSalePrice = 0;
            string saleType = btnCash.Checked ? "Cash" : "Credit";
            DateTime? dueDate = null;

            // Check if custom sale price is provided
            if (decimal.TryParse(txtCSP.Text, out decimal parsedCSP))
            {
                customSalePrice = parsedCSP;
            }
            else
            {
                // Retrieve the original selling price from the inventory
                customSalePrice = GetSellingPrice(selectedProductID, userId);
            }

            // Apply discount if entered
            if (decimal.TryParse(txtdiscount.Text, out decimal parsedDiscount))
            {
                discount = parsedDiscount;
                finalSalePrice = customSalePrice - discount;
            }
            else
            {
                finalSalePrice = customSalePrice;
            }

            // If it's a credit sale, set the due date
            if (saleType == "Credit" && duedate.Value != null)
            {
                dueDate = duedate.Value;
            }

            // Update the database with the sale information
            RecordSale(selectedProductID, finalSalePrice, saleType, dueDate, userId);
        }

        // Method to fetch selling price from inventory
        private decimal GetSellingPrice(int productID, int userId)
        {
            using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                connect.Open();
                string query = "SELECT selling_price FROM inventory WHERE prod_id = @ProductID AND UserID = @UserID";
                using (MySqlCommand cmd = new MySqlCommand(query, connect))
                {
                    cmd.Parameters.AddWithValue("@ProductID", productID);
                    cmd.Parameters.AddWithValue("@UserID", userId); // Filter by UserID
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToDecimal(result);
                    }
                }
            }
            return 0; // Default value in case of failure
        }
        private void RecordSale(int productID, decimal salePrice, string saleType, DateTime? dueDate, int userId)
        {
            using (MySqlConnection connect = new MySqlConnection(@"server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                connect.Open();
                string query = @"INSERT INTO sales (product_id, sale_price, sale_type, due_date, UserID) 
                         VALUES (@ProductID, @SalePrice, @SaleType, @DueDate, @UserID)";
                using (MySqlCommand cmd = new MySqlCommand(query, connect))
                {
                    cmd.Parameters.AddWithValue("@ProductID", productID);
                    cmd.Parameters.AddWithValue("@SalePrice", salePrice);
                    cmd.Parameters.AddWithValue("@SaleType", saleType);
                    cmd.Parameters.AddWithValue("@DueDate", dueDate.HasValue ? (object)dueDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserID", userId); // Associate sale with the user

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void guna2HtmlLabel8_Click(object sender, EventArgs e)
        {

        }
        public class ReceiptPrinter
        {
            private PrintDocument printDocument;
            private string contentToPrint;

            public ReceiptPrinter(string content)
            {
                printDocument = new PrintDocument();
                contentToPrint = content;
                printDocument.PrintPage += PrintDocument_PrintPage;
            }

            public void PrintContent()
            {
                PrintDialog printDialog = new PrintDialog
                {
                    Document = printDocument
                };

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDocument.Print();
                }

                // Ask user if they want to save as PDF after printing

            }




            private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
            {
                //   e.Graphics.DrawString(contentToPrint, new Font("Arial", 12), Brushes.Black, new PointF(100, 100));
                int startX = 10;
                int startY = 10;
                int offsetY = 20;
                Font headerFont = new Font("Arial", 14, FontStyle.Bold);
                Font bodyFont = new Font("Arial", 10);
                Font footerFont = new Font("Arial", 9, FontStyle.Italic);

                // Add Company Header
                e.Graphics.DrawString("Tendo Data Campany", headerFont, Brushes.Black, startX, startY);
                e.Graphics.DrawString("kamwokya", bodyFont, Brushes.Black, startX, startY + offsetY);
                e.Graphics.DrawString("Phone: (123) 456-7890", bodyFont, Brushes.Black, startX, startY + offsetY * 2);
                offsetY += 60;

                // Add Date/Time
                e.Graphics.DrawString($"Date: {DateTime.Now:MM/dd/yyyy}", bodyFont, Brushes.Black, startX, startY + offsetY);
                e.Graphics.DrawString($"Time: {DateTime.Now:hh:mm tt}", bodyFont, Brushes.Black, startX + 200, startY + offsetY);
                offsetY += 40;

                // Draw Itemized Content
                e.Graphics.DrawString(contentToPrint, bodyFont, Brushes.Black, startX, startY + offsetY);
                offsetY += 100;

                // Footer Section
                e.Graphics.DrawString("Thank you for shopping with us!", footerFont, Brushes.Black, startX, startY + offsetY);
            }
        }




        private void GenerateReceipt()
        {
            decimal salePrice = 0;

            // Check if the custom sale price is to be used
            if (chkAboveBelowSalePrice.Checked)
            {
                decimal.TryParse(txtCSP.Text, out salePrice);
            }
            else
            {
                decimal.TryParse(txtprice.Text, out salePrice);
            }

            decimal totalPrice = salePrice * qty.Value;

            string receiptContent = $"--- RECEIPT ---\n" +
                                    $"Product: {prodname.Text}\n" +
                                    $"Quantity: {qty.Value}\n" +
                                    $"Sale Price: {salePrice:C}\n" +
                                    $"Total: {totalPrice:C}\n";

            var receiptPrinter = new ReceiptPrinter(receiptContent);
            receiptPrinter.PrintContent();
        }

        private void GenerateInvoice()
        {
            decimal salePrice = 0;

            // Check if the custom sale price is to be used
            if (chkAboveBelowSalePrice.Checked)
            {
                decimal.TryParse(txtCSP.Text, out salePrice);
            }
            else
            {
                decimal.TryParse(txtprice.Text, out salePrice);
            }

            decimal totalPrice = salePrice * qty.Value;
            // "C0", new System.Globalization.CultureInfo("en-UG")
            string invoiceContent = $"--- INVOICE ---\n" +
                                    $"Product: {prodname.Text}\n" +
                                    $"Quantity: {qty.Value}\n" +
                                    $"Sale Price: {salePrice:C}\n" +
                                    $"Due Date: {duedate.Value.ToShortDateString()}\n" +
                                    $"Total: {totalPrice:C}\n";

            var invoicePrinter = new ReceiptPrinter(invoiceContent);
            invoicePrinter.PrintContent();
        }
        private void btnreceipt_Click(object sender, EventArgs e)
        {
            if (btnCash.Checked)
            {
                GenerateReceipt();
            }
            else
            {
                MessageBox.Show("Receipt can only be issued for cash sales.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btninvoice_Click(object sender, EventArgs e)
        {
            if (btnCredit.Checked)
            {
                GenerateInvoice();
            }
            else
            {
                MessageBox.Show("Invoice can only be issued for credit sales.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SearchSales(string searchText, int userId)
        {
            using (MySqlConnection connect = new MySqlConnection("server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV"))
            {
                try
                {
                    connect.Open();

                    // Query to search sales by product_name, sale_date, and filter by UserID
                    string salesQuery = @"
                SELECT sale_id, product_name, product_id, quantity_sold, sale_price, sale_date, 
                       cost_of_sales, discount, custom_sale_price, sale_type, due_date
                FROM sales
                WHERE (product_name LIKE @searchText OR DATE_FORMAT(sale_date, '%Y-%m-%d') LIKE @searchText)
                AND UserID = @UserID";

                    using (MySqlCommand cmdSales = new MySqlCommand(salesQuery, connect))
                    {
                        cmdSales.Parameters.AddWithValue("@searchText", "%" + searchText + "%");
                        cmdSales.Parameters.AddWithValue("@UserID", userId); // Filter by UserID

                        using (MySqlDataAdapter salesAdapter = new MySqlDataAdapter(cmdSales))
                        {
                            DataTable salesResults = new DataTable();
                            salesAdapter.Fill(salesResults);

                            // Display the sales results in dataGridViewSales
                            datasales.DataSource = salesResults;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error during sales search: " + ex.Message);
                }
            }
        }
        public void ProdId()
        {
            // Ensure a product is selected
            if (prodid.SelectedItem == null)
                return;

            // Get the selected product ID
            string selectedProductID = prodid.SelectedItem.ToString();

            // Retrieve the current user's ID (replace this with actual session management logic)
            int userId = Session.UserID; // Replace this with the actual method to get the logged-in user's ID
            if (userId <= 0)
            {
                MessageBox.Show("User session has expired. Please log in again.", "Session Expired", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Define the connection string (ideally, store it in a configuration file)
            string connectionString = "server=localhost;user id=root;password=sap;persistsecurityinfo=True;database=IV";

            // Query to fetch product details for the specific user
            string query = @"
        SELECT product_name, selling_price 
        FROM inventory 
        WHERE prod_id = @prod_id AND UserID = @user_id"; // Ensure column name matches DB schema

            using (MySqlConnection connect = new MySqlConnection(connectionString))
            {
                try
                {
                    connect.Open();

                    using (MySqlCommand cmd = new MySqlCommand(query, connect))
                    {
                        // Add parameters for product ID and user ID
                        cmd.Parameters.AddWithValue("@prod_id", selectedProductID);
                        cmd.Parameters.AddWithValue("@user_id", userId);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Check if the query returned a result
                            if (reader.Read())
                            {
                                // Safely update UI elements with product details
                                prodname.Text = reader["product_name"]?.ToString() ?? "N/A";
                                txtprice.Text = reader["selling_price"]?.ToString() ?? "0.00";
                            }
                            else
                            {
                                MessageBox.Show("No product details found for the selected ID and user.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                catch (MySqlException sqlEx)
                {
                    MessageBox.Show("Database error: " + sqlEx.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        private void datasales_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            int userId = Session.UserID; // Replace with your method for retrieving the current UserID
            //SearchSales(searchTextBox.Text, userId);
            SearchSales(txtSearchSales.Text.Trim(), userId);
        }

        private void txtSearchSales_TextChanged(object sender, EventArgs e)
        {
            int userId = Session.UserID;
            SearchSales(txtSearchSales.Text.Trim(), userId);
        }

        private void prodid_Click(object sender, EventArgs e)
        {
            ProdId();
        }

        private void prodid_SelectedIndexChanged(object sender, EventArgs e)
        {
            ProdId();
        }

        private void export_Click(object sender, EventArgs e)
        {
            ExportToExcel(datasales, "Sales Report");
        }
    }
}
    
