using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Echevil;
using System.Threading;
using System.Diagnostics;
using System.Configuration;
using FileTransmitor;
using System.IO;
using System.Data.Common;

namespace StationSoftware
{
    public partial class Index : Form
    {
        const int CLOSE_SIZE = 12;
        private bool sure;
        NetworkMonitor netMonitor;
        System.Timers.Timer timer2;

        private void ResetDevices()
        {
            SignAndControl sac = new SignAndControl();
            //sac.Control();...输出控制各个通道的信号，重启各部分的机构

        }

        public static Index GetInstance()
        {
            if (instance == null || instance.IsDisposed)
            {
                instance = new Index();
            }
            if (!instance.Visible)
                instance.Show();
            instance.WindowState = FormWindowState.Maximized;
            instance.BringToFront();
            instance.Focus();
            return instance;
        }

        private Index()
        {
            InitializeComponent();
            this.tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;//由自己绘制标题
            this.tabControl1.Padding = new System.Drawing.Point(CLOSE_SIZE + 2, 2);
            this.tabControl1.DrawItem += new DrawItemEventHandler(this.tabControl1_DrawItem);
            this.tabControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tabControl1_MouseDown);
            string ids = ConfigurationManager.AppSettings["IPAddrs"];
            string inter = ConfigurationManager.AppSettings["Inter"];
            string timeo = ConfigurationManager.AppSettings["Timeout"];
            int interval = 10000;
            int RET;
            if (int.TryParse(inter, out RET))
            {
                interval = RET;
            }
            int timeout = 1000;
            if (int.TryParse(timeo, out RET))
            {
                timeout = RET;
            }
            SafeTimer.Checking += new EventHandler<IntEventArgs>(SafeTimer_Checking);
            SafeTimer.Init(interval, timeout, ids);
        }

        void SafeTimer_Checking(object sender, IntEventArgs e)
        {
            CheckTerminalIsLink(SafeTimer.State, e.Interval, e.Timeout);
            SynArguments();
        }

        private void SynArguments()
        {
            using (SqlHelper sqlHelper = new SqlHelper("RemoteServer"))
            {
                DataTable dt = sqlHelper.ExecuteQueryDataTable("select * from s_argument");
                int c = dt.Rows.Count;
                for (int i = 0; i < c; i++)
                {
                    string argument_name = dt.Rows[i]["argument_name"].ToString();
                    short device_type_id = Convert.ToInt16(dt.Rows[i]["device_type_id"]);
                    short point_type_id = Convert.ToInt16(dt.Rows[i]["point_type_id"]);
                    string standard_value = dt.Rows[i]["standard_value"].ToString();
                    string min_value = dt.Rows[i]["min_value"].ToString();
                    string max_value = dt.Rows[i]["max_value"].ToString();
                    string valueIsNumeric = Convert.ToBoolean(dt.Rows[i]["valueIsNumeric"]) ? "1" : "0";
                    string isRange = Convert.ToBoolean(dt.Rows[i]["isRange"]) ? "1" : "0";
                    string isEnable = Convert.ToBoolean(dt.Rows[i]["isEnable"]) ? "1" : "0";
                    using (SqlHelper sqlHelperLoc = new SqlHelper())
                    {
                        sqlHelperLoc.ExecuteNonQuery("if exists (select 1 from s_argument where argument_name='" + argument_name + "') update s_argument set device_type_id=" + device_type_id + ", point_type_id=" + point_type_id + ", standard_value='" + standard_value + "', min_value='" + min_value + "', max_value='" + max_value + "', valueIsNumeric=" + valueIsNumeric + ", isRange=" + isRange + ", isEnable=" + isEnable + " where argument_name='" + argument_name + "' else insert into s_argument (device_type_id, point_type_id, argument_name, standard_value, min_value, max_value, valueIsNumeric, isRange, isEnable) values (" + device_type_id + ", " + point_type_id + ", '" + argument_name + "', '" + standard_value + "', '" + min_value + "', '" + max_value + "', " + valueIsNumeric + ", " + isRange + ", " + isEnable + ")");
                    }
                }
            }
        }

        void CheckTerminalIsLink(object ips, int interval, int timeout)
        {
            if (ips != null)
            {
                System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                string ipAddrs = ips.ToString();
                string[] ipss = ipAddrs.Split('|');
                foreach (string ipAddr in ipss)
                {
                    System.Net.NetworkInformation.PingReply pr = ping.Send(ipAddr, timeout > interval ? interval : timeout);
                    bool off = pr.Status != System.Net.NetworkInformation.IPStatus.Success;
                    if (off)
                        ShowStatus("与服务器的连接已断开！");
                    else
                        ShowStatus("Ready...");
                }
                ping.Dispose();
            }
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                Rectangle myTabRect = this.tabControl1.GetTabRect(e.Index);
                e.Graphics.FillRectangle(new SolidBrush(SystemColors.Control), myTabRect);
                //先添加TabPage属性   
                e.Graphics.DrawString(this.tabControl1.TabPages[e.Index].Text
                , this.Font, SystemBrushes.ControlText, myTabRect.X + 2, myTabRect.Y + 2);


                //再画一个矩形框
                using (Pen p = new Pen(Color.Black))
                {
                    myTabRect.Offset(myTabRect.Width - (CLOSE_SIZE + 3), 2);
                    myTabRect.Width = CLOSE_SIZE;
                    myTabRect.Height = CLOSE_SIZE;
                    e.Graphics.DrawRectangle(p, myTabRect);
                }
                //填充矩形框
                Color recColor = e.State == DrawItemState.Selected ? Color.MediumVioletRed : Color.DarkGray;
                using (Brush b = new SolidBrush(recColor))
                {
                    e.Graphics.FillRectangle(b, myTabRect);
                }

                //画关闭符号
                using (Pen p = new Pen(Color.White))
                {
                    //画"\"线
                    Point p1 = new Point(myTabRect.X + 3, myTabRect.Y + 3);
                    Point p2 = new Point(myTabRect.X + myTabRect.Width - 3, myTabRect.Y + myTabRect.Height - 3);
                    e.Graphics.DrawLine(p, p1, p2);

                    //画"/"线
                    Point p3 = new Point(myTabRect.X + 3, myTabRect.Y + myTabRect.Height - 3);
                    Point p4 = new Point(myTabRect.X + myTabRect.Width - 3, myTabRect.Y + 3);
                    e.Graphics.DrawLine(p, p3, p4);
                }
                e.Graphics.Dispose();
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace(true);
                StackFrame sf = st.GetFrame(0);
                string fileName = sf.GetFileName();
                Type type = sf.GetMethod().ReflectedType;
                string assName = type.Assembly.FullName;
                string typeName = type.FullName;
                string methodName = sf.GetMethod().Name;
                int lineNo = sf.GetFileLineNumber();
                int colNo = sf.GetFileColumnNumber();
                Logs.LogError(fileName + " : " + assName + "." + typeName + "." + methodName + "(" + lineNo + "行" + colNo + "列)", ex.Message);
            }
        }

        private void tabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int x = e.X, y = e.Y;

                //计算关闭区域   
                Rectangle myTabRect = this.tabControl1.GetTabRect(this.tabControl1.SelectedIndex);

                myTabRect.Offset(myTabRect.Width - (CLOSE_SIZE + 3), 2);
                myTabRect.Width = CLOSE_SIZE;
                myTabRect.Height = CLOSE_SIZE;

                //如果鼠标在区域内就关闭选项卡   
                bool isClose = x > myTabRect.X && x < myTabRect.Right
                 && y > myTabRect.Y && y < myTabRect.Bottom;

                if (isClose)
                {
                    CloseSubForm(this.tabControl1.SelectedTab);
                }
            }
        }

        #region
        public void AddTabPage(string str, Form form)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                ShowStatus("正在拼命加载中，请稍候...");
                try
                {
                    AddTabForm(str, form);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    ShowStatus("Ready...");
                }
            });
        }

        public void ShowStatus(string msg)
        {
            if (statusStrip1.InvokeRequired)
            {
                statusStrip1.BeginInvoke(new MethodInvoker(delegate
                {
                    toolStripStatusLabel1.Text = msg;
                    statusStrip1.Refresh();
                }));
            }
            else
            {
                toolStripStatusLabel1.Text = msg;
                statusStrip1.Refresh();
            }
        }

        private void AddTabForm(string str, Form form)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string, Form>(AddTabForm), str, form);
            }
            else
            {
                int have = TabControlCheckHave(this.tabControl1, form.GetType().FullName);
                if (have > -1)
                {
                    tabControl1.SelectTab(have);
                    tabControl1.SelectedTab.Text = str;
                    tabControl1.SelectedTab.Controls.Clear();
                    form.TopLevel = false;//设置非顶级窗口
                    form.Parent = tabControl1.SelectedTab;
                    form.WindowState = FormWindowState.Maximized;
                    form.FormBorderStyle = FormBorderStyle.None;
                    form.Show();
                }
                else
                {
                    tabControl1.TabPages.Add(str);
                    tabControl1.SelectTab(tabControl1.TabPages.Count - 1);
                    string classFullname = form.GetType().FullName;
                    tabControl1.SelectedTab.Tag = classFullname;
                    int index = classFullname.LastIndexOf(".");
                    string className = classFullname;
                    if (index > -1)
                        className = classFullname.Substring(index + 1);
                    tabControl1.SelectedTab.Name = "Tab_" + className;
                    form.TopLevel = false;//设置非顶级窗口
                    form.Parent = tabControl1.SelectedTab;
                    form.WindowState = FormWindowState.Maximized;
                    form.FormBorderStyle = FormBorderStyle.None;
                    form.Show();
                }
            }
        }

        public void CloseSubForm(TabPage page)
        {
            string name = page.Name.Substring(4);
            Control[] cs = page.Controls.Find(name, false);
            if (cs != null && cs.Length > 0)
            {
                if (cs[0] is Form)
                {
                    Form f = cs[0] as Form;
                    f.Close();
                }
            }
            this.tabControl1.TabPages.Remove(page);
        }

        //判断TabPage是否已创建
        private int TabControlCheckHave(TabControl tab, string tabPage)
        {
            int index = -1;
            for (int i = 0; i < tab.TabPages.Count; i++)
            {
                TabPage tp = tab.TabPages[i];
                if (tp.Tag.ToString() == tabPage)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        #endregion

        internal void SureExit()
        {
            SafeTimer.Dispose();
            notifyIcon1.Dispose();
            Environment.Exit(0);
        }

        void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            MethodInvoker mi = new MethodInvoker(RefreshNetworkInfo);
            mi.BeginInvoke(null, null);
        }

        private void RefreshNetworkInfo()
        {
            if (netMonitor.Adapters.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                int i = 0;
                foreach (NetworkAdapter adp in netMonitor.Adapters)
                {
                    i++;
                    string netAvanda = "[网卡(" + i.ToString() + "): " + Math.Round(adp.DownloadSpeedKbps, 2) + "Kbps/" + Math.Round(adp.UploadSpeedKbps, 2) + "Kbps]";
                    if (sb.Length == 0)
                        sb.Append(netAvanda);
                    else
                        sb.Append(" " + netAvanda);
                }
                Action<string> a = new Action<string>(RefreshNet);
                a.BeginInvoke(sb.ToString(), null, null);
            }
        }
        private void RefreshNet(string info)
        {
            if (statusStrip1.InvokeRequired)
            {
                statusStrip1.BeginInvoke(new Action<string>(a =>
                {
                    toolStripStatusLabel3.Text = a;
                    statusStrip1.Refresh();
                }), info);
            }
            else
            {
                toolStripStatusLabel3.Text = info;
                statusStrip1.Refresh();
            }
        }

        static Index instance;

        public static Index Instance
        {
            get { return Index.instance; }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ShowUI();
        }

        private void ShowUI()
        {
            this.Show();
            this.BringToFront();
            this.Focus();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            sure = true;
            this.Close();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sure = true;
            this.Close();
        }

        private void Index_Load(object sender, EventArgs e)
        {
            netMonitor = new NetworkMonitor();
            if (netMonitor.Adapters.Length > 0)
                netMonitor.StartMonitoring();
            timer2 = new System.Timers.Timer(1000);
            timer2.AutoReset = true;
            timer2.Elapsed += new System.Timers.ElapsedEventHandler(timer2_Elapsed);
            timer2.Start();
            ShowStatus("正在初始化系统...");
            Login login = new Login();
            if (login.ShowDialog() == DialogResult.OK)
            {
                Logs.LogLogin();
                ShowStatus("Ready...");
            }
            else
            {
                SureExit();
            }
        }

        private void Index_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sure)
            {
                if (MessageBox.Show("确定要退出系统么？", "退出提醒", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    if (netMonitor.Adapters.Length > 0)
                        netMonitor.StopMonitoring();
                    if (timer2.Enabled)
                        timer2.Stop();
                    SureExit();
                }
                else
                {
                    sure = false;
                    e.Cancel = true;
                }
            }
            else
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowUI();
        }

        public bool SynFiles(DateTime start, DateTime end)
        {
            try
            {
                List<string> files = new List<string>();
                //获取在start至end时段内未同步的文件列表：
                using (SqlHelper sqlHelper = new SqlHelper())
                {
                    DbDataReader reader = sqlHelper.ExecuteQueryReader("select filepath from d_picVid_log where saveTime between '" + start + "' and '" + end + "' and tag=0");
                    while (reader.Read())
                    {
                        //int id = reader.GetInt32(0);
                        string file = reader.GetString(0);
                        if (!string.IsNullOrEmpty(file))
                        {
                            FileTransmiter.SendWorker worker = new FileTransmiter.SendWorker(FileTransmiter.RealEndPoint);
                            SendFile(file, worker);
                            sqlHelper.ExecuteNonQuery("update d_picVid_log set tag=1, tagTime=getdate() where filepath='" + file + "'");
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace(true);
                StackFrame sf = st.GetFrame(0);
                string fileName = sf.GetFileName();
                Type type = sf.GetMethod().ReflectedType;
                string assName = type.Assembly.FullName;
                string typeName = type.FullName;
                string methodName = sf.GetMethod().Name;
                int lineNo = sf.GetFileLineNumber();
                int colNo = sf.GetFileColumnNumber();
                Logs.LogError(fileName + " : " + assName + "." + typeName + "." + methodName + "(" + lineNo + "行" + colNo + "列)", ex.Message);
                return false;
            }
        }

        public bool SynData(DateTime start, DateTime end)
        {
            try
            {
                SynTrains(start, end);
                SynDatas(start, end);
                SynAlarms(start, end);
                return true;
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace(true);
                StackFrame sf = st.GetFrame(0);
                string fileName = sf.GetFileName();
                Type type = sf.GetMethod().ReflectedType;
                string assName = type.Assembly.FullName;
                string typeName = type.FullName;
                string methodName = sf.GetMethod().Name;
                int lineNo = sf.GetFileLineNumber();
                int colNo = sf.GetFileColumnNumber();
                Logs.LogError(fileName + " : " + assName + "." + typeName + "." + methodName + "(" + lineNo + "行" + colNo + "列)", ex.Message);
                return false;
            }
        }

        private void SendFile(string file, FileTransmiter.SendWorker worker)
        {
            if (File.Exists(file))
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(Send), new SendArgs(file, worker));
            }
            else
            {
                StackTrace st = new StackTrace(true);
                StackFrame sf = st.GetFrame(0);
                string fileName = sf.GetFileName();
                Type type = sf.GetMethod().ReflectedType;
                string assName = type.Assembly.FullName;
                string typeName = type.FullName;
                string methodName = sf.GetMethod().Name;
                int lineNo = sf.GetFileLineNumber();
                int colNo = sf.GetFileColumnNumber();
                Logs.LogError(fileName + " : " + assName + "." + typeName + "." + methodName + "(" + lineNo + "行" + colNo + "列)", "指定的文件[" + file + "]不存在！");
            }
        }

        private void Send(object o)
        {
            SendArgs arg = o as SendArgs;
            FileTransmiter.SupperSend(FileTransmiter.RealEndPoint, arg.File, arg.Worker, new Action<string, int, bool, double>(ReportStatus), new Action<string, long>(ReportSpeed));
        }

        private void ReportStatus(string id, int percent, bool finished, double elapsedMilliseconds)
        {
            if (statusStrip1.IsDisposed)
                return;
            if (statusStrip1.InvokeRequired)
            {
                statusStrip1.BeginInvoke(new Action<string, int, bool, double>(ReportStatus), id, percent, finished, elapsedMilliseconds);
            }
            else
            {
                if (finished)
                {
                    toolStripProgressBar1.Value = 0;
                    statusStrip1.Refresh();
                    string file = id.Split('|')[1];
                    using (SqlHelper sqlHelper = new SqlHelper())
                    {
                        DbDataReader reader = sqlHelper.ExecuteQueryReader("select train_log_id, isVideo, saveTime from d_picVid_log from d_picVid_log where filepath='" + file + "'");
                        if (reader.Read())
                        {
                            int train_log_id = reader.GetInt32(0);
                            string isVideo = reader.GetBoolean(1) ? "1" : "0";
                            DateTime saveTime = reader.GetDateTime(2);
                            using (SqlHelper sqlHelperRem = new SqlHelper(ConfigurationManager.ConnectionStrings["RemoteServer"].ConnectionString))
                            {
                                sqlHelperRem.ExecuteNonQuery("if not exists (select 1 from d_picVid_log where filepath='" + file + "') insert into d_picVid_log (filepath, train_log_id, isVideo, saveTime) values ('" + file + "', " + train_log_id + ", " + isVideo + ", '" + saveTime + "')");
                            }
                            sqlHelper.ExecuteNonQuery("update d_picVid_log set tag=2, tagTime=getdate() where filepath='" + file + "'");
                        }
                    }
                    return;
                }
                toolStripProgressBar1.Value = percent;
                statusStrip1.Refresh();
            }
        }
        [Obsolete("暂时不用", true)]
        private void UpdateSynStatus(string file, int status)
        {
            using (SqlHelper sqlHelperLoc = new SqlHelper())
            {
                sqlHelperLoc.ExecuteNonQuery("update d_picVid_log set tag=" + status + ", tagTime=getdate() where filepath='" + file + "'");
            }
        }

        private void ReportSpeed(string id, long speed)
        {
            if (statusStrip1.IsDisposed)
                return;
            if (statusStrip1.InvokeRequired)
            {
                statusStrip1.BeginInvoke(new Action<string, long>(ReportSpeed), id, speed);
            }
            else
            {
                toolStripStatusLabel5.Text = FileTransmitor.Common.ByteConvertToGBMBKB(speed) + "/S";
                statusStrip1.Refresh();
            }
        }

        private void SynAlarms(DateTime start, DateTime end)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                DbDataReader reader = sqlHelper.ExecuteQueryReader("select id, train_log_id, device_id, point_type_id, start_time, end_time, alarm_value, alarm_status, affirmance, remark from d_alarm_log where start_time between '" + start + "' and '" + end + "' and tag=0");
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    int train_log_id = reader.GetInt32(1);
                    int device_id = reader.GetInt32(2);
                    short point_type_id = reader.GetInt16(3);
                    DateTime start_time = reader.GetDateTime(4);
                    DateTime end_time = reader.GetDateTime(5);
                    decimal alarm_value = reader.GetDecimal(6);
                    short alarm_status = reader.GetInt16(7);
                    int affirmance = reader.GetInt32(8);
                    string remark = reader.GetString(9);
                    sqlHelper.ExecuteNonQuery("update d_alarm_log set tag=1, tagTime=getdate() where id=" + id);
                    using (SqlHelper sqlHelperRem = new SqlHelper(ConfigurationManager.ConnectionStrings["RemoteServer"].ConnectionString))
                    {
                        sqlHelperRem.ExecuteNonQuery("if not exists (select 1 from d_alarm_log where train_log_id=" + train_log_id + " and device_id=" + device_id + " and point_type_id=" + point_type_id + ") insert into d_alarm_log (train_log_id, device_id, point_type_id, start_time, end_time, alarm_value, alarm_status, affirmance, remark) values (" + train_log_id + ", " + device_id + ", " + point_type_id + ", '" + start_time + "', '" + end_time + "', " + alarm_value + ", " + alarm_status + ", " + affirmance + ", '" + remark + "')");
                    }
                    sqlHelper.ExecuteNonQuery("update d_alarm_log set tag=2, tagTime=getdate() where id=" + id);
                }
            }
        }

        private void SynDatas(DateTime start, DateTime end)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                DbDataReader reader = sqlHelper.ExecuteQueryReader("select id, train_log_id, device_id, point_type_id, flash_time, data_value, alarm_status from d_data_log where start_time between '" + start + "' and '" + end + "' and tag=0");
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    int train_log_id = reader.GetInt32(1);
                    int device_id = reader.GetInt32(2);
                    short point_type_id = reader.GetInt16(3);
                    DateTime flash_time = reader.GetDateTime(4);
                    decimal data_value = reader.GetDecimal(6);
                    short alarm_status = reader.GetInt16(7);
                    sqlHelper.ExecuteNonQuery("update d_data_log set tag=1, tagTime=getdate() where id=" + id);
                    using (SqlHelper sqlHelperRem = new SqlHelper(ConfigurationManager.ConnectionStrings["RemoteServer"].ConnectionString))
                    {
                        sqlHelperRem.ExecuteNonQuery("if not exists (select 1 from d_data_log where train_log_id=" + train_log_id + " and device_id=" + device_id + " and point_type_id=" + point_type_id + ") insert into d_data_log (train_log_id, device_id, point_type_id, flash_time, data_value, alarm_status) values (" + train_log_id + ", " + device_id + ", " + point_type_id + ", '" + flash_time + "', " + data_value + ", " + alarm_status + ")");
                    }
                    sqlHelper.ExecuteNonQuery("update d_data_log set tag=2, tagTime=getdate() where id=" + id);
                }
            }
        }

        private void SynTrains(DateTime start, DateTime end)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                DbDataReader reader = sqlHelper.ExecuteQueryReader("select id, train_id, station_id, direction, come_time, go_time, alarm_count, alarm_status from d_train_log where start_time between '" + start + "' and '" + end + "' and tag=0");
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    int train_id = reader.GetInt32(1);
                    int station_id = reader.GetInt32(2);
                    string direction = reader.GetBoolean(3) ? "1" : "0";
                    DateTime come_time = reader.GetDateTime(4);
                    DateTime go_time = reader.GetDateTime(4);
                    int alarm_count = reader.GetInt32(6);
                    short alarm_status = reader.GetInt16(7);
                    sqlHelper.ExecuteNonQuery("update d_train_log set tag=1, tagTime=getdate() where id=" + id);
                    using (SqlHelper sqlHelperRem = new SqlHelper(ConfigurationManager.ConnectionStrings["RemoteServer"].ConnectionString))
                    {
                        sqlHelperRem.ExecuteNonQuery("if not exists (select 1 from d_train_log where train_id=" + train_id + " and come_time='" + come_time + "') insert into d_train_log (train_id, station_id, direction, come_time, go_time, alarm_count, alarm_status) values (" + train_id + ", " + station_id + ", " + direction + ", '" + come_time + "', '" + go_time + "', " + alarm_count + ", " + alarm_status + ")");
                    }
                    sqlHelper.ExecuteNonQuery("update d_train_log set tag=2, tagTime=getdate() where id=" + id);
                }
            }
        }

        private void 登录日志ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogForm lf = new LogForm(0);
            AddTabPage("日志列表-登录", lf);
        }

        private void 信号日志ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogForm lf = new LogForm(1);
            AddTabPage("日志列表-信号", lf);
        }

        private void 错误日志ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogForm lf = new LogForm(2);
            AddTabPage("日志列表-错误", lf);
        }

        private void 文档同步ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            DateTime start = now.Date;
            DateTime end = new DateTime(start.Year, start.Month, start.Day, 23, 59, 59);
            SynFiles(start, end);
        }

        private void 数据同步ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            DateTime start = now.Date;
            DateTime end = new DateTime(start.Year, start.Month, start.Day, 23, 59, 59);
            SynData(start, end);
        }

        private void 重启设备ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetDevices();
        }

        private void 本地配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LocalConfig lc = new LocalConfig();
            AddTabPage("本地配置", lc);
        }

        private void 服务器配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ServerConfig sc = new ServerConfig();
            AddTabPage("服务器配置", sc);
        }
    }

    public class SendArgs
    {
        string file;

        public string File
        {
            get { return file; }
            set { file = value; }
        }
        FileTransmiter.SendWorker worker;

        public FileTransmiter.SendWorker Worker
        {
            get { return worker; }
            set { worker = value; }
        }
        public SendArgs(string file, FileTransmiter.SendWorker worker)
        {
            this.file = file;
            this.worker = worker;
        }
    }
}
