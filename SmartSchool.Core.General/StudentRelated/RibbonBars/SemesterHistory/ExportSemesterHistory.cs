using System.Collections.Generic;
using SHSchool.Data;
using SmartSchool.API.PlugIn;

namespace SmartSchool.StudentRelated.RibbonBars.SemesterHistory
{
    class ExportSemesterHistory : SmartSchool.API.PlugIn.Export.Exporter
    {
        //建構子
        public ExportSemesterHistory()
        {
            this.Image = null;
            this.Text = "匯出學期對照表";
        }

        //覆寫
        public override void InitializeExport(SmartSchool.API.PlugIn.Export.ExportWizard wizard)
        {
            wizard.ExportableFields.AddRange("學年度", "學期", "年級","當時科別", "當時班級", "當時座號", "當時班導師姓名", "上課天數", "課程群組代碼");

            wizard.ExportPackage += (sender, e) =>
            {
                //取得學生清單

                List<SHSemesterHistoryRecord> records = SHSemesterHistory.SelectByStudentIDs(e.List);

                foreach (SHSemesterHistoryRecord record in records)
                {
                    foreach (K12.Data.SemesterHistoryItem Item in record.SemesterHistoryItems)
                    {
                        RowData row = new RowData();

                        row.ID = record.RefStudentID;

                        foreach (string field in e.ExportFields)
                        {
                            if (wizard.ExportableFields.Contains(field))
                            {
                                switch (field)
                                {
                                    case "學年度": row.Add(field, "" + Item.SchoolYear); break;
                                    case "學期": row.Add(field, "" + Item.Semester); break;
                                    case "年級": row.Add(field, "" + Item.GradeYear); break;
                                    case "當時科別": row.Add(field, ""+Item.DeptName ); break;
                                    case "當時班級": row.Add(field, "" + Item.ClassName); break;
                                    case "當時座號": row.Add(field, "" + Item.SeatNo); break;
                                    case "當時班導師姓名": row.Add(field, "" + Item.Teacher); break;
                                    case "上課天數": row.Add(field, "" + Item.SchoolDayCount); break;
                                    case "課程群組代碼": row.Add(field, "" + Item.CourseGroupCode); break;
                                }
                            }
                        }
                        e.Items.Add(row);
                    }
                }
            };
        }
    }
}