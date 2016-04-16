using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Web;

namespace StationSoftware
{
    /// <summary>
    /// 错误级别
    /// </summary>
    public enum ErrorLevel : byte
    {
        登录 = 0,
        信号 = 1,
        错误 = 2
    }
    public static class Logs
    {
        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="err_source"></param>
        /// <param name="err_msg"></param>
        /// <returns></returns>
        public static bool LogError(string err_source, string err_msg)
        {
            ErrorLevel level = ErrorLevel.错误;
            string ip = Common.GetIPv4().ToString();
            string mac = Common.GetMac();
            string client = ip + "(" + mac + ")";
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                try
                {
                    int r = sqlHelper.ExecuteNonQuery("insert into d_error_log (err_level, err_source, err_msg, err_client) values (" + level + ", '" + err_source + "', '" + err_msg + "', '" + client + "')");
                    return r > 0;
                }
                catch (Exception e)
                {
                    
                }
            }
            return false;
        }
        /// <summary>
        /// 登录日志
        /// </summary>
        /// <returns></returns>
        public static bool LogLogin()
        {
            ErrorLevel level = ErrorLevel.登录;
            string ip = Common.GetIPv4().ToString();
            string mac = Common.GetMac();
            string client = ip + "(" + mac + ")";
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                try
                {
                    int r = sqlHelper.ExecuteNonQuery("insert into d_error_log (err_level, err_source, err_msg, err_client) values (" + (byte)level + ", 'StationSoftware', '登录', '" + client + "')");
                    return r > 0;
                }
                catch (Exception e)
                {

                }
            }
            return false;
        }
        /// <summary>
        /// 信号日志
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public static bool LogSignal(string signal)
        {
            ErrorLevel level = ErrorLevel.信号;
            string ip = Common.GetIPv4().ToString();
            string mac = Common.GetMac();
            string client = ip + "(" + mac + ")";
            using (SqlHelper sqlHelper = new SqlHelper())
            {
                try
                {
                    int r = sqlHelper.ExecuteNonQuery("insert into d_error_log (err_level, err_source, err_msg, err_client) values (" + (byte)level + ", 'StationSoftware', '信号:" + signal + "', '" + client + "')");
                    return r > 0;
                }
                catch (Exception e)
                {

                }
            }
            return false;
        }
    }
}