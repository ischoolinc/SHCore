using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using FISCA.DSAUtil;
using SmartSchool.ExceptionHandler;
using SmartSchool.Feature.ApplicationLog;
using FISCA.LogAgent;

namespace SmartSchool.ApplicationLog
{
    internal class LogSender
    {
        private LogStorageQueue _source;
        private BackgroundWorker _send_workder;

        public LogSender(LogStorageQueue source)
        {
            _source = source;
            _send_workder = new BackgroundWorker();

            _send_workder.DoWork += new DoWorkEventHandler(SendWorkder_DoWork);
            _send_workder.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SendWorkder_RunWorkerCompleted);
            Application.Idle += new EventHandler(Application_Idle);
        }

        public void Send()
        {
            try
            {
                CommitLog();
            }
            catch (Exception ex)
            {
                CurrentUser user = CurrentUser.Instance;
                BugReporter.ReportException(user.SystemName, user.SystemVersion, ex, false);
            }
        }

        private void SendWorkder_DoWork(object sender, DoWorkEventArgs e)
        {
            CommitLog();
        }

        private void SendWorkder_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                CurrentUser user = CurrentUser.Instance;
                BugReporter.ReportException(user.SystemName, user.SystemVersion, e.Error, false);
            }
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            try
            {
                if (_source.Count <= 0) return;

                if (!_send_workder.IsBusy)
                    _send_workder.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                CurrentUser user = CurrentUser.Instance;
                BugReporter.ReportException(user.SystemName, user.SystemVersion, ex, false);
            }
        }

        private void CommitLog()
        {
            List<ILogInformation> logs = new List<ILogInformation>();

            lock (_source.SyncRoot)
            {
                int readcount = 5;

                for (int i = 1; i <= readcount; i++)
                {
                    if (_source.Count > 0)
                        logs.Add(_source.Dequeue());
                    else
                        break;
                }
            }

            if (logs.Count > 0) UploadLog(logs);

            if (_source.Count > 0) CommitLog();
        }

        private void UploadLog(List<ILogInformation> logs)
        {
            DSXmlHelper request = new DSXmlHelper("InsertRequest");

            LogSaver sLogSaver = FISCA.LogAgent.ApplicationLog.CreateLogSaverInstance();

            foreach (ILogInformation each in logs)
            {
                //request.AddElement(".", each.Content.BaseElement);
                sLogSaver
                    .AddBatch("系統歷程",each.Content.Action,"",each.Content.EntityID,each.Content.Description);
            }

            if (CurrentUser.Instance.IsDebugMode)
            {
                LogToLocal(request);
            }
            else
            {
                //Manage.InsertLog(request.BaseElement);
                sLogSaver.LogBatch(false);
            }
        }

        private void LogToLocal(DSXmlHelper request)
        {
            string file = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss fffffff");

            File.WriteAllText(file + ".xml", request.GetRawXml());
        }
    }
}
