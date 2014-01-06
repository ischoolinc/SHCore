using System;
using System.Collections.Generic;
using System.Text;
using FISCA.DSAUtil;
//using FISCA.UDT;

namespace SmartSchool.ErrorReporting
{
    [FISCA.Authentication.AutoRetryOnWebException]
    public static class ReportingService
    {
        //private static DSConnection _Connection = null;
        //private static DSConnection Connection
        //{
        //    get
        //    {
                //if (_Connection == null)
                //{
                //    _Connection = new DSConnection("diagnostics", "anonymous", "1234");
                //    _Connection.Connect();
                //}
        //        //return _Connection;
        //    }
        //}
        public static bool ReportException(ErrorMessgae msg)
        {
            try
            {
             //   msg.DSConnection = Connection;                
                msg.Save();
                return true;
            }
            catch (Exception e)
            {
                try
                {
                    new ErrorMessgae(new Exception("建置錯誤訊息時發生錯誤。", e)) { 
                        }.Save();
                }
                catch { }
                return false;
            }
        }
        public static bool ReportException(Exception ex)
        {
            try
            {
                ErrorMessgae msg = new ErrorMessgae(ex);
//                msg.DSConnection = Connection;
                msg.GetApplicationSnapShot();
                msg.GetDeploySources();
                msg.Save();
                
                return true;
            }
            catch (Exception e)
            {
                try
                {
                    ErrorMessgae msg = new ErrorMessgae(new Exception("建置錯誤訊息時發生錯誤。", e));
//                    msg.DSConnection = Connection;
                   // msg.Save();
                    msg.Save();
                }
                catch { }
                return false;
            }
        }
    }
}
