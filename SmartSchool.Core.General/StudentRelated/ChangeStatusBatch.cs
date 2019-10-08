using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.Customization.PlugIn;
using SmartSchool.Customization.Data;
using SmartSchool.ApplicationLog;
using SmartSchool.Common;
using System.Windows.Forms;
using System.ComponentModel;
using SmartSchool.ExceptionHandler;

namespace SmartSchool.StudentRelated
{
    internal class ChangeStatusBatch
    {
        private static BackgroundWorker _BKW = new BackgroundWorker();
        private static Dictionary<string, List<string>> _CheckIDNumber = new Dictionary<string, List<string>>();
        private static Dictionary<string, List<string>> _CheckStudNumber = new Dictionary<string, List<string>>();
        private static bool _checkDataError = false;

        static ChangeStatusBatch()
        {

            _BKW.DoWork += new DoWorkEventHandler(_BKW_DoWork);
            _BKW.ProgressChanged += new ProgressChangedEventHandler(_BKW_ProgressChanged);
            _BKW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BKW_RunWorkerCompleted);
            _BKW.WorkerReportsProgress = true;
        }

        public static void Init()
        {
            ButtonAdapter btn一般 = new ButtonAdapter();
            btn一般.Path = "變更學生狀態";
            btn一般.Text = "一般";
            btn一般.OnClick += new EventHandler(btn一般_OnClick);
            Customization.PlugIn.ContextMenu.StudentMenuButton.AddItem(btn一般);

            ButtonAdapter btn畢業或離校 = new ButtonAdapter();
            btn畢業或離校.Path = "變更學生狀態";
            btn畢業或離校.Text = "畢業或離校";
            btn畢業或離校.OnClick += new EventHandler(btn畢業或離校_OnClick);
            Customization.PlugIn.ContextMenu.StudentMenuButton.AddItem(btn畢業或離校);

            ButtonAdapter btn休學 = new ButtonAdapter();
            btn休學.Path = "變更學生狀態";
            btn休學.Text = "休學";
            btn休學.OnClick += new EventHandler(btn休學_OnClick);
            Customization.PlugIn.ContextMenu.StudentMenuButton.AddItem(btn休學);

            ButtonAdapter btn延修 = new ButtonAdapter();
            btn延修.Path = "變更學生狀態";
            btn延修.Text = "延修";
            btn延修.OnClick += new EventHandler(btn延修_OnClick);
            Customization.PlugIn.ContextMenu.StudentMenuButton.AddItem(btn延修);

            ButtonAdapter btn刪除 = new ButtonAdapter();
            btn刪除.Path = "變更學生狀態";
            btn刪除.Text = "刪除";
            btn刪除.OnClick += new EventHandler(btn刪除_OnClick);
            Customization.PlugIn.ContextMenu.StudentMenuButton.AddItem(btn刪除);

        }

        private static void btn刪除_OnClick(object sender, EventArgs e)
        {
            if (_BKW.IsBusy)
            {
                MsgBox.Show("系統正在變更學生狀態，\n請等待目前作業完成後，\n再次執行此動作。");
                return;
            }
            DialogResult dr = MsgBox.Show("是否變更學生狀態為\"刪除\"？", Application.ProductName, MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
                SetStatus("刪除");
        }

        static void btn一般_OnClick(object sender, EventArgs e)
        {
            if (_BKW.IsBusy)
            {
                MsgBox.Show("系統正在變更學生狀態，\n請等待目前作業完成後，\n再次執行此動作。");
                return;
            }
            DialogResult dr = MsgBox.Show("是否變更學生狀態為\"一般\"？", Application.ProductName, MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
                SetStatus("一般");
        }

        static void btn畢業或離校_OnClick(object sender, EventArgs e)
        {
            if (_BKW.IsBusy)
            {
                MsgBox.Show("系統正在變更學生狀態，\n請等待目前作業完成後，\n再次執行此動作。");
                return;
            }
            DialogResult dr = MsgBox.Show("是否變更學生狀態為\"畢業或離校\"？\n您將無法從\"在校學生\"中找到這些學生。\n", Application.ProductName, MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
                SetStatus("畢業或離校");
        }

        static void btn休學_OnClick(object sender, EventArgs e)
        {
            if (_BKW.IsBusy)
            {
                MsgBox.Show("系統正在變更學生狀態，\n請等待目前作業完成後，\n再次執行此動作。");
                return;
            }
            DialogResult dr = MsgBox.Show("是否變更學生狀態為\"休學\"？\n您將無法從\"在校學生\"中找到這些學生。\n", Application.ProductName, MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
                SetStatus("休學");
        }

        static void btn延修_OnClick(object sender, EventArgs e)
        {
            if (_BKW.IsBusy)
            {
                MsgBox.Show("系統正在變更學生狀態，\n請等待目前作業完成後，\n再次執行此動作。");
                return;
            }
            DialogResult dr = MsgBox.Show("是否變更學生狀態為\"延修\"？", Application.ProductName, MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
                SetStatus("延修");
        }

        static void SetStatus(string newStatus)
        {
            _BKW.RunWorkerAsync(newStatus);
        }

        static void _BKW_DoWork(object sender, DoWorkEventArgs e)
        {
            string newStatus = "" + e.Argument;

            _checkDataError = false;
            _CheckIDNumber.Clear();
            _CheckStudNumber.Clear();
            // 取得檢查資料,所有學生建立 Dict
            foreach (BriefStudentData stud in StudentRelated.Student.Instance.Items)
            {
                // 學號
                if (!string.IsNullOrEmpty(stud.StudentNumber))
                {
                    if (_CheckStudNumber.ContainsKey(stud.Status))
                        _CheckStudNumber[stud.Status].Add(stud.StudentNumber);
                    else
                    {
                        List<string> data = new List<string>();
                        data.Add(stud.StudentNumber);
                        _CheckStudNumber.Add(stud.Status, data);
                    }
                }

                // 身分證號
                if (!string.IsNullOrEmpty(stud.IDNumber))
                {
                    if (_CheckIDNumber.ContainsKey(stud.Status))
                        _CheckIDNumber[stud.Status].Add(stud.IDNumber.ToUpper());
                    else
                    {
                        List<string> data = new List<string>();
                        data.Add(stud.IDNumber.ToUpper());
                        _CheckIDNumber.Add(stud.Status, data);
                    }
                }
            }

            List<string> errorMsg = new List<string>();
            // 驗證資料
            foreach (BriefStudentData student in StudentRelated.Student.Instance.SelectionStudents)
            {
                // 狀態相同
                if (student.Status == newStatus)
                    continue;

                // 驗身分證號
                if (_CheckIDNumber.ContainsKey(newStatus))
                    if (_CheckIDNumber[newStatus].Contains(student.IDNumber.ToUpper()))
                        errorMsg.Add("身分證號:" + student.IDNumber + "在" + newStatus + "有相同身分證號無法變更");

                // 驗學號
                if (_CheckStudNumber.ContainsKey(newStatus))
                    if (_CheckStudNumber[newStatus].Contains(student.StudentNumber))
                        errorMsg.Add("學號:" + student.StudentNumber + "在" + newStatus + "有相同學號無法變更");
            }

            // 有錯誤時停止並回報
            if (errorMsg.Count > 0)
            {
                e.Result = errorMsg;
                _checkDataError = true;
            }
            if (errorMsg.Count == 0)
            {
                List<string> idlist = new List<string>();
                e.Result = idlist;

                decimal totle = StudentRelated.Student.Instance.SelectionStudents.Count;
                if (totle == 0)
                    totle = 1;
                decimal current = 0;
                _BKW.ReportProgress((int)(current * 100m / totle));
                Dictionary<string, string> idLog = new Dictionary<string, string>();


                foreach (BriefStudentData student in StudentRelated.Student.Instance.SelectionStudents)
                {
                    current++;
                    if (student.Status != newStatus)
                    {
                        idLog.Add(student.ID, string.Format("學生：{0}{1} \n狀態由「{2}」變更為「{3}」", student.StudentNumber.Length > 0 ? "(" + student.StudentNumber + ")" : "", student.Name, student.Status, newStatus));
                        if (idLog.Count > 100)
                        {
                            _BKW.ReportProgress((int)(current * 100m / totle));
                            Feature.EditStudent.ChangeStudentStatus(newStatus, new List<string>(idLog.Keys).ToArray());
                            idlist.AddRange(idLog.Keys);
                            #region 修改學生狀態 Log
                            foreach (string id in idLog.Keys)
                            {
                                CurrentUser.Instance.AppLog.Write(EntityType.Student, "變更狀態", id, idLog[id], "學生", "");
                            }
                            #endregion
                            idLog.Clear();
                        }
                    }
                }
                if (idLog.Count > 0)
                {
                    _BKW.ReportProgress((int)(current * 100m / totle));
                    Feature.EditStudent.ChangeStudentStatus(newStatus, new List<string>(idLog.Keys).ToArray());
                    idlist.AddRange(idLog.Keys);
                    #region 修改學生狀態 Log
                    foreach (string id in idLog.Keys)
                    {
                        CurrentUser.Instance.AppLog.Write(EntityType.Student, "變更狀態", id, idLog[id], "學生", "");
                    }
                    #endregion
                    idLog.Clear();
                }
            }
        }

        static void _BKW_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Customization.PlugIn.Global.SetStatusBarMessage("變更學生狀態...", e.ProgressPercentage);
        }

        static void _BKW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // 當檢查有錯誤
            if (_checkDataError)
            {
                List<string> errorMsg = (List<string>)e.Result;
                if (errorMsg.Count > 0)
                {
                    FISCA.Presentation.Controls.MsgBox.Show(string.Join(",", errorMsg.ToArray()));
                    return;
                }
            }

            if (e.Error != null)
            {
                MsgBox.Show("變更狀態發生錯誤，可能有部分學生狀態未修改。");
                BugReporter.ReportException("SmartSchool", CurrentUser.Instance.SystemVersion, e.Error, false);
            }
            List<string> idlist = (List<string>)e.Result;
            if (idlist.Count > 0)
            {
                //Student.Instance.InvokBriefDataChanged(idlist.ToArray());
                FISCA.Presentation.MotherForm.SetStatusBarMessage("變更學生狀態成功");
                SmartSchool.Broadcaster.Events.Items["學生/資料變更"].Invoke(idlist.ToArray());
            }
        }
    }
}
