using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POS
{
    public partial class frmMain : Form
    {
        
        public frmMain()
        {
            InitializeComponent();
            
        }


        private void frmMain_Load(object sender, EventArgs e)
        {
           
            btnMax.PerformClick();

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2CirclePictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void centerpanel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
