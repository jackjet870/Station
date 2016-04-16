using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Reflection;
using System.Net;
using System.Configuration;
using System.Collections.Generic;
using System.Management;
using KellOutlookGrid;
using System.Xml;
using System.ComponentModel;

namespace StationSoftware
{
    /// <summary>
    /// Common 的摘要说明
    /// </summary>
    public static class Common
    {
        public static DataTable LoadTableFromFile(string filename)
        {
            if (Path.GetExtension(filename).Equals(".xml", StringComparison.InvariantCultureIgnoreCase))
            {
                DataTable data = new DataTable();
                try
                {
                    XmlReadMode xrm = data.ReadXml(filename);
                }
                catch (Exception e)
                {
                    MessageBox.Show("读取XML文件出错：" + e.Message);
                }
                return data;
            }
            MessageBox.Show("只接受XML文件，载入出错！");
            return null;
        }

        public static bool SaveTableToFile(DataTable data, string filename)
        {
            if (data == null || data.Columns.Count == 0)
                return false;
            if (string.IsNullOrEmpty(data.TableName))
                data.TableName = "配置表";
            try
            {
                data.WriteXml(filename, XmlWriteMode.WriteSchema);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("写入XML文件出错：" + e.Message);
                return false;
            }
        }
        public static Dictionary<string, T> LoadDictionaryFromFile<T>(string filename, out string dictName) where T : IStringConverter<T>, new()
        {
            dictName = null;
            if (!File.Exists(filename))
                return null;
            Dictionary<string, T> dict = new Dictionary<string, T>();
            if (Path.GetExtension(filename).Equals(".xml", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(filename);
                    XmlNode xNode;
                    XmlElement xElem;
                    if (xDoc.DocumentElement != null)
                    {
                        xNode = xDoc.DocumentElement;
                        dictName = xNode.Name;
                        if (xNode != null)
                        {
                            foreach (XmlNode node in xNode.ChildNodes)
                            {
                                xElem = (XmlElement)node;
                                if (xElem != null)
                                {
                                    string name = xElem.GetAttribute("name");
                                    string channel = xElem.GetAttribute("channel");
                                    if (!dict.ContainsKey(name))
                                    {
                                        T t = new T();
                                        t.ConvertFrom(channel);
                                        dict.Add(name, t);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("读取XML文件出错：" + e.Message);
                }
                return dict;
            }
            MessageBox.Show("只接受XML文件，载入出错！");
            return null;
        }

        public static bool SaveDictionaryToFile<T>(string dictName, Dictionary<string, T> dict, string filename, bool reNew = false) where T : IStringConverter<T>
        {
            if (string.IsNullOrEmpty(dictName))
                return false;

            XmlDocument xDoc = new XmlDocument();
            XmlNode xNode;
            XmlElement xElem;
            try
            {
                if (!File.Exists(filename))
                {
                    using (StreamWriter sw = File.CreateText(filename))
                    {
                    }
                }
                if (!reNew)
                {
                    xDoc.Load(filename);
                }
                bool hasDeclaration = false;
                if (xDoc.ChildNodes.Count > 0)
                {
                    foreach (XmlNode n in xDoc.ChildNodes)
                    {
                        if (n.NodeType == XmlNodeType.XmlDeclaration)
                        {
                            hasDeclaration = true;
                            break;
                        }
                    }
                }
                if (!hasDeclaration)
                {
                    XmlDeclaration xmlDeclaration = xDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                    xDoc.AppendChild(xmlDeclaration);
                }
                xNode = xDoc.DocumentElement;

                if (xNode != null)
                {
                    foreach (string key in dict.Keys)
                    {
                        T t = dict[key];
                        if (t != null)
                        {
                            xElem = (XmlElement)xNode.SelectSingleNode("//add[@name='" + key + "']");
                            if (xElem != null)
                            {
                                xElem.SetAttribute("channel", t.ConvertToString());
                            }
                            else
                            {
                                XmlNode node = xDoc.CreateNode(XmlNodeType.Element, "add", null);
                                XmlAttribute attr = xDoc.CreateAttribute("name");
                                attr.Value = key;
                                node.Attributes.Append(attr);
                                XmlAttribute attr2 = xDoc.CreateAttribute("channel");
                                attr2.Value = t.ConvertToString();
                                node.Attributes.Append(attr2);
                                xNode.AppendChild(node);
                            }
                        }
                    }
                }
                else
                {
                    xNode = xDoc.CreateElement(dictName);
                    xDoc.AppendChild(xNode);
                    foreach (string key in dict.Keys)
                    {
                        T t = dict[key];
                        if (t != null)
                        {
                            xElem = (XmlElement)xNode.SelectSingleNode("//add[@name='" + key + "']");
                            if (xElem != null)
                            {
                                xElem.SetAttribute("channel", t.ConvertToString());
                            }
                            else
                            {
                                XmlNode node = xDoc.CreateNode(XmlNodeType.Element, "add", null);
                                XmlAttribute attr = xDoc.CreateAttribute("name");
                                attr.Value = key;
                                node.Attributes.Append(attr);
                                XmlAttribute attr2 = xDoc.CreateAttribute("channel");
                                attr2.Value = t.ConvertToString();
                                node.Attributes.Append(attr2);
                                xNode.AppendChild(node);
                            }
                        }
                    }
                }
                xDoc.Save(filename);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("写入XML文件出错：" + e.Message);
                return false;
            }
        }

        public static Form GetForm(string formName, object owner, object[] args)
        {
            if (!string.IsNullOrEmpty(formName))
                return null;
            List<object> para = new List<object>();
            para.Add(owner);
            para.AddRange(args);
            Assembly ass = Assembly.GetExecutingAssembly();
            Type[] types = ass.GetTypes();
            if (types != null)
            {
                foreach (Type t in types)
                {
                    if (t.IsSubclassOf(typeof(Form)) && t.Name.Equals(formName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return (Form)ass.CreateInstance(t.FullName, true, BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public, null, para.ToArray(), null, null);
                    }
                }
            }
            else
            {
                MessageBox.Show("本程序集中没有任何类！");
            }
            return null;
        }

        public static void GetParameter(ref Dictionary<string, object> args, out object target)
        {
            if (args == null)
                args = new Dictionary<string, object>();

            target = null;

            string parameter = ConfigurationManager.AppSettings["Parameter"];
            string[] paras = parameter.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (string arg in paras)
            {
                if (args.ContainsKey(arg))
                {
                    int id = Convert.ToInt32(args[arg]);
                    switch (arg)
                    {
                        case "{ID}":
                            args[arg] = id;
                            break;
                        case "{TARGET}":
                            target = args[arg];
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        public static bool SaveConnectionString(string name, SqlConnectionStringBuilder scsb, string configPath = null)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName + ".config";
            if (!string.IsNullOrEmpty(configPath))
                path = configPath;
            if (!File.Exists(path))
                return false;
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(path);
            XmlNode xNode;
            XmlElement xElem;
            xNode = xDoc.SelectSingleNode("//connectionStrings");
            if (xNode != null)
            {
                xElem = (XmlElement)xNode.SelectSingleNode("//add[@name='" + name + "']");
                if (xElem != null)
                {
                    xElem.SetAttribute("connectionString", string.Format("Data Source={0};User ID={1};Password={2};Initial Catalog={3}", scsb.DataSource, scsb.UserID, scsb.Password, scsb.InitialCatalog));
                    xDoc.Save(path);
                    return true;
                }
            }
            return false;
        }

        public static string GetConnectionString(string name, string configPath = null)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName + ".config";
            if (!string.IsNullOrEmpty(configPath))
                path = configPath;
            if (!File.Exists(path))
                return "";
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(path);
            XmlNode xNode;
            XmlElement xElem;
            xNode = xDoc.SelectSingleNode("//connectionStrings");
            if (xNode != null)
            {
                xElem = (XmlElement)xNode.SelectSingleNode("//add[@name='" + name + "']");
                if (xElem != null)
                {
                    string s = xElem.GetAttribute("connectionString");
                    return s;
                }
            }
            return "";
        }

        public static bool SaveAppSetting(string key, string value, string configPath = null)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName + ".config";
            if (!string.IsNullOrEmpty(configPath))
                path = configPath;
            if (!File.Exists(path))
                return false;
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(path);
            XmlNode xNode;
            XmlElement xElem;
            xNode = xDoc.SelectSingleNode("//appSettings");
            if (xNode != null)
            {
                xElem = (XmlElement)xNode.SelectSingleNode("//add[@key='" + key + "']");
                if (xElem != null)
                {
                    xElem.SetAttribute("value", value);
                    xDoc.Save(path);
                    return true;
                }
                else
                {
                    XmlNode node = xDoc.CreateNode(XmlNodeType.Element, "add", null);
                    XmlAttribute attr = xDoc.CreateAttribute("key");
                    attr.Value = key;
                    node.Attributes.Append(attr);
                    XmlAttribute attr2 = xDoc.CreateAttribute("value");
                    attr2.Value = value;
                    node.Attributes.Append(attr2);
                    xNode.AppendChild(node);
                    xDoc.Save(path);
                    return true;
                }
            }
            else
            {
                xNode = xDoc.CreateNode(XmlNodeType.Element, "appSettings", null);
                xDoc.AppendChild(xNode);
                xElem = (XmlElement)xNode.SelectSingleNode("//add[@key='" + key + "']");
                if (xElem != null)
                {
                    xElem.SetAttribute("value", value);
                    xDoc.Save(path);
                    return true;
                }
                else
                {
                    XmlNode node = xDoc.CreateNode(XmlNodeType.Element, "add", null);
                    XmlAttribute attr = xDoc.CreateAttribute("key");
                    attr.Value = key;
                    node.Attributes.Append(attr);
                    XmlAttribute attr2 = xDoc.CreateAttribute("value");
                    attr2.Value = value;
                    node.Attributes.Append(attr2);
                    xNode.AppendChild(node);
                    xDoc.Save(path);
                    return true;
                }
            }
        }

        public static string GetAppSetting(string key, string configPath = null)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName + ".config";
            if (!string.IsNullOrEmpty(configPath))
                path = configPath;
            if (!File.Exists(path))
                return "";
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(path);
            XmlNode xNode;
            XmlElement xElem;
            xNode = xDoc.SelectSingleNode("//appSettings");
            if (xNode != null)
            {
                xElem = (XmlElement)xNode.SelectSingleNode("//add[@key='" + key + "']");
                if (xElem != null)
                {
                    string s = xElem.GetAttribute("value");
                    return s;
                }
            }
            return "";
        }

        public static string AdminId
        {
            get
            {
                return ConfigurationManager.AppSettings["AdminId"];
            }
        }

        public static int GetIndexById(this DataTable dt, string colName, string id)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i][colName].ToString() == id)
                {
                    return i;
                }
            }
            return -1;
        }

        public static IPAddress GetIPv4()
        {
            IPAddress[] ips = Dns.GetHostAddresses("");
            foreach (IPAddress ip in ips)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return ip;
            }
            return IPAddress.Loopback;
        }

        public static string GetMac()
        {
            //using (ManagementObjectSearcher nisc = new ManagementObjectSearcher("select IPEnabled,MACAddress from Win32_NetworkAdapterConfiguration"))
            //{
            //    foreach (ManagementObject nic in nisc.Get())
            //    {
            //        if (Convert.ToBoolean(nic["IPEnabled"]))
            //        {
            //            return nic["MACAddress"].ToString();
            //        }
            //    }
            //}
            using (ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
            {
                using (ManagementObjectCollection moc = mc.GetInstances())
                {
                    foreach (ManagementObject mo in moc)
                    {
                        if (mo["IPEnabled"].ToString() == "True")
                            return mo["MacAddress"].ToString();
                    }
                }
            }
            return "";
        }
        /// <summary>
        /// 获取当前的数据库服务器名(或者IP)
        /// </summary>
        /// <returns></returns>
        public static string GetServer()
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                return sqlHelper.Conn.DataSource;
            }
        }
        /// <summary>
        /// 根据设备类型获取设备地址的前缀
        /// </summary>
        /// <param name="device_type_id"></param>
        /// <returns></returns>
        public static string GetAddressPrefix(int device_type_id)
        {
            string prefix = "";
            switch (device_type_id)
            {
                case 101:
                    prefix = "pantograph_";
                    break;
                case 102:
                    prefix = "train_";
                    break;
                case 103:
                    prefix = "plc_";
                    break;
                case 104:
                    prefix = "alnico_";
                    break;
                case 105:
                    prefix = "camera_";
                    break;
                case 106:
                    prefix = "vidicon_";
                    break;
                case 107:
                    prefix = "laser_";
                    break;
                case 108:
                    prefix = "stress_";
                    break;
            }
            return prefix;
        }
        /// <summary>
        /// 根据列车ID获取对应的编组数
        /// </summary>
        /// <param name="trainId">列车ID</param>
        /// <returns>编组数</returns>
        public static int GetCountByTrainId(int trainId)
        {
            string sql = "select [count] from m_train where object_id=" + trainId;
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                object obj = sqlHelper.ExecuteScalar(sql);
                if (obj != null && obj != DBNull.Value)
                {
                    int RET;
                    if (int.TryParse(obj.ToString(), out RET))
                    {
                        return RET;
                    }
                }
                return 0;
            }
        }
        /// <summary>
        /// 设备表中是否已经存在指定对象ID的设备
        /// </summary>
        /// <param name="object_id"></param>
        /// <returns></returns>
        public static bool HasDevice(int object_id)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                string sql = "select count(*) from m_device where object_id=" + object_id;
                object obj = sqlHelper.ExecuteScalar(sql);
                if (obj != null && obj != DBNull.Value)
                    return Convert.ToInt32(obj) > 0;
                return false;
            }
        }
        /// <summary>
        /// 获取上线或下线的状态（如果原来记录中没有则返回-1，如存在记录，且最后一个记录跟现在检测的状态一致则返回2(不必记录)，否则就返回上次的状态，如：上线0(则记录本次下线)或者下线1(则记录本次上线)）
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="off">现在检测的状态(是否断网)</param>
        /// <returns></returns>
        public static int LastOnOff(string ip, bool off)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                object obj = sqlHelper.ExecuteScalar("select top 1 onOff from d_ping_log where ip='" + ip + "' order by pingtime desc");
                if (obj != null && obj != DBNull.Value)
                {
                    bool of = Convert.ToBoolean(obj);
                    if (of == off)
                        return 2;
                    else
                        return (of ? 1 : 0);
                }
                return -1;
            }
        }
        public static string GetStandard(string point_type_id)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                string sql = "select mutation, normal_low_alarm, normal_high_alarm from s_rm_type where point_type_id=" + point_type_id;
                DataTable dt = sqlHelper.ExecuteQueryDataTable(sql);
                if (dt.Rows.Count > 0)
                    return "突变值=" + dt.Rows[0][0].ToString() + ",上下限范围[" + dt.Rows[0][1].ToString() + "," + dt.Rows[0][2].ToString() + "]";
                return "";
            }
        }

        public static string GetStandard(string object_name, string alarm_status, string object_id, string point_type_id)
        {
            string pt = GetPointPictureId(Convert.ToInt32(object_id));
            string s = "前";
            if (pt == "B_")
                s = "后";
            if (object_name == s + "滑板磨耗")
            {
                return GetSX(point_type_id);
            }
            else if (object_name == s + "滑板缺口")
            {
                return GetSX(point_type_id);
            }
            else if (object_name == "中心偏移平均值")
            {
                return GetSX(point_type_id);
            }
            else if (object_name == "上下倾斜平均值")
            {
                return GetSX(point_type_id);
            }
            else if (object_name == "前后倾斜量")
            {
                return GetSX(point_type_id);
            }
            else
            {
                if (alarm_status == "1")
                {
                    return GetTB(point_type_id);
                }
                else
                {
                    return GetXX(point_type_id) + "～" + GetSX(point_type_id);
                }
            }
        }

        private static string GetSX(string ptid)
        {
            string sx = "0";
            DataSet Ds = new DataSet();
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                object o = sqlHelper.ExecuteScalar("Select max_value from s_argument where isEnable=1 and point_type_id=" + ptid);//报警值表
                if (o != null && o != DBNull.Value)
                    sx = o.ToString();//上限
            }
            return sx;
        }

        private static string GetXX(string ptid)
        {
            string xx = "0";
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                object o = sqlHelper.ExecuteScalar("Select min_value from s_argument where isEnable=1 and point_type_id=" + ptid);//报警值表                
                if (o != null && o != DBNull.Value)
                    xx = o.ToString();//下限
            }
            return xx;
        }

        private static string GetTB(string ptid)
        {
            string tb = "0";
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                object o = sqlHelper.ExecuteScalar("Select standard_value from s_argument where isEnable=1 and point_type_id=" + ptid);//报警值表
                if (o != null && o != DBNull.Value)
                    tb = o.ToString();//突变
            }
            return tb;
        }

        /// <summary>
        /// 可以匹配yyyyMM,yyyyMMdd,yyyyMMddHH,yyyyMMddHHmm,yyyyMMddHHmmss,yyyyMMddHHmmssfff等时间格式
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPicPath(string path)
        {
            string p = path;
            //根据无连接符号的时间格式来找
            Match mat = Regex.Match(p, @"(\d{1,4}[0-1]\d[0-3]\d[0-2]\d[0-5]\d[0-5]\d\d{3})|(\d{1,4}[0-1]\d[0-3]\d[0-2]\d[0-5]\d[0-5]\d)|(\d{1,4}[0-1]\d[0-3]\d[0-2]\d[0-5]\d)|(\d{1,4}[0-1]\d[0-3]\d[0-2]\d)|(\d{1,4}[0-1]\d[0-3]\d)");
            if (mat.Success)
            {
                p = mat.Value;
            }
            //int index = path.LastIndexOf(@"\");
            //if (index > -1)//a\sc\c=4
            //{
            //    p = path.Substring(0, index);//a\sc=1
            //    int i = p.LastIndexOf(@"\");
            //    if (i > -1)
            //        p = p.Substring(i + 1);//sc
            //}
            return p;
        }

        public static string GetMohaoOrQuekou(int objectid)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                string sql = "select object_name from m_object where object_id=" + objectid;
                DataTable dt = sqlHelper.ExecuteQueryDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    string objectname = dt.Rows[0][0].ToString();
                    if (objectname.Contains("磨耗"))
                    {
                        return "磨耗";
                    }
                    else
                    {
                        return "缺口";
                    }
                }
                return "磨耗";
            }
        }

        public static string GetLine(int station_id)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                string sql = "select line_no from m_station where id = " + station_id;
                DataTable dt = sqlHelper.ExecuteQueryDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    return dt.Rows[0][0].ToString();
                }
                return "";
            }
        }

        public static string GetStation(int station_id)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                string sql = "select station_name from m_station where id = " + station_id;
                DataTable dt = sqlHelper.ExecuteQueryDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    return dt.Rows[0][0].ToString();
                }
                return "";
            }
        }

        public static string GetAddress(int station_id)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                string sql = "select line_no,station_name from m_station where id = " + station_id;
                DataTable dt = sqlHelper.ExecuteQueryDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    return dt.Rows[0][0].ToString() + " " + dt.Rows[0][1].ToString();
                }
                return "";
            }
        }

        public static string GetFrontOrBack(int device_id)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                string sql = "select device_name from m_device where device_id = " + device_id;
                DataTable dt = sqlHelper.ExecuteQueryDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    return dt.Rows[0][0].ToString();
                }
                return "";
            }
        }

        public static string GetPointPictureId(int device_id)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                string sql = "select device_name from m_device where device_id = " + device_id;
                DataTable dt = sqlHelper.ExecuteQueryDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    string objectname = dt.Rows[0][0].ToString();
                    if (objectname.Contains("球铰高度"))
                    {
                        return "1";
                    }
                    else if (objectname.Contains("前滑板"))
                    {
                        return "A_";
                    }
                    else if (objectname.Contains("后滑板"))
                    {
                        return "B_";
                    }
                }
                return "1";
            }
        }

        public static DataTable GetAlarmType(int alarm_type_id)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                string sql = "select * from s_alarm_type where alarm_type_id=" + alarm_type_id;
                DataTable dt = sqlHelper.ExecuteQueryDataTable(sql);
                return dt;
            }
        }

        public static DataTable GetStatusType(int status_type_id)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                string sql = "select * from s_status_type where status_type_id=" + status_type_id;
                DataTable dt = sqlHelper.ExecuteQueryDataTable(sql);
                return dt;
            }
        }

        public static DataTable GetPointType(int point_type_id)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                string sql = "select * from s_point_type where point_type_id=" + point_type_id;
                DataTable dt = sqlHelper.ExecuteQueryDataTable(sql);
                return dt;
            }
        }

        public static DataTable GetDeviceType(int device_type_id)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                string sql = "select * from s_device_type where device_type_id=" + device_type_id;
                DataTable dt = sqlHelper.ExecuteQueryDataTable(sql);
                return dt;
            }
        }

        public static DataTable GetTrain(int trainid)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                string sql = "select * from m_train where id=" + trainid;
                DataTable dt = sqlHelper.ExecuteQueryDataTable(sql);
                return dt;
            }
        }

        public static DataTable GetDevice(int deviceid)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                string sql = "select * from m_device where id=" + deviceid;
                DataTable dt = sqlHelper.ExecuteQueryDataTable(sql);
                return dt;
            }
        }

        public static DataTable GetDevices(int devicetype)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                string sql = "select * from m_device where device_type_id=" + devicetype;
                DataTable dt = sqlHelper.ExecuteQueryDataTable(sql);
                return dt;
            }
        }

        public static string GetEnableId(string name)
        {
            string id = "2";
            if (name == "不可见不报警")
            {
                id = "0";
            }
            else if (name == "可见不报警")
            {
                id = "1";
            }
            else if (name == "可见可报警")
            {
                id = "2";
            }
            return id;
        }

        public static string GetEnableById(string id)
        {
            string name = "可见可报警";
            int RET;
            if (int.TryParse(id, out RET))
            {
                if (RET == 0)
                {
                    name = "不可见不报警";
                }
                else if (RET == 1)
                {
                    name = "可见不报警";
                }
                else if (RET == 2)
                {
                    name = "可见可报警";
                }
            }
            return name;
        }

        public static string GetDirection(string code)
        {
            return code == "0" ? "正向" : "反向";
        }

        public static string GetDirectionCode(string dir)
        {
            return dir == "正向" ? "0" : "1";
        }

        public static DateTime GetLastestAlarmTime()
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                string sql = "select top 1 start_time from d_alarm_log where alarm_status<>0 and affirmance=0 order by start_time desc";
                object obj = sqlHelper.ExecuteScalar(sql);
                if (obj != null && obj != DBNull.Value)
                {
                    return Convert.ToDateTime(obj);
                }
                return DateTime.MinValue;
            }
        }

        public static DateTime GetAlarmTimeById(int id)
        {
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                string sql = "select start_time from d_alarm_log where id=" + id;
                object obj = sqlHelper.ExecuteScalar(sql);
                if (obj != null && obj != DBNull.Value)
                {
                    return Convert.ToDateTime(obj);
                }
                return DateTime.MinValue;
            }
        }
    }
    public class CryptUtil
    {
        static MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

        public static MD5CryptoServiceProvider Md5
        {
            get { return md5; }
        }

        public static string GetMD5Hash(string input)
        {
            input = input ?? "";
            byte[] res = md5.ComputeHash(Encoding.Unicode.GetBytes(input), 0, input.Length);
            char[] temp = new char[res.Length];
            System.Array.Copy(res, temp, res.Length);
            return new String(temp);
        }

        static DESCryptoServiceProvider des = new DESCryptoServiceProvider();

        public static DESCryptoServiceProvider DES
        {
            get { return des; }
        }

        const string EncryptionKey = "OuKell";
        const string EncryptionIV = "kell";

        public static string EncodeDes(string input)
        {
            byte[] SourceData = Encoding.Unicode.GetBytes(input);
            byte[] returnData = null;
            try
            {
                des.Key = ASCIIEncoding.Unicode.GetBytes(EncryptionKey);
                des.IV = ASCIIEncoding.Unicode.GetBytes(EncryptionIV);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(SourceData, 0, SourceData.Length);
                cs.FlushFinalBlock();
                returnData = ms.ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Encoding.Unicode.GetString(returnData);
        }

        public static string DecodeDes(string input)
        {
            byte[] SourceData = Encoding.Unicode.GetBytes(input);
            byte[] returnData = null;
            try
            {
                DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();
                desProvider.Key = Encoding.Unicode.GetBytes(EncryptionKey);
                desProvider.IV = Encoding.Unicode.GetBytes(EncryptionIV);
                MemoryStream ms = new MemoryStream();
                ICryptoTransform encrypto = desProvider.CreateDecryptor();
                CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);
                cs.Write(SourceData, 0, SourceData.Length);
                cs.FlushFinalBlock();
                returnData = ms.ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Encoding.Unicode.GetString(returnData);
        }
    }
    public static class Paging
    {
        public static int GetPageCount(int record_count, int page_size)
        {
            int pagecount = 0;
            if (record_count % page_size == 0)
                pagecount = record_count / page_size;
            else
                pagecount = (record_count / page_size) + 1;

            return pagecount;
        }

        public static DataView GetPagerForView(DataTable dt, int page_size, int page_index, out int page_count, out string msg)
        {
            page_index = page_index - 1;
            DataView dv = new DataView();
            page_count = 0;
            if (dt != null)
            {
                int recordCount = dt.Rows.Count; //总记录数
                int page_sum = GetPageCount(recordCount, page_size);
                if (page_size < dt.Rows.Count)//kl2 :SQL查询函数返回的DATASET   
                {
                    if (page_size == 0)//text_intpase :判断用户设置的分页是否合法
                        page_size = 10;
                    //recordCount = kl2.Tables[0].Rows.Count;//假设每页只显示1条数据,则共可以显示的页数:pagemark页
                    if (page_size < 1)
                    {
                        msg = "请将page_size设置在[1-" + dt.Rows.Count.ToString() + "]之间";
                    }
                    msg = "共" + page_sum.ToString() + "页," + dt.Rows.Count.ToString() + "条";//page_num :lable
                    DataTable page_table = new DataTable();//记录当前正在操作的是哪个表,全局变量,值由查询函数获取
                    page_table.TableName = dt.TableName;
                    for (int k = 0; k < dt.Columns.Count; k++)
                    {
                        page_table.Columns.Add(dt.Columns[k].ColumnName);
                    }
                    if (dt.Rows.Count != 0 && page_size < dt.Rows.Count)
                    {
                        page_table.Clear();
                        try    //普通页面显示
                        {
                            page_table.Clear();
                            for (int i = 0; i < page_size; i++)
                            {
                                page_table.Rows.Add(dt.Rows[i + (page_index * page_size)].ItemArray);
                            }
                        }
                        catch //最后不足一个页面的显示
                        {
                            page_table.Clear();
                            try
                            {
                                for (int s = 0; s < recordCount - (page_index * page_size); s++)
                                {
                                    page_table.Rows.Add(dt.Rows[s + (page_index * page_size)].ItemArray);
                                }
                            }
                            catch { }
                        }
                        msg += "　当前第" + (page_index + 1).ToString() + "页";
                    }
                    dv = page_table.DefaultView;
                }
                else
                {
                    dv = dt.DefaultView;
                    msg = "共1页," + dt.Rows.Count.ToString() + "条";
                    msg += "　当前第" + (page_index + 1).ToString() + "页";
                }
                page_count = page_sum;
                return dv;
            }
            else
            {
                msg = "没有数据！";
                return null;
            }
        }
    }
    public static class GridUtil
    {
        /// <summary>
        /// 转换原始数据，以便于在数据网格中正确显示
        /// </summary>
        /// <param name="dt">原始数据</param>
        /// <param name="columnHeaders">要显示(更改)的列头名称</param>
        /// <param name="removeColumns">要移除的列</param>
        /// <param name="colNewOrder">重新排序原来数据的列顺序(如果有移除的列，则表示移除后的顺序)</param>
        /// <returns></returns>
        public static DataTable ViewData(DataTable dt, Dictionary<string, string> columnHeaders, List<string> removeColumns = null, List<string> colNewOrder = null)
        {
            if (dt == null)
                return null;
            DataTable result = dt.Copy();
            if (columnHeaders != null && columnHeaders.Count > 0)
            {
                foreach (string colName in columnHeaders.Keys)
                {
                    int index = result.Columns.IndexOf(colName);
                    if (index > -1)
                    {
                        result.Columns[index].ColumnName = columnHeaders[colName];
                    }
                }
            }
            if (removeColumns != null && removeColumns.Count > 0)
            {
                foreach (string colName in removeColumns)
                {
                    result.Columns.Remove(colName);
                }
            }
            if (colNewOrder != null && colNewOrder.Count > 0)
            {
                for (int i = 0; i < colNewOrder.Count; i++)
                {
                    result.Columns[colNewOrder[i]].SetOrdinal(i);
                }
            }
            return result;
        }

        public static void AddLinkColumn(OutlookGrid outlookGrid1, string text, out string columnKey, CallbackArgs callback = null)
        {
            columnKey = "#Link#";
            DataGridViewLinkColumn link = new DataGridViewLinkColumn();
            if (outlookGrid1.Columns.Contains(columnKey))
                columnKey = "#Link#" + DateTime.Now.Ticks.ToString().Substring(4);
            link.Name = columnKey;
            //link.Text = text;
            link.HeaderText = text;
            link.UseColumnTextForLinkValue = true;
            link.TrackVisitedState = true;
            outlookGrid1.Columns.Add(link);
            if (callback != null)
                AddNewEvenHandler(outlookGrid1, columnKey, callback);
        }

        public static void AddLinkColumn(OutlookGrid outlookGrid1, string text, string columnKey = "#LinkSpecial#", CallbackArgs callback = null)
        {
            DataGridViewLinkColumn link = new DataGridViewLinkColumn();
            if (outlookGrid1.Columns.Contains(columnKey))
                columnKey = "#Link#" + DateTime.Now.Ticks.ToString().Substring(4);
            link.Name = columnKey;
            //link.Text = text;
            link.HeaderText = text;
            link.UseColumnTextForLinkValue = true;
            link.TrackVisitedState = true;
            outlookGrid1.Columns.Add(link);
            if (callback != null)
                AddNewEvenHandler(outlookGrid1, columnKey, callback);
        }

        public static void AddButtonColumn(OutlookGrid outlookGrid1, string text, out string columnKey, CallbackArgs callback = null)
        {
            columnKey = "#Button#";
            if (outlookGrid1.Columns.Contains(columnKey))
                columnKey = "#Button#" + DateTime.Now.Ticks.ToString().Substring(4);
            DataGridViewButtonColumn button = new DataGridViewButtonColumn();
            button.Name = columnKey;
            //button.Text = text;
            button.HeaderText = text;
            outlookGrid1.Columns.Add(button);
            if (callback != null)
                AddNewEvenHandler(outlookGrid1, columnKey, callback);
        }

        public static void AddButtonColumn(OutlookGrid outlookGrid1, string text, string columnKey = "#ButtonSpecial#", CallbackArgs callback = null)
        {
            if (outlookGrid1.Columns.Contains(columnKey))
                columnKey = "#Button#" + DateTime.Now.Ticks.ToString().Substring(4);
            DataGridViewButtonColumn button = new DataGridViewButtonColumn();
            button.Name = columnKey;
            //button.Text = text;
            button.HeaderText = text;
            outlookGrid1.Columns.Add(button);
            if (callback != null)
                AddNewEvenHandler(outlookGrid1, columnKey, callback);
        }

        public static void ClearData(OutlookGrid outlookGrid1)
        {
            outlookGrid1.BindData(null, null);
        }

        public static void BindData(OutlookGrid outlookGrid1, DataTable dt, bool isGroup = false, int groupColumnIndex = -1, int sumColumn = -1, ListSortDirection direction = ListSortDirection.Ascending, int sortColumnIndex = -1)
        {
            outlookGrid1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            outlookGrid1.ReadOnly = true;
            outlookGrid1.AllowUserToAddRows = false;
            outlookGrid1.BindData(dt, null);
            if (isGroup)
            {
                outlookGrid1.GroupTemplate = new OutlookGridMoneyGroup();
                outlookGrid1.GroupTemplate.Column = outlookGrid1.Columns[groupColumnIndex];
                outlookGrid1.SumColumn = sumColumn;
                outlookGrid1.CollapseIcon = KellOutlookGrid.Properties.Resources.ExpandBig.ToBitmap();
                outlookGrid1.ExpandIcon = KellOutlookGrid.Properties.Resources.CollapseBig.ToBitmap();
            }
            if (sortColumnIndex > -1)
                outlookGrid1.Sort(sortColumnIndex, direction);
            else if (groupColumnIndex > -1)
                outlookGrid1.Sort(groupColumnIndex, direction);
        }

        public static void ClearGroup(OutlookGrid outlookGrid1)
        {
            outlookGrid1.ClearGroups();
        }

        public static void ExpandAll(OutlookGrid outlookGrid1)
        {
            outlookGrid1.ExpandAll();
        }

        public static void CollapseAll(OutlookGrid outlookGrid1)
        {
            outlookGrid1.CollapseAll();
        }

        public static void RegisterLinkEvent(OutlookGrid outlookGrid1, string columnKey, CallbackArgs callback)
        {
            AddNewEvenHandler(outlookGrid1, columnKey, callback);
        }

        private static void AddNewEvenHandler(OutlookGrid outlookGrid1, string columnKey, CallbackArgs callback)
        {
            Dictionary<string, CallbackArgs> callbacks = outlookGrid1.Tag as Dictionary<string, CallbackArgs>;
            if (callbacks != null)
            {
                if (!callbacks.ContainsKey(columnKey))
                {
                    callbacks.Add(columnKey, callback);
                    //outlookGrid1.Tag = callbacks;
                }
                else
                {
                    callbacks[columnKey] = callback;
                }
            }
            else
            {
                outlookGrid1.CellContentClick += OutlookGrid1_CellContentClick;
                Dictionary<string, CallbackArgs> callbackss = new Dictionary<string, CallbackArgs>();
                callbackss.Add(columnKey, callback);
                outlookGrid1.Tag = callbackss;
            }
        }

        private static void OutlookGrid1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            if (dgv.Columns[e.ColumnIndex] is DataGridViewLinkColumn || dgv.Columns[e.ColumnIndex] is DataGridViewButtonColumn)//如果是链接列或者按钮列被点击
            {
                string columnKey = dgv.Columns[e.ColumnIndex].Name;
                DataGridViewRow row = dgv.Rows[e.RowIndex];
                if (dgv.Columns[e.ColumnIndex].HeaderText == "确认报警")
                {
                    if (row.Cells[5].Value != null && row.Cells[5].Value.ToString() == "已确认")
                    {
                        return;
                    }
                }
                if (dgv.Columns[e.ColumnIndex].HeaderText == "处理报警")
                {
                    if (row.Cells[5].Value != null && row.Cells[5].Value.ToString() == "已处理")
                    {
                        return;
                    }
                }
                Dictionary<string, CallbackArgs> callbacks = dgv.Tag as Dictionary<string, CallbackArgs>;
                if (callbacks != null && callbacks.ContainsKey(columnKey))
                {
                    CallbackArgs callback = callbacks[columnKey];
                    if (callback != null)
                    {
                        List<object> args = new List<object>();
                        Dictionary<string, object> sysArgList = new Dictionary<string, object>();//保存本系统可能用到的所有参数类型
                        //...Start...
                        sysArgList.Add("{OWNER}", Index.Instance);
                        sysArgList.Add("{TARGET}", callback.Target);
                        sysArgList.Add("{ID}", row.Cells[0].Value);
                        DateTime now = DateTime.Now;
                        int last = 50;
                        string lastDays = ConfigurationManager.AppSettings["lastDays"];
                        int RET;
                        if (!string.IsNullOrEmpty(lastDays))
                        {
                            if (int.TryParse(lastDays, out RET))
                            {
                                last = RET;
                            }
                        }
                        DateTime start = now.AddDays(-last);
                        sysArgList.Add("{STARTTIME}", start.ToString());
                        sysArgList.Add("{ENDTIME}", now.ToString());
                        sysArgList.Add("{FLASHTIME}", row.Cells[1].Value);
                        sysArgList.Add("{DEVICENAME}", row.Cells[2].Value);
                        sysArgList.Add("{TRAINNO}", row.Cells[3].Value);
                        //...End...

                        object target;
                        Common.GetParameter(ref sysArgList, out target);
                        if (callback.Parameters != null)
                        {
                            foreach (object o in callback.Parameters)
                            {
                                if (o != null)
                                {
                                    string arg = o.ToString();
                                    if (arg.StartsWith("$") && arg.EndsWith("$"))
                                    {
                                        List<object> internalArgs = new List<object>();
                                        string formName = arg.Substring(1, arg.Length - 2);
                                        int ind = arg.IndexOf('|');//$SdgReport|{OWNER},{ID}$
                                        if (ind > 1)
                                        {
                                            formName = arg.Substring(1, ind - 1);
                                        }
                                        string para = arg.Substring(ind + 1, arg.Length - ind - 1);
                                        string[] paras = para.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                        if (paras.Length > 0)
                                        {
                                            foreach (string p in paras)
                                            {
                                                if (sysArgList.ContainsKey(p))
                                                {
                                                    internalArgs.Add(sysArgList[p]);
                                                }
                                            }
                                        }
                                        Form form = Common.GetForm(formName, target, internalArgs.ToArray());
                                        args.Add(form);
                                    }
                                    else
                                    {
                                        args.Add(o);
                                    }
                                }
                            }
                        }
                        callback.CallbackHandler.Method.Invoke(callback.Target, args.ToArray());
                    }
                }
            }
        }

        public static void RegisterLinkEvent(DataGridView outlookGrid1, string columnKey, CallbackArgs callback)
        {
            AddNewEvenHandler(outlookGrid1, columnKey, callback);
        }

        private static void AddNewEvenHandler(DataGridView outlookGrid1, string columnKey, CallbackArgs callback)
        {
            Dictionary<string, CallbackArgs> callbacks = outlookGrid1.Tag as Dictionary<string, CallbackArgs>;
            if (callbacks != null)
            {
                if (!callbacks.ContainsKey(columnKey))
                {
                    callbacks.Add(columnKey, callback);
                    //outlookGrid1.Tag = callbacks;
                }
                else
                {
                    callbacks[columnKey] = callback;
                }
            }
            else
            {
                outlookGrid1.CellContentClick += OutlookGrid1_CellContentClick;
                Dictionary<string, CallbackArgs> callbackss = new Dictionary<string, CallbackArgs>();
                callbackss.Add(columnKey, callback);
                outlookGrid1.Tag = callbackss;
            }
        }

        public static void ClearData(DataGridView outlookGrid1)
        {
            outlookGrid1.DataSource = null;
        }

        public static void BindData(DataGridView outlookGrid1, DataTable dt, ListSortDirection direction = ListSortDirection.Ascending, int sortColumnIndex = -1)
        {
            outlookGrid1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            outlookGrid1.ReadOnly = true;
            outlookGrid1.AllowUserToAddRows = false;
            outlookGrid1.DataSource = dt;
            if (sortColumnIndex > -1 && sortColumnIndex < outlookGrid1.Columns.Count)
            {
                DataGridViewColumn dgvc = outlookGrid1.Columns[sortColumnIndex];
                outlookGrid1.Sort(dgvc, direction);
            }
        }

        public static void AddLinkColumn(DataGridView outlookGrid1, string text, out string columnKey, CallbackArgs callback = null)
        {
            columnKey = "#Link#";
            DataGridViewLinkColumn link = new DataGridViewLinkColumn();
            if (outlookGrid1.Columns.Contains(columnKey))
                columnKey = "#Link#" + DateTime.Now.Ticks.ToString().Substring(4);
            link.Name = columnKey;
            //link.Text = text;
            link.HeaderText = text;
            link.UseColumnTextForLinkValue = true;
            link.TrackVisitedState = true;
            outlookGrid1.Columns.Add(link);
            if (callback != null)
                AddNewEvenHandler(outlookGrid1, columnKey, callback);
        }

        public static void AddLinkColumn(DataGridView outlookGrid1, string text, string columnKey = "#LinkSpecial#", CallbackArgs callback = null)
        {
            DataGridViewLinkColumn link = new DataGridViewLinkColumn();
            if (outlookGrid1.Columns.Contains(columnKey))
                columnKey = "#Link#" + DateTime.Now.Ticks.ToString().Substring(4);
            link.Name = columnKey;
            //link.Text = text;
            link.HeaderText = text;
            link.UseColumnTextForLinkValue = true;
            link.TrackVisitedState = true;
            outlookGrid1.Columns.Add(link);
            if (callback != null)
                AddNewEvenHandler(outlookGrid1, columnKey, callback);
        }

        public static void AddButtonColumn(DataGridView outlookGrid1, string text, out string columnKey, CallbackArgs callback = null)
        {
            columnKey = "#Button#";
            if (outlookGrid1.Columns.Contains(columnKey))
                columnKey = "#Button#" + DateTime.Now.Ticks.ToString().Substring(4);
            DataGridViewButtonColumn button = new DataGridViewButtonColumn();
            button.Name = columnKey;
            //button.Text = text;
            button.HeaderText = text;
            outlookGrid1.Columns.Add(button);
            if (callback != null)
                AddNewEvenHandler(outlookGrid1, columnKey, callback);
        }

        public static void AddButtonColumn(DataGridView outlookGrid1, string text, string columnKey = "#ButtonSpecial#", CallbackArgs callback = null)
        {
            if (outlookGrid1.Columns.Contains(columnKey))
                columnKey = "#Button#" + DateTime.Now.Ticks.ToString().Substring(4);
            DataGridViewButtonColumn button = new DataGridViewButtonColumn();
            button.Name = columnKey;
            //button.Text = text;
            button.HeaderText = text;
            outlookGrid1.Columns.Add(button);
            if (callback != null)
                AddNewEvenHandler(outlookGrid1, columnKey, callback);
        }
    }

    public class CallbackArgs
    {
        object target;
        object[] parameters;
        Delegate callback;

        public object Target
        {
            get
            {
                return target;
            }
        }
        /// <summary>
        /// 委托方法的参数列表
        /// </summary>
        public object[] Parameters
        {
            get
            {
                return parameters;
            }
        }

        public Delegate CallbackHandler
        {
            get
            {
                return callback;
            }

            set
            {
                callback = value;
            }
        }

        public CallbackArgs(Delegate callback, object target, object[] parameters)
        {
            this.callback = callback;
            this.target = target;
            this.parameters = parameters;
        }
    }
}