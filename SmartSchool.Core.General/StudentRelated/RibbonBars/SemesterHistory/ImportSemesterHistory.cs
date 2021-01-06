using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using K12.Data;
using SmartSchool.API.PlugIn;

namespace SmartSchool.StudentRelated.RibbonBars.SemesterHistory
{
    class ImportSemesterHistory : SmartSchool.API.PlugIn.Import.Importer
    {
        private List<string> ValidateKeys = new List<string>();

        public ImportSemesterHistory()
        {
            this.Image = null;
            this.Text = "匯入學期對照表";
        }

        public override void InitializeImport(SmartSchool.API.PlugIn.Import.ImportWizard wizard)
        {
            wizard.PackageLimit = 3000;
            //可匯入的欄位
            wizard.ImportableFields.AddRange("學年度", "學期", "年級", "當時科別", "當時班級", "當時座號", "當時班導師姓名", "上課天數", "課程群組代碼");
            //必需要有的欄位
            wizard.RequiredFields.AddRange("學年度", "學期", "年級");
            //開始驗證
            wizard.ValidateStart += (sender, e) => ValidateKeys.Clear();
            //驗證每行資料的事件
            wizard.ValidateRow += new System.EventHandler<SmartSchool.API.PlugIn.Import.ValidateRowEventArgs>(wizard_ValidateRow);
            //實際匯入資料的事件
            wizard.ImportPackage += new System.EventHandler<SmartSchool.API.PlugIn.Import.ImportPackageEventArgs>(wizard_ImportPackage);
            wizard.ImportComplete += (sender, e) => MessageBox.Show("匯入完成!");
        }

        void wizard_ValidateRow(object sender, SmartSchool.API.PlugIn.Import.ValidateRowEventArgs e)
        {
            #region 驗各欄位填寫格式
            int t;
            foreach (string field in e.SelectFields)
            {
                string value = e.Data[field];
                switch (field)
                {
                    default:
                        break;
                    case "學年度":
                        if (value == "" || !int.TryParse(value, out t))
                            e.ErrorFields.Add(field, "此欄為必填欄位，必須填入整數。");
                        break;
                    case "學期":
                        if (value == "" || !int.TryParse(value, out t))
                        {
                            e.ErrorFields.Add(field, "此欄為必填欄位，必須填入整數。");
                        }
                        else if (t != 1 && t != 2)
                        {
                            e.ErrorFields.Add(field, "必須填入1或2");
                        }
                        break;
                    case "年級":
                        if (value == "" || !int.TryParse(value, out t))
                        {
                            e.ErrorFields.Add(field, "此欄為必填欄位，必須填入整數。");
                        }
                        break;
                }
            }
            #endregion

            string ValidateKey = e.Data.ID + "-" + e.Data["學年度"] + "-" + e.Data["學期"];

            if (ValidateKeys.Contains(ValidateKey))
                e.ErrorMessage = "學生編號、學年及學期的組合不能重覆!";
            else
                ValidateKeys.Add(ValidateKey);
        }

        void wizard_ImportPackage(object sender, SmartSchool.API.PlugIn.Import.ImportPackageEventArgs e)
        {
            //根據學生編號、學年度、學期組成主鍵
            List<string> keyList = new List<string>();
            Dictionary<string, int> schoolYearMapping = new Dictionary<string, int>();
            Dictionary<string, int> semesterMapping = new Dictionary<string, int>();
            Dictionary<string, string> studentIDMapping = new Dictionary<string, string>();
            Dictionary<string, List<RowData>> rowsMapping = new Dictionary<string, List<RowData>>();
            //一個學生只會有一筆學期對照表
            Dictionary<string, SemesterHistoryRecord> studentSemesterHistory = new Dictionary<string, SemesterHistoryRecord>();

            //掃描每行資料，定出資料的PrimaryKey，並且將PrimaryKey對應到的資料寫成Dictionary
            foreach (RowData Row in e.Items)
            {
                int schoolYear = int.Parse(Row["學年度"]);
                int semester = int.Parse(Row["學期"]);
                string studentID = Row.ID;
                string key = schoolYear + "^_^" + semester + "^_^" + studentID;

                if (!keyList.Contains(key))
                {
                    keyList.Add(key);
                    schoolYearMapping.Add(key, schoolYear);
                    semesterMapping.Add(key, semester);
                    studentIDMapping.Add(key, studentID);
                    rowsMapping.Add(key, new List<RowData>());
                }
                rowsMapping[key].Add(Row);
            }

            //一次取得學生學期對照表            
            List<SemesterHistoryRecord> records = K12.Data.SemesterHistory.SelectByStudentIDs(studentIDMapping.Values.Distinct());

            #region 將學生現有學期對照表做快取
            foreach (SemesterHistoryRecord var in records)
                studentSemesterHistory.Add(var.RefStudentID, var);
            #endregion

            List<SemesterHistoryRecord> UpdateRecords = new List<SemesterHistoryRecord>();

            foreach (string key in keyList)
            {
                SemesterHistoryRecord record = studentSemesterHistory[studentIDMapping[key]];

                List<RowData> Rows = rowsMapping[key];

                if (record != null)
                {
                    for (int i = 0; i < Rows.Count; i++)
                    {
                        string StudentID = Rows[i].ID;
                        string SchoolYear = Rows[i]["學年度"];
                        string Semester = Rows[i]["學期"];
                        string GradeYear = Rows[i]["年級"];
                        string ClassName = Rows[i].ContainsKey("當時班級") ? Rows[i]["當時班級"] : string.Empty;
                        string SeatNo = Rows[i].ContainsKey("當時座號") ? Rows[i]["當時座號"] : string.Empty;
                        string TeacherName = Rows[i].ContainsKey("當時班導師姓名") ? Rows[i]["當時班導師姓名"] : string.Empty;
                        string SchoolDayCount = Rows[i].ContainsKey("上課天數") ? Rows[i]["上課天數"] : string.Empty;
                        string DeptName = Rows[i].ContainsKey("當時科別") ? Rows[i]["當時科別"] : string.Empty;
                        string CourseGroupCode = Rows[i].ContainsKey("課程群組代碼") ? Rows[i]["課程群組代碼"] : string.Empty;

                        List<K12.Data.SemesterHistoryItem> Items = record.SemesterHistoryItems.Where(x => x.RefStudentID == StudentID && x.SchoolYear.ToString() == SchoolYear && x.Semester.ToString() == Semester).ToList();

                        if (Items.Count > 0)
                        {
                            Items[0].GradeYear = K12.Data.Int.Parse(GradeYear);
                            if (Rows[i].ContainsKey("當時班級"))
                                Items[0].ClassName = ClassName;
                            if (Rows[i].ContainsKey("當時座號"))
                                Items[0].SeatNo = K12.Data.Int.ParseAllowNull(SeatNo);
                            if (Rows[i].ContainsKey("當時班導師姓名"))
                                Items[0].Teacher = TeacherName;
                            if (Rows[i].ContainsKey("上課天數"))
                                Items[0].SchoolDayCount = K12.Data.Int.ParseAllowNull(SchoolDayCount);
                            if (Rows[i].ContainsKey("當時科別"))
                                Items[0].DeptName = DeptName;
                            if (Rows[i].ContainsKey("課程群組代碼"))
                                Items[0].CourseGroupCode = CourseGroupCode;

                        }
                        else
                        {
                            K12.Data.SemesterHistoryItem Item = new K12.Data.SemesterHistoryItem();
                            Item.SchoolYear = K12.Data.Int.Parse(SchoolYear);
                            Item.Semester = K12.Data.Int.Parse(Semester);
                            Item.GradeYear = K12.Data.Int.Parse(GradeYear);
                            Item.ClassName = ClassName;
                            Item.SeatNo = K12.Data.Int.ParseAllowNull(SeatNo);
                            Item.Teacher = TeacherName;
                            Item.SchoolDayCount = K12.Data.Int.ParseAllowNull(SchoolDayCount);
                            Item.DeptName = DeptName;
                            Item.CourseGroupCode = CourseGroupCode;
                            record.SemesterHistoryItems.Add(Item);
                        }
                    }

                    UpdateRecords.Add(record);
                }
                else
                {
                    SemesterHistoryRecord newrecord = new SemesterHistoryRecord();

                    newrecord.RefStudentID = Rows[0].ID;

                    List<K12.Data.SemesterHistoryItem> Items = new List<K12.Data.SemesterHistoryItem>();

                    for (int i = 0; i < Rows.Count; i++)
                    {
                        string SchoolYear = Rows[i]["學年度"];
                        string Semester = Rows[i]["學期"];
                        string GradeYear = Rows[i].ContainsKey("年級") ? Rows[i]["年級"] : string.Empty;
                        string ClassName = Rows[i].ContainsKey("當時班級") ? Rows[i]["當時班級"] : string.Empty;
                        string SeatNo = Rows[i].ContainsKey("當時座號") ? Rows[i]["當時座號"] : string.Empty;
                        string TeacherName = Rows[i].ContainsKey("當時班導師姓名") ? Rows[i]["當時班導師姓名"] : string.Empty;
                        string SchoolDayCount = Rows[i].ContainsKey("上課天數") ? Rows[i]["上課天數"] : string.Empty;
                        string DeptName = Rows[i].ContainsKey("當時科別") ? Rows[i]["當時科別"] : string.Empty;
                        string CourseGroupCode = Rows[i].ContainsKey("課程群組代碼") ? Rows[i]["課程群組代碼"] : string.Empty;

                        K12.Data.SemesterHistoryItem Item = new K12.Data.SemesterHistoryItem();
                        Item.SchoolYear = K12.Data.Int.Parse(SchoolYear);
                        Item.Semester = K12.Data.Int.Parse(Semester);
                        Item.GradeYear = K12.Data.Int.Parse(GradeYear);
                        Item.ClassName = ClassName;
                        Item.SeatNo = K12.Data.Int.ParseAllowNull(SeatNo);
                        Item.Teacher = TeacherName;
                        Item.SchoolDayCount = K12.Data.Int.ParseAllowNull(SchoolDayCount);
                        Item.DeptName = DeptName;
                        Item.CourseGroupCode = CourseGroupCode;
                        Items.Add(Item);
                    }

                    newrecord.SemesterHistoryItems = Items;

                    UpdateRecords.Add(newrecord);
                }
            }

            if (UpdateRecords.Count > 0)
                K12.Data.SemesterHistory.Update(UpdateRecords);
        }
    }
}