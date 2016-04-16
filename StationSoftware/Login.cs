using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace StationSoftware
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string s =  txtName.Text.Trim();
            if (s.Length >= 8)
            {
                string auth = ConfigurationManager.AppSettings["auth"];
                if (s.Equals(DateTime.Now.ToString("yyMMddHH") + auth, StringComparison.InvariantCultureIgnoreCase))
                {
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("授权码错误！");
                }
            }
            else
            {
                MessageBox.Show("授权码错误！");
            }
        }

        private void txtPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                button1.PerformClick();
            }
        }
    }
}
