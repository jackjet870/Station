using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace StationSoftware
{
    public partial class LogForm : Form
    {
        DataTable dt;
        string type = "未知";
        public LogForm(int type)
        {
            InitializeComponent();
            winFormPager1.PageSize = 20;
            if (type == 0)
                this.type = "登录";
            else if (type == 1)
                this.type = "信号";
            else if (type == 2)
                this.type = "错误";
            this.Text = "日志列表-" + this.type;
        }

        private void LoginLog_Load(object sender, EventArgs e)
        {
            winFormPager1.PageIndex = 1;
            Query(this.type);
            int pageCount;
            string msg;
            DataView dv = Paging.GetPagerForView(dt, winFormPager1.PageSize, winFormPager1.PageIndex, out pageCount, out msg);
            GridUtil.BindData(outlookGrid1, dv.Table);
        }

        void Query(string type)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                int allCount = 0;
                dt = sqlHelper.ExecuteQueryDataTable("select * from v_error_log where 日志级别='" + type + "' order by 发生时间 desc");
                if (dt != null)
                    allCount = dt.Rows.Count;
                winFormPager1.RecordCount = allCount;
            }
        }

        private void winFormPager1_PageIndexChanged(object sender, EventArgs e)
        {
            int pageCount;
            string msg;
            DataView dv = Paging.GetPagerForView(dt, winFormPager1.PageSize, winFormPager1.PageIndex, out pageCount, out msg);
            GridUtil.BindData(outlookGrid1, dv.Table);
        }
    }
}
