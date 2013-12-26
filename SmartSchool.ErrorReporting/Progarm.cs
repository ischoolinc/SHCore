using System;
using System.Collections.Generic;
using System.Text;
using FISCA;
using System.Windows.Forms;
using System.Threading;

namespace SmartSchool.ErrorReporting
{
    public static class Progarm
    {
        [MainMethod]
        public static void Main()
        {
            Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e)
            {
                if (!IsThreadAbortException(e.Exception))
                {
                    ReportingService.ReportException(e.Exception);
                    new SorrySorryForm().ShowDialog();
                    Application.Exit();
                }
            };
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                if (!IsThreadAbortException((Exception)e.ExceptionObject))
                {
                    ReportingService.ReportException((Exception)e.ExceptionObject);
                    new SorrySorryForm().ShowDialog();
                    Application.Exit();
                }
            };
        }
        private static bool IsThreadAbortException(Exception exception)
        {
            if (exception == null)
                return false;
            if (exception is ThreadAbortException)
                return true;
            else
                return IsThreadAbortException(exception.InnerException);
        }
    }
}
