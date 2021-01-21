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
        //    #region ����
        //    _columnHeader = new ColumnHeader();
        //    _columnHeader.Columns.Add(new ColumnSetting("1","�Z��", 70));
        //    _columnHeader.Columns.Add(new ColumnSetting("1", "�y��", 65));
        //    _columnHeader.Columns.Add(new ColumnSetting("1", "�m�W", 70));
        //    _columnHeader.Columns.Add(new ColumnSetting("1", "�Ǹ�", 70));
        //    _columnHeader.Columns.Add(new ColumnSetting("1", "���Z1", 80));
        //    _columnHeader.Columns.Add(new ColumnSetting("1", "���Z2", 80));

        //    _rowCollection = new RowCollection();

        //    RowEntity row = new RowEntity("1");            
        //    row.AddCell("�Z��", new StudentCell("��@��"));
        //    row.AddCell("�y��", new StudentCell("1"));
        //    row.AddCell("�m�W", new StudentCell("�����"));
        //    row.AddCell("�Ǹ�", new StudentCell("84203226"));        
        //    row.AddCell("���Z1", new ExamCell("100"));   
        //    row.AddCell("���Z2", new ExamCell("100"));
        //    _rowCollection.Add("1", row);

        //    row = new RowEntity("2");
        //    row.AddCell("�Z��", new StudentCell("��@��"));
        //    row.AddCell("�y��", new StudentCell("2"));
        //    row.AddCell("�m�W", new StudentCell("������"));
        //    row.AddCell("�Ǹ�", new StudentCell("8825252"));
        //    row.AddCell("���Z1", new ExamCell("90"));
        //    row.AddCell("���Z2", new ExamCell("90"));
        //    _rowCollection.Add("2", row);

        //    row = new RowEntity("3");
        //    row.AddCell("�Z��", new StudentCell("��@��"));
        //    row.AddCell("�y��", new StudentCell("3"));
        //    row.AddCell("�m�W", new StudentCell("�g�h�Z�B"));
        //    row.AddCell("�Ǹ�", new StudentCell("08009786"));
        //    row.AddCell("���Z1", new ExamCell("30"));
        //    row.AddCell("���Z2", new ExamCell("50"));
        //    _rowCollection.Add("3", row);

        //    row = new RowEntity("4");
        //    row.AddCell("�Z��", new StudentCell("��@��"));
        //    row.AddCell("�y��", new StudentCell("4"));
        //    row.AddCell("�m�W", new StudentCell("���C�O"));
        //    row.AddCell("�Ǹ�", new StudentCell("08000800"));
        //    row.AddCell("���Z1", new ExamCell("10"));
        //    row.AddCell("���Z2", new ExamCell("10"));
        //    _rowCollection.Add("4", row);

        //    row = new RowEntity("5");
        //    row.AddCell("�Z��", new StudentCell("��@��"));
        //    row.AddCell("�y��", new StudentCell("5"));
        //    row.AddCell("�m�W", new StudentCell("�����"));
        //    row.AddCell("�Ǹ�", new StudentCell("88880000"));
        //    row.AddCell("���Z1", new ExamCell("30"));
        //    row.AddCell("���Z2", new ExamCell("80"));
        //    _rowCollection.Add("5", row);

        //    #endregion
        //}

        #region IDataProvider ����

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

            // ���o�ҵ{�����˪O�էO
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


            // �� Header
            _columnHeader = new ColumnHeader();
            _columnHeader.Columns.Add(new ColumnSetting("-1", "�ǥͨt�νs��", 0));
            _columnHeader.Columns.Add(new ColumnSetting("-1", "�Z��", 60));
            _columnHeader.Columns.Add(new ColumnSetting("-1", "�y��", 40, new DecimalComparer()));
            _columnHeader.Columns.Add(new ColumnSetting("-1", "�m�W", 70));
            _columnHeader.Columns.Add(new ColumnSetting("-1", "�Ǹ�", 70));

            //foreach (XmlNode node in helper.GetElements("Course"))
            //{
            //    string examName = node.SelectSingleNode("ExamName").InnerText;
            //    string examid = node.SelectSingleNode("RefExamID").InnerText;

            //    _columnHeader.Columns.Add(new ColumnSetting(examid, examName, 80, new DecimalComparer()));
            //    examList.Add(examid, examName);
            //}

            foreach (string examid in examDict.Keys)
                _columnHeader.Columns.Add(new ColumnSetting(examid, examDict[examid], 80, new DecimalComparer()));

            _columnHeader.Columns.Add(new ColumnSetting("-1", "�ҵ{���Z", 100, new DecimalComparer()));

            _columnHeader.Columns.Add(new ColumnSetting("-1", "�ή�з�", 80, new DecimalComparer()));
            _columnHeader.Columns.Add(new ColumnSetting("-1", "�ɦҼз�", 80, new DecimalComparer()));
            _columnHeader.Columns.Add(new ColumnSetting("-1", "�������w�`���Z", 120, new DecimalComparer()));
            _columnHeader.Columns.Add(new ColumnSetting("-1", "�Ƶ�", 200));


            // �¼g�k
            // ���^���ҵ{�Ҧ��ǥͪ��Ҹզ��Z
            //DSResponse scoreResp = QueryCourse.GetSECTake(courseid);
            //XmlElement scoreElement = scoreResp.GetContent().GetElement(".");


            // �� Row
            _rowCollection = new RowCollection();
            //dsrsp = QueryCourse.GetSCAttend(courseid);
            //helper = dsrsp.GetContent();

            // ���o�ǥͭ׽Ҭ���
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

            // ���o�Z�ŭ׽Ҿǥ�
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

            // ���o�׽Ҿǥͬq�Ҧ��Z
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
                row.AddCell("�ǥͨt�νs��", new StudentCell(dr["student_id"].ToString()));
                row.AddCell("�Z��", new StudentCell(dr["class_name"].ToString()));
                row.AddCell("�y��", new StudentCell(dr["seat_no"].ToString()));
                row.AddCell("�m�W", new StudentCell(dr["student_name"].ToString()));
                row.AddCell("�Ǹ�", new StudentCell(dr["student_number"].ToString()));

                foreach (string examid in examDict.Keys)
                {
                    string key = examid + "_" + sc_attend_id;
                    ScoreInfo score = new ScoreInfo();
                    if (sceScoreDict.ContainsKey(key))
                    {
                        score = GetScore(sceScoreDict[key]);
                    }

                    if (score.Score == "-1")
                        score.Score = "��";
                    row.AddCell(examDict[examid], new ExamCell(score));
                }

                if (dtScattendDict.ContainsKey(sc_attend_id))
                {
                    row.AddCell("�ҵ{���Z", new ScoreExamCell(new ScoreInfo(sc_attend_id, dtScattendDict[sc_attend_id]["score"].ToString())));

                    row.AddCell("�ή�з�", new ScoreExamCell(new ScoreInfo(sc_attend_id, dtScattendDict[sc_attend_id]["passing_standard"].ToString())));
                    row.AddCell("�ɦҼз�", new ScoreExamCell(new ScoreInfo(sc_attend_id, dtScattendDict[sc_attend_id]["makeup_standard"].ToString())));
                    row.AddCell("�������w�`���Z", new ScoreExamCell(new ScoreInfo(sc_attend_id, dtScattendDict[sc_attend_id]["designate_final_score"].ToString())));
                    if (dtScattendDict[sc_attend_id]["remark"] != null)
                        row.AddCell("�Ƶ�", new TextCell(dtScattendDict[sc_attend_id]["remark"].ToString()));
                    else
                        row.AddCell("�Ƶ�", new TextCell(""));

                }

                _rowCollection.Add(sc_attend_id, row);
            }



            //foreach (XmlNode node in helper.GetElements("Student"))
            //{
            //    string sc_attend_id = node.Attributes["ID"].Value;

            //    RowEntity row = new RowEntity(node.Attributes["ID"].Value);
            //    row.AddCell("�Z��", new StudentCell(node.SelectSingleNode("ClassName").InnerText));
            //    row.AddCell("�y��", new StudentCell(node.SelectSingleNode("SeatNumber").InnerText));
            //    row.AddCell("�m�W", new StudentCell(node.SelectSingleNode("Name").InnerText));
            //    row.AddCell("�Ǹ�", new StudentCell(node.SelectSingleNode("StudentNumber").InnerText));
            //    foreach (string examid in examList.Keys)
            //    {
            //        ScoreInfo score = GetScore(scoreElement, examid, node.Attributes["ID"].Value);
            //        row.AddCell(examList[examid], new ExamCell(score));
            //    }
            //    //row.AddCell("�ҵ{���Z", new ScoreExamCell(new ScoreInfo(node.Attributes["ID"].Value, node.SelectSingleNode("Score").InnerText)));

            //    if (dtScattendDict.ContainsKey(sc_attend_id))
            //    {
            //        row.AddCell("�ҵ{���Z", new ScoreExamCell(new ScoreInfo(sc_attend_id, dtScattendDict[sc_attend_id]["score"].ToString())));

            //        row.AddCell("�ή�з�", new ScoreExamCell(new ScoreInfo(sc_attend_id, dtScattendDict[sc_attend_id]["passing_standard"].ToString())));
            //        row.AddCell("�ɦҼз�", new ScoreExamCell(new ScoreInfo(sc_attend_id, dtScattendDict[sc_attend_id]["makeup_standard"].ToString())));
            //        row.AddCell("�������w�`���Z", new ScoreExamCell(new ScoreInfo(sc_attend_id, dtScattendDict[sc_attend_id]["designate_final_score"].ToString())));
            //        if (dtScattendDict[sc_attend_id]["remark"] != null)
            //            row.AddCell("�Ƶ�", new TextCell(dtScattendDict[sc_attend_id]["remark"].ToString()));
            //        else
            //            row.AddCell("�Ƶ�", new TextCell(""));

            //    }

            //    _rowCollection.Add(node.Attributes["ID"].Value, row);



            //}
        }

        #region IDataProvider ����

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
