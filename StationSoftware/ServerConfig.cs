using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace StationSoftware
{
    public partial class ServerConfig : Form
    {
        bool changed;
        bool loadover;
        public ServerConfig()
        {
            InitializeComponent();
        }

        private void LoadServerConfig()
        {
            try
            {
                string IPAddrs = Common.GetAppSetting("IPAddrs");
                textBox1.Text = IPAddrs;
                string ip = Common.GetAppSetting("ip");
                if (!string.IsNullOrEmpty(ip))
                {
                    textBox6.Text = ip;
                }
                else
                {
                    textBox6.Text = "192.168.1.10";
                }
                string port = Common.GetAppSetting("port");
                if (!string.IsNullOrEmpty(port))
                {
                    numericUpDown4.Value = Convert.ToInt32(port);
                }
                else
                {
                    numericUpDown4.Value = 520;
                }
                string connStr = Common.GetConnectionString("RemoteServer");
                SqlConnectionStringBuilder scsb = new SqlConnectionStringBuilder(connStr);
                textBox2.Text = scsb.DataSource;
                textBox3.Text = scsb.InitialCatalog;
                textBox4.Text = scsb.UserID;
                textBox5.Text = scsb.Password;
            }
            catch (Exception ex)
            {
                MessageBox.Show("载入服务器配置出错：" + ex.Message);
            }
        }

        private void SaveServerConfig()
        {
            try
            {
                string IPAddrs = textBox1.Text.Trim();
                Common.SaveAppSetting("IPAddrs", IPAddrs);
                string ip = textBox6.Text.Trim();
                Common.SaveAppSetting("ip", ip);
                int port = (int)numericUpDown4.Value;
                Common.SaveAppSetting("port", port.ToString());
                SqlConnectionStringBuilder scsb = new SqlConnectionStringBuilder();
                scsb.DataSource = textBox2.Text.Trim();
                scsb.InitialCatalog = textBox3.Text.Trim();
                scsb.UserID = textBox4.Text.Trim();
                scsb.Password = textBox5.Text.Trim();
                Common.SaveConnectionString("RemoteServer", scsb);
                changed = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存服务器配置出错：" + ex.Message);
            }
        }

        private void ServerConfig_Load(object sender, EventArgs e)
        {
            LoadServerConfig();
            loadover = true;
        }

        private void ServerConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (changed && MessageBox.Show("您已经更改过配置未保存，是否立即保存？", "退出提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK)
            {
                SaveServerConfig();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (loadover)
            {
                changed = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveServerConfig();
        }
    }
}
