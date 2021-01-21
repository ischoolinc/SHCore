using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Feature.Course;
using FISCA.DSAUtil;
using System.Xml;
using FISCA.Data;
using System.Data;

namespace SmartSchool.CourseRelated.ScoreDataGridView
{
    interface IDataProvider
    {
        ColumnHeader ColumnHeader { get; }
        RowCollection Rows { get; }
        ICell FindCell(string studentid, string columnName);
    }

    class TestDataProvider : IDataProvider
    {
        private ColumnHeader _columnHeader = null;
        private RowCollection _rowCollection = null;

        //public TestDataProvider()
        //{
        //    #region 塞資料
        //    _columnHeader = new ColumnHeader();
        //    _columnHeader.Columns.Add(new ColumnSetting("1","班級", 70));
        //    _columnHeader.Columns.Add(new ColumnSetting("1", "座號", 65));
        //    _columnHeader.Columns.Add(new ColumnSetting("1", "姓名", 70));
        //    _columnHeader.Columns.Add(new ColumnSetting("1", "學號", 70));
        //    _columnHeader.Columns.Add(new ColumnSetting("1", "成績1", 80));
        //    _columnHeader.Columns.Add(new ColumnSetting("1", "成績2", 80));

        //    _rowCollection = new RowCollection();

        //    RowEntity row = new RowEntity("1");            
        //    row.AddCell("班級", new StudentCell("資一忠"));
        //    row.AddCell("座號", new StudentCell("1"));
        //    row.AddCell("姓名", new StudentCell("路西華"));
        //    row.AddCell("學號", new StudentCell("84203226"));        
        //    row.AddCell("成績1", new ExamCell("100"));   
        //    row.AddCell("成績2", new ExamCell("100"));
        //    _rowCollection.Add("1", row);

        //    row = new RowEntity("2");
        //    row.AddCell("班級", new StudentCell("資一忠"));
        //    row.AddCell("座號", new StudentCell("2"));
        //    row.AddCell("姓名", new StudentCell("有紅痔"));
        //    row.AddCell("學號", new StudentCell("8825252"));
        //    row.AddCell("成績1", new ExamCell("90"));
        //    row.AddCell("成績2", new ExamCell("90"));
        //    _rowCollection.Add("2", row);

        //    row = new RowEntity("3");
        //    row.AddCell("班級", new StudentCell("資一忠"));
        //    row.AddCell("座號", new StudentCell("3"));
        //    row.AddCell("姓名", new StudentCell("君士坦丁"));
        //    row.AddCell("學號", new StudentCell("08009786"));
        //    row.AddCell("成績1", new ExamCell("30"));
        //    row.AddCell("成績2", new ExamCell("50"));
        //    _rowCollection.Add("3", row);

        //    row = new RowEntity("4");
        //    row.AddCell("班級", new StudentCell("資一忠"));
        //    row.AddCell("座號", new StudentCell("4"));
        //    row.AddCell("姓名", new StudentCell("赫丘力"));
        //    row.AddCell("學號", new StudentCell("08000800"));
        //    row.AddCell("成績1", new ExamCell("10"));
        //    row.AddCell("成績2", new ExamCell("10"));
        //    _rowCollection.Add("4", row);

        //    row = new RowEntity("5");
        //    row.AddCell("班級", new StudentCell("資一忠"));
        //    row.AddCell("座號", new StudentCell("5"));
        //    row.AddCell("姓名", new StudentCell("路西華"));
        //    row.AddCell("學號", new StudentCell("88880000"));
        //    row.AddCell("成績1", new ExamCell("30"));
        //    row.AddCell("成績2", new ExamCell("80"));
        //    _rowCollection.Add("5", row);

        //    #endregion
        //}

        #region IDataProvider 成員

        public ColumnHeader ColumnHeader
        {
            get { return _columnHeader; }
        }

        public RowCollection Rows
        {
            get { return _rowCollection; }
        }

        public ICell FindCell(string studentid, string columnName)
        {
            return _rowCollection.FindCell(studentid, columnName);
        }

        #endregion
    }

    class SmartSchoolDataProvider : IDataProvider
    {
        private string _courseid;
        private ColumnHeader _columnHeader;
        private RowCollection _rowCollection;

        public SmartSchoolDataProvider(string courseid)
        {
            _courseid = courseid;
            //DSResponse dsrsp = QueryCourse.GetCourseExam(courseid);
            //DSXmlHelper helper = dsrsp.GetContent();
            QueryHelper cousreExamQH = new QueryHelper();
            Dictionary<string, string> examDict = new Dictionary<string, string>();

            // 取得課程評分樣板試別
            string qryCousreExam = "SELECT " +
                "exam.id AS exam_id" +
                ",exam.exam_name " +
                "FROM " +
                "course INNER JOIN te_include " +
                "ON course.ref_exam_template_id = te_include.ref_exam_template_id " +
                "INNER JOIN exam " +
                "ON te_include.ref_exam_id = exam.id " +
                "WHERE course.id = " + courseid + " ORDER BY display_order";

            DataTable dtCousreExam = cousreExamQH.Select(qryCousreExam);
            foreach (DataRow dr in dtCousreExam.Rows)
            {
                string exam_id = dr["exam_id"].ToString();
                string exam_name = dr["exam_name"].ToString();
                if (!examDict.ContainsKey(exam_id))
                    examDict.Add(exam_id, exam_name);
            }


            // 塞 Header
            _columnHeader = new ColumnHeader();
            _columnHeader.Columns.Add(new ColumnSetting("-1", "學生系統編號", 0));
            _columnHeader.Columns.Add(new ColumnSetting("-1", "班級", 60));
            _columnHeader.Columns.Add(new ColumnSetting("-1", "座號", 40, new DecimalComparer()));
            _columnHeader.Columns.Add(new ColumnSetting("-1", "姓名", 70));
            _columnHeader.Columns.Add(new ColumnSetting("-1", "學號", 70));

            //foreach (XmlNode node in helper.GetElements("Course"))
            //{
            //    string examName = node.SelectSingleNode("ExamName").InnerText;
            //    string examid = node.SelectSingleNode("RefExamID").InnerText;

            //    _columnHeader.Columns.Add(new ColumnSetting(examid, examName, 80, new DecimalComparer()));
            //    examList.Add(examid, examName);
            //}

            foreach (string examid in examDict.Keys)
                _columnHeader.Columns.Add(new ColumnSetting(examid, examDict[examid], 80, new DecimalComparer()));

            _columnHeader.Columns.Add(new ColumnSetting("-1", "課程成績", 100, new DecimalComparer()));

            _columnHeader.Columns.Add(new ColumnSetting("-1", "及格標準", 80, new DecimalComparer()));
            _columnHeader.Columns.Add(new ColumnSetting("-1", "補考標準", 80, new DecimalComparer()));
            _columnHeader.Columns.Add(new ColumnSetting("-1", "直接指定總成績", 120, new DecimalComparer()));
            _columnHeader.Columns.Add(new ColumnSetting("-1", "備註", 200));


            // 舊寫法
            // 取回本課程所有學生的考試成績
            //DSResponse scoreResp = QueryCourse.GetSECTake(courseid);
            //XmlElement scoreElement = scoreResp.GetContent().GetElement(".");


            // 塞 Row
            _rowCollection = new RowCollection();
            //dsrsp = QueryCourse.GetSCAttend(courseid);
            //helper = dsrsp.GetContent();

            // 取得學生修課紀錄
            QueryHelper qhScattend = new QueryHelper();
            string qryScattend = "SELECT " +
                "sc_attend.id AS sc_attend_id" +
                ",ref_student_id AS student_id" +
                ",sc_attend.passing_standard" +
                ",sc_attend.makeup_standard" +
                ",sc_attend.remark" +
                ",sc_attend.designate_final_score" +
                ",sc_attend.score " +
                "FROM " +
                "sc_attend " +
                "WHERE ref_course_id = " + courseid + ";";

            DataTable dtScattend = qhScattend.Select(qryScattend);

            Dictionary<string, DataRow> dtScattendDict = new Dictionary<string, DataRow>();
            foreach (DataRow dr in dtScattend.Rows)
            {
                string sc_id = dr["sc_attend_id"].ToString();
                if (!dtScattendDict.ContainsKey(sc_id))
                {
                    dtScattendDict.Add(sc_id, dr);
                }
            }

            // 取得班級修課學生
            QueryHelper qhCourseStudent = new QueryHelper();
            string qryCourseStudent = "SELECT " +
                "sc_attend.id AS sc_attend_id" +
                ",student.id AS student_id" +
                ",class.class_name" +
                ",student.seat_no" +
                ",student.student_number" +
                ",student.name AS student_name  " +
                "FROM " +
                "sc_attend INNER JOIN student " +
                "ON sc_attend.ref_student_id = student.id " +
                "LEFT JOIN class " +
                "ON student.ref_class_id = class.id " +
                "WHERE ref_course_id = " + courseid + " " +
                "ORDER BY class_name,seat_no,student_number";

            DataTable dtCourseStudent = qhCourseStudent.Select(qryCourseStudent);

            // 取得修課學生段考成績
            QueryHelper qhSCETakeScore = new QueryHelper();
            string qrySCETakeScore = "SELECT " +
                "sce_take.id" +
                ",ref_exam_id" +
                ",ref_sc_attend_id" +
                ",sce_take.score " +
                "FROM " +
                "sce_take INNER JOIN " +
                "sc_attend " +
                "ON sce_take.ref_sc_attend_id = sc_attend.id " +
                "WHERE sc_attend.ref_course_id = " + courseid + ";";
            DataTable dtSCETakeScore = qhSCETakeScore.Select(qrySCETakeScore);
            Dictionary<string, DataRow> sceScoreDict = new Dictionary<string, DataRow>();
            foreach (DataRow dr in dtSCETakeScore.Rows)
            {
                string key = dr["ref_exam_id"].ToString() + "_" + dr["ref_sc_attend_id"].ToString();
                if (!sceScoreDict.ContainsKey(key))
                    sceScoreDict.Add(key, dr);
            }

            foreach (DataRow dr in dtCourseStudent.Rows)
            {
                string sc_attend_id = dr["sc_attend_id"].ToString();
                RowEntity row = new RowEntity(sc_attend_id);
                row.AddCell("學生系統編號", new StudentCell(dr["student_id"].ToString()));
                row.AddCell("班級", new StudentCell(dr["class_name"].ToString()));
                row.AddCell("座號", new StudentCell(dr["seat_no"].ToString()));
                row.AddCell("姓名", new StudentCell(dr["student_name"].ToString()));
                row.AddCell("學號", new StudentCell(dr["student_number"].ToString()));

                foreach (string examid in examDict.Keys)
                {
                    string key = examid + "_" + sc_attend_id;
                    ScoreInfo score = new ScoreInfo();
                    if (sceScoreDict.ContainsKey(key))
                    {
                        score = GetScore(sceScoreDict[key]);
                    }

                    if (score.Score == "-1")
                        score.Score = "缺";
                    row.AddCell(examDict[examid], new ExamCell(score));
                }

                if (dtScattendDict.ContainsKey(sc_attend_id))
                {
                    row.AddCell("課程成績", new ScoreExamCell(new ScoreInfo(sc_attend_id, dtScattendDict[sc_attend_id]["score"].ToString())));

                    row.AddCell("及格標準", new ScoreExamCell(new ScoreInfo(sc_attend_id, dtScattendDict[sc_attend_id]["passing_standard"].ToString())));
                    row.AddCell("補考標準", new ScoreExamCell(new ScoreInfo(sc_attend_id, dtScattendDict[sc_attend_id]["makeup_standard"].ToString())));
                    row.AddCell("直接指定總成績", new ScoreExamCell(new ScoreInfo(sc_attend_id, dtScattendDict[sc_attend_id]["designate_final_score"].ToString())));
                    if (dtScattendDict[sc_attend_id]["remark"] != null)
                        row.AddCell("備註", new TextCell(dtScattendDict[sc_attend_id]["remark"].ToString()));
                    else
                        row.AddCell("備註", new TextCell(""));

                }

                _rowCollection.Add(sc_attend_id, row);
            }



            //foreach (XmlNode node in helper.GetElements("Student"))
            //{
            //    string sc_attend_id = node.Attributes["ID"].Value;

            //    RowEntity row = new RowEntity(node.Attributes["ID"].Value);
            //    row.AddCell("班級", new StudentCell(node.SelectSingleNode("ClassName").InnerText));
            //    row.AddCell("座號", new StudentCell(node.SelectSingleNode("SeatNumber").InnerText));
            //    row.AddCell("姓名", new StudentCell(node.SelectSingleNode("Name").InnerText));
            //    row.AddCell("學號", new StudentCell(node.SelectSingleNode("StudentNumber").InnerText));
            //    foreach (string examid in examList.Keys)
            //    {
            //        ScoreInfo score = GetScore(scoreElement, examid, node.Attributes["ID"].Value);
            //        row.AddCell(examList[examid], new ExamCell(score));
            //    }
            //    //row.AddCell("課程成績", new ScoreExamCell(new ScoreInfo(node.Attributes["ID"].Value, node.SelectSingleNode("Score").InnerText)));

            //    if (dtScattendDict.ContainsKey(sc_attend_id))
            //    {
            //        row.AddCell("課程成績", new ScoreExamCell(new ScoreInfo(sc_attend_id, dtScattendDict[sc_attend_id]["score"].ToString())));

            //        row.AddCell("及格標準", new ScoreExamCell(new ScoreInfo(sc_attend_id, dtScattendDict[sc_attend_id]["passing_standard"].ToString())));
            //        row.AddCell("補考標準", new ScoreExamCell(new ScoreInfo(sc_attend_id, dtScattendDict[sc_attend_id]["makeup_standard"].ToString())));
            //        row.AddCell("直接指定總成績", new ScoreExamCell(new ScoreInfo(sc_attend_id, dtScattendDict[sc_attend_id]["designate_final_score"].ToString())));
            //        if (dtScattendDict[sc_attend_id]["remark"] != null)
            //            row.AddCell("備註", new TextCell(dtScattendDict[sc_attend_id]["remark"].ToString()));
            //        else
            //            row.AddCell("備註", new TextCell(""));

            //    }

            //    _rowCollection.Add(node.Attributes["ID"].Value, row);



            //}
        }

        #region IDataProvider 成員

        public ColumnHeader ColumnHeader
        {
            get { return _columnHeader; }
        }

        public RowCollection Rows
        {
            get { return _rowCollection; }
        }

        public ICell FindCell(string studentid, string columnName)
        {
            return _rowCollection.FindCell(studentid, columnName);
        }

        #endregion

        //private ScoreInfo GetScore(XmlElement element, string examid, string attendid)
        //{
        //    foreach (XmlNode node in element.SelectNodes("Score"))
        //    {
        //        if (node.SelectSingleNode("ExamID").InnerText == examid && node.SelectSingleNode("AttendID").InnerText == attendid)
        //            return new ScoreInfo(node.Attributes["ID"].Value, node.SelectSingleNode("Score").InnerText);
        //    }
        //    return new ScoreInfo();
        //}
        private ScoreInfo GetScore(DataRow data)
        {

            return new ScoreInfo(data["id"].ToString(), data["score"].ToString());
            //  return new ScoreInfo();
        }
    }

    public class ScoreInfo
    {
        private string _score;

        public string Score
        {
            get { return _score; }
            set { _score = value; }
        }
        private string _id;

        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public ScoreInfo(string id, string score)
        {
            _id = id;
            _score = score;
        }

        public ScoreInfo()
        {
            _id = "";
            _score = "";
        }
    }
}
