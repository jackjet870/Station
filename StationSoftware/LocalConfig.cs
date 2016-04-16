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
    public partial class LocalConfig : Form
    {
        bool changed;
        bool loadover;
        static string filename = AppDomain.CurrentDomain.BaseDirectory + "signal.xml";
        public LocalConfig()
        {
            InitializeComponent();
        }

        private void LoadLocalConfig()
        {
            try
            {
                string comNum = Common.GetAppSetting("comNum");
                if (!string.IsNullOrEmpty(comNum))
                {
                    numericUpDown1.Value = Convert.ToInt32(comNum);
                }
                else
                {
                    numericUpDown1.Value = 1;
                }
                string Inter = Common.GetAppSetting("Inter");
                if (!string.IsNullOrEmpty(Inter))
                {
                    numericUpDown2.Value = Convert.ToInt32(Inter);
                }
                else
                {
                    numericUpDown2.Value = 10000;
                }
                string Timeout = Common.GetAppSetting("Timeout");
                if (!string.IsNullOrEmpty(Timeout))
                {
                    numericUpDown3.Value = Convert.ToInt32(Timeout);
                }
                else
                {
                    numericUpDown3.Value = 1000;
                }
                string connStr = Common.GetConnectionString("MonitoringSystem");
                SqlConnectionStringBuilder scsb = new SqlConnectionStringBuilder(connStr);
                textBox2.Text = scsb.DataSource;
                textBox3.Text = scsb.InitialCatalog;
                textBox4.Text = scsb.UserID;
                textBox5.Text = scsb.Password;
                string dictName;
                Dictionary<string, Channel> dict = Common.LoadDictionaryFromFile<Channel>(filename, out dictName);
                if (dict != null)
                {
                    foreach (string key in dict.Keys)
                    {
                        uint channel = 0;
                        Channel c = dict[key];
                        if (c != null)
                            channel = c.ChannelValue;
                        string dir = key.Substring(0, 3);
                        uint channelNo = Convert.ToUInt32(key.Substring(10));
                        if (dir == "in_")//"in_channel0"
                        {
                            switch (channelNo)
                            {
                                case 1:
                                    numericUpDown4.Value = channel;
                                    break;
                                case 2:
                                    numericUpDown5.Value = channel;
                                    break;
                                case 3:
                                    numericUpDown6.Value = channel;
                                    break;
                                case 4:
                                    numericUpDown7.Value = channel;
                                    break;
                                case 5:
                                    numericUpDown8.Value = channel;
                                    break;
                                case 6:
                                    numericUpDown9.Value = channel;
                                    break;
                                case 7:
                                    numericUpDown10.Value = channel;
                                    break;
                                case 8:
                                    numericUpDown11.Value = channel;
                                    break;
                            }
                        }
                        else if (dir == "ou_")
                        {
                            switch (channel)
                            {
                                case 1:
                                    numericUpDown12.Value = channel;
                                    break;
                                case 2:
                                    numericUpDown13.Value = channel;
                                    break;
                                case 3:
                                    numericUpDown14.Value = channel;
                                    break;
                                case 4:
                                    numericUpDown15.Value = channel;
                                    break;
                                case 5:
                                    numericUpDown16.Value = channel;
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("载入本地配置出错：" + ex.Message);
            }
        }

        private void SaveLocalConfig()
        {
            try
            {
                int comNum = (int)numericUpDown1.Value;
                Common.SaveAppSetting("comNum", comNum.ToString());
                int Inter = (int)numericUpDown2.Value;
                Common.SaveAppSetting("Inter", Inter.ToString());
                int Timeout = (int)numericUpDown3.Value;
                Common.SaveAppSetting("Timeout", Timeout.ToString());
                SqlConnectionStringBuilder scsb = new SqlConnectionStringBuilder();
                scsb.DataSource = textBox2.Text.Trim();
                scsb.InitialCatalog = textBox3.Text.Trim();
                scsb.UserID = textBox4.Text.Trim();
                scsb.Password = textBox5.Text.Trim();
                Common.SaveConnectionString("MonitoringSystem", scsb);
                Dictionary<string, Channel> dict = new Dictionary<string, Channel>();
                dict.Add("in_channel1", new Channel((uint)numericUpDown4.Value));
                dict.Add("in_channel2", new Channel((uint)numericUpDown5.Value));
                dict.Add("in_channel3", new Channel((uint)numericUpDown6.Value));
                dict.Add("in_channel4", new Channel((uint)numericUpDown7.Value));
                dict.Add("in_channel5", new Channel((uint)numericUpDown8.Value));
                dict.Add("in_channel6", new Channel((uint)numericUpDown9.Value));
                dict.Add("in_channel7", new Channel((uint)numericUpDown10.Value));
                dict.Add("in_channel8", new Channel((uint)numericUpDown11.Value));
                dict.Add("ou_channel1", new Channel((uint)numericUpDown12.Value));
                dict.Add("ou_channel2", new Channel((uint)numericUpDown13.Value));
                dict.Add("ou_channel3", new Channel((uint)numericUpDown14.Value));
                dict.Add("ou_channel4", new Channel((uint)numericUpDown15.Value));
                dict.Add("ou_channel5", new Channel((uint)numericUpDown16.Value));
                if (Common.SaveDictionaryToFile<Channel>("signal", dict, filename))
                    changed = false;
                else
                    MessageBox.Show("保存信号映射配置出错！");
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存本地配置出错：" + ex.Message);
            }
        }

        private void LocalConfig_Load(object sender, EventArgs e)
        {
            LoadLocalConfig();
            loadover = true;
        }

        private void LocalConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (changed && MessageBox.Show("您已经更改过配置未保存，是否立即保存？", "退出提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK)
            {
                SaveLocalConfig();
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (loadover)
            {
                changed = true;
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (loadover)
            {
                changed = true;
            }
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (loadover)
            {
                changed = true;
            }
        }

        private void maskedTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (loadover)
            {
                changed = true;
            }
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if (loadover)
            {
                changed = true;
            }
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            if (loadover)
            {
                changed = true;
            }
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            if (loadover)
            {
                changed = true;
            }
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            if (loadover)
            {
                changed = true;
            }
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            if (loadover)
            {
                changed = true;
            }
        }

        private void numericUpDown10_ValueChanged(object sender, EventArgs e)
        {
            if (loadover)
            {
                changed = true;
            }
        }

        private void numericUpDown11_ValueChanged(object sender, EventArgs e)
        {
            if (loadover)
            {
                changed = true;
            }
        }

        private void numericUpDown12_ValueChanged(object sender, EventArgs e)
        {
            if (loadover)
            {
                changed = true;
            }
        }

        private void numericUpDown13_ValueChanged(object sender, EventArgs e)
        {
            if (loadover)
            {
                changed = true;
            }
        }

        private void numericUpDown14_ValueChanged(object sender, EventArgs e)
        {
            if (loadover)
            {
                changed = true;
            }
        }

        private void numericUpDown15_ValueChanged(object sender, EventArgs e)
        {
            if (loadover)
            {
                changed = true;
            }
        }

        private void numericUpDown16_ValueChanged(object sender, EventArgs e)
        {
            if (loadover)
            {
                changed = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveLocalConfig();
        }
    }
}
