using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;
using SmartSchool.CourseRelated;
using FISCA.DSAUtil;
using SmartSchool;
using System.Threading;
using FISCA.Presentation;
using FISCA.Data;

namespace SmartSchool.CourseRelated.RibbonBars.ScoresCalc.Forms
{
            

    public partial class CalculateWizard : BaseForm, IProgressUI
    {
        private BackgroundWorker _calc_worker, _export_worker;
        private CourseDataLoader _raw_data;

        private Boolean Is_AbsentEqualZero { get; set; }

    
        public bool _Is_AbsentEqualZero
        {
            get { return Is_AbsentEqualZero; }
        }

        public CalculateWizard()
        {
            InitializeComponent();

            InitializeBackgroundWorker();

            lblCourseCount.Text = ""; // 2018/6/5 穎驊修改，根據佳樺的反應優化項目，[H成績][01]計算課程成績，功能選取後畫面上顯示的資料非選取的課程資訊。 這邊一開始先不帶資料。
            btnExport.Visible = false; // 這個匯出功能一開始也不先給使用者按，因為在計算完之前按，會當掉

            lblTitle.Font = new Font(FontStyles.GeneralFontFamily, 20);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            _raw_data = new CourseDataLoader();
            _raw_data.LoadCalculationData(this);

            // 取得缺考設定
            Dictionary<string, string> UseTextScoreTypeDict = GetExamUseTextScoreType();

            CourseScoreCalculate calculate = new CourseScoreCalculate(_raw_data.Courses);
            //  原本畫面設定讓使用者自行決定缺考是否0分
            //calculate.Calculate(Is_AbsentEqualZero);

            // 使用缺考設定處理
            calculate.Calculate(UseTextScoreTypeDict);
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (_raw_data == null) return;

            if (e.UserState != null)
                lblCourseCount.Text = string.Format("<font color=\"#0A11FF\">建立課程總覽資料，請稍後...(進度：{0}/4)\n", _raw_data.CurrentStep) + e.UserState.ToString() + "</font>";
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Cancellation) return;

            if (e.Error == null)
            {
                btnExport.Visible = true; // 給按了。
                btnExport.Enabled = true;
                btnCalcuate.Enabled = true;

                CourseSummaryCalculate summary = new CourseSummaryCalculate(_raw_data.Courses);
                summary.Calculate();
                lblCourseCount.Text = summary.Message();
            }
            else
                MsgBox.Show("取得課程成績相關資料錯誤。", Application.ProductName);

            CalculateResult result = new CalculateResult(_raw_data.Courses);
            result.ShowDialog();

        }

        private void ExportWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            MotherForm.SetStatusBarMessage("儲存檔案中…", 0);
            LackScoreExporter exporter = new LackScoreExporter(_raw_data.Courses, new BarMessageProgress(this));
            exporter.Export();
        }

        private void ExportWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                MsgBox.Show(e.Error.Message, Application.ProductName);

            btnExport.Enabled = true;
            MotherForm.SetStatusBarMessage("儲存完畢");
        }

        private void CalculateWizard_Load(object sender, EventArgs e)
        {
            lblCourseCount.Text = "建立課程總覽資料，請稍後...";
            btnExport.Enabled = false;
            btnCalcuate.Enabled = false;
            _calc_worker.RunWorkerAsync();
        }

        private void btnCalcuate_Click(object sender, EventArgs e)
        {

            lblCourseCount.Text = "建立課程總覽資料，請稍後...";
            btnExport.Enabled = false;
            btnCalcuate.Enabled = false;
            _calc_worker.RunWorkerAsync();
                                    
            //CalculateResult result = new CalculateResult(_raw_data.Courses);
            //result.ShowDialog();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {            
            _export_worker.RunWorkerAsync();
            btnExport.Enabled = false;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Cancel();
        }

        #region IProgressUI 成員

        public void ReportProgress(string message, int progress)
        {
            _calc_worker.ReportProgress(progress, message);
        }

        public void Cancel()
        {
            _calc_worker.CancelAsync();
        }

        public bool Cancellation
        {
            get { return _calc_worker.CancellationPending; }
        }

        #endregion

        private void InitializeBackgroundWorker()
        {
            _calc_worker = new BackgroundWorker();
            _calc_worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
            _calc_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
            _calc_worker.ProgressChanged += new ProgressChangedEventHandler(Worker_ProgressChanged);
            _calc_worker.WorkerReportsProgress = true;
            _calc_worker.WorkerSupportsCancellation = true;

            _export_worker = new BackgroundWorker();
            _export_worker.DoWork += new DoWorkEventHandler(ExportWorker_DoWork);
            _export_worker.WorkerReportsProgress = true;
            _export_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ExportWorker_RunWorkerCompleted);
        }

        class BarMessageProgress : IProgressUI
        {
            private Form _frm;

            public BarMessageProgress(Form frm)
            {
                _frm = frm;
            }

            #region IProgressUI 成員

            private delegate void RaiseProgress(string message, int progress);

            public void ReportProgress(string message, int progress)
            {
                if (!string.IsNullOrEmpty(message))
                    MotherForm.SetStatusBarMessage(message);
                else
                {
                    if (_frm.InvokeRequired)
                        _frm.Invoke(new RaiseProgress(ReportProgress), message, progress);
                    else
                    {
                        MotherForm.SetStatusBarMessage(message, progress);
                        Application.DoEvents();
                    }
                }
            }

            public void Cancel()
            {
            }

            public bool Cancellation
            {
                get { return false; }
            }

            #endregion
        }


        private void AbsentEqualZero_CheckedChanged(object sender, EventArgs e)
        {
            if (AbsentEqualZero.Checked)
            {
                Is_AbsentEqualZero = true;
            }
            else
            {
                Is_AbsentEqualZero = false;
            }

        }
    
        // 取得缺考設定
        private Dictionary<string,string> GetExamUseTextScoreType()
        {
            Dictionary<string, string> value = new Dictionary<string, string>();
            QueryHelper qh = new QueryHelper();

            string sql = string.Format(@"
            SELECT
                array_to_string(xpath('//UseText/text()', settings), '') AS UseText,
                array_to_string(xpath('//ScoreType/text()', settings), '') AS ScoreType,
                array_to_string(xpath('//ReportValue/text()', settings), '') AS ReportValue,
                array_to_string(xpath('//UseValue/text()', settings), '') AS UseValue
            FROM
                (
                    SELECT
                        unnest(
                            xpath(
                                '//Configurations/Configuration/Settings/Setting',
                                xmlparse(
                                    content REPLACE(REPLACE(content, '&lt;', '<'), '&gt;', '>')
                                )
                            )
                        ) AS settings
                    FROM
                        list
                    WHERE
                        name = '評量成績缺考設定'
                ) AS content
            ");

            DataTable dt = qh.Select(sql);
            foreach(DataRow dr in dt.Rows)
            {
                string text = dr["usetext"] + "";
                string type = dr["scoretype"] + "";

                if (!value.ContainsKey(text))
                    value.Add(text, type);
            }
            return value;
        }
    }
}