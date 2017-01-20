using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.Feature.Course;

namespace SmartSchool.CourseRelated.RibbonBars.ScoresCalc
{
    class SCAttend
    {
        private string _identity, _RefStudentID, _course_id;
        private string _class_name, _student_number, _stu_name, _seat_number;
        private ExamScoreCollection _scetakes;
        private Course _course;
        private string _previous_score, _score;
        private bool _contains_lack = false;

        public SCAttend(XmlElement data)
        {
            DSXmlHelper objData = new DSXmlHelper(data);
            _identity = objData.GetText("@ID");
            _RefStudentID = objData.GetText("RefStudentID");
            _course_id = objData.GetText("RefCourseID");
            _class_name = objData.GetText("ClassName");
            _student_number = objData.GetText("StudentNumber");
            _seat_number = objData.GetText("SeatNumber");
            _stu_name = objData.GetText("Name");
            _previous_score = objData.GetText("Score");

            _scetakes = new ExamScoreCollection();
        }

        public string Identity
        {
            get { return _identity; }
        }

        public string StudentIdentity
        {
            get { return _RefStudentID; }
        }

        public string CourseIdentity
        {
            get { return _course_id; }
        }

        public string ClassName
        {
            get { return _class_name; }
        }

        public string StudentNumber
        {
            get { return _student_number; }
        }

        public string StudentName
        {
            get { return _stu_name; }
        }

        public string SeatNumber
        {
            get { return _seat_number; }
        }

        public Course Course
        {
            get { return _course; }
        }

        public ExamScoreCollection SCETakes
        {
            get { return _scetakes; }
        }

        public string PreviousScore
        {
            get { return _previous_score; }
            set { _previous_score = value; }
        }

        public string Score
        {
            get { return _score; }
        }

        public void SaveScore()
        {
            _previous_score = _score;
        }

        public void SetScore(string score)
        {
            _score = score;
        }

        public void SetCourse(Course course)
        {
            _course = course;
        }

        public bool ContainsLack
        {
            get { return _contains_lack; }
        }

        public void CalculateScore()
        {
            CalculateScore(false);


        }
        /// <summary>
        /// 進位方式
        /// </summary>
        private enum RoundMode { 四捨五入, 無條件進位, 無條件捨去 }
        private static decimal GetRoundScore(decimal score, int decimals, RoundMode mode)
        {
            decimal seed = Convert.ToDecimal(Math.Pow(0.1, Convert.ToDouble(decimals)));
            switch (mode)
            {
                default:
                case RoundMode.四捨五入:
                    score = decimal.Round(score, decimals, MidpointRounding.AwayFromZero);
                    break;
                case RoundMode.無條件捨去:
                    score /= seed;
                    score = decimal.Floor(score);
                    score *= seed;
                    break;
                case RoundMode.無條件進位:
                    decimal d2 = GetRoundScore(score, decimals, RoundMode.無條件捨去);
                    if (d2 != score)
                        score = d2 + seed;
                    else
                        score = d2;
                    break;
            }
            string ss = "0.";
            for (int i = 0; i < decimals; i++)
            {
                ss += "0";
            }
            return Convert.ToDecimal(Math.Round(score, decimals).ToString(ss));
        }
        public void CalculateScore(bool absentEqualZero)
        {
            if (Course.ExamTemplate == null)
                return;

            if (!Course.ExamRequired)
                return;

            //AllowUpload 為 True 時，略過成績計算，因為成績是由老師提供。
            if (Course.ExamTemplate.AllowUpload)
                return;

            decimal total = 0;



            if (absentEqualZero)
            {

                foreach (TEInclude exam in Course.RefExams)
                {
                    decimal score = 0;
                    if (SCETakes.ContainsKey(exam.ExamId))
                    {
                        SCETake take = SCETakes[exam.ExamId];
                        if (!decimal.TryParse(take.Score, out score)) //如果缺考會 0 分處理。
                            _contains_lack = true;
                    }
                    else
                        _contains_lack = true;

                    total += (score * ((decimal)exam.Weight / 100m));
                }

            }

            //缺考不當0分處理
            else
            {
                decimal weight = 0;

                foreach (TEInclude exam in Course.RefExams)
                {
                    decimal score = 0;

                    if (SCETakes.ContainsKey(exam.ExamId))
                    {
                        SCETake take = SCETakes[exam.ExamId];
                        if (!decimal.TryParse(take.Score, out score))
                        {
                            _contains_lack = true;
                        }
                        else
                        {
                            total += (score * (decimal)exam.Weight);
                            weight += (decimal)exam.Weight;
                        }

                    }
                    else
                    {
                        _contains_lack = true;
                    }
                }
                if (weight != 0)
                {
                    total = total / weight;
                }
            }
            #region 處理小數點位數
            //精準位數
            int decimals = 2;
            //進位模式
            RoundMode mode = RoundMode.四捨五入;
            #region 抓成績計算規則
            SmartSchool.Customization.Data.AccessHelper accessHelper = new Customization.Data.AccessHelper();
            var studentRec = accessHelper.StudentHelper.GetStudent(_RefStudentID);
            accessHelper.StudentHelper.FillField("成績計算規則", new SmartSchool.Customization.Data.StudentRecord[] { studentRec });
            if (studentRec.Fields.ContainsKey("成績計算規則"))
            {
                XmlElement scoreCalcRule = studentRec.Fields["成績計算規則"] as XmlElement;
                DSXmlHelper helper = new DSXmlHelper(scoreCalcRule);
                bool tryParsebool;
                int tryParseint;
                if (scoreCalcRule.SelectSingleNode("各項成績計算位數/科目成績計算位數") != null)
                {
                    if (int.TryParse(helper.GetText("各項成績計算位數/科目成績計算位數/@位數"), out tryParseint))
                        decimals = tryParseint;
                    if (bool.TryParse(helper.GetText("各項成績計算位數/科目成績計算位數/@四捨五入"), out tryParsebool) && tryParsebool)
                        mode = RoundMode.四捨五入;
                    if (bool.TryParse(helper.GetText("各項成績計算位數/科目成績計算位數/@無條件捨去"), out tryParsebool) && tryParsebool)
                        mode = RoundMode.無條件捨去;
                    if (bool.TryParse(helper.GetText("各項成績計算位數/科目成績計算位數/@無條件進位"), out tryParsebool) && tryParsebool)
                        mode = RoundMode.無條件進位;
                }
            }
            #endregion
            total = GetRoundScore(total, decimals, mode);
            #endregion

            //SetScore(Math.Round(total, 2).ToString());
            SetScore(total.ToString());
        }

        public static SCAttendCollection GetSCAttends(IProgressUI progress, params string[] courseIds)
        {
            SCAttendPackingLoader loader = new SCAttendPackingLoader(progress, courseIds);
            return loader.LoadData();
        }
    }

    class SCAttendCollection : Dictionary<string, SCAttend>
    {
    }

    class SCAttendPackingLoader
    {
        private const int PackingSize = 50;

        private IProgressUI _progress;
        private List<List<string>> _packings;

        public SCAttendPackingLoader(IProgressUI progress, string[] courseIds)
        {
            _progress = progress;

            _packings = new List<List<string>>();
            List<string> package = new List<string>();
            for (int i = 0; i < courseIds.Length; i++)
            {
                if (i % PackingSize == 0)
                {
                    package = new List<string>();
                    _packings.Add(package);
                }
                package.Add(courseIds[i]);
            }
        }

        public SCAttendCollection LoadData()
        {
            int current = 0;
            SCAttendCollection objSCAttends = new SCAttendCollection();
            foreach (List<string> each in _packings)
            {
                current++;

                if (_progress.Cancellation) break;

                _progress.ReportProgress(string.Format("下載修課相關資料({0}%)", Math.Round(((float)current / (float)_packings.Count) * 100, 0)), 0);
                XmlElement xmlSCAttends = QueryCourse.GetSCAttendBrief(each.ToArray()).GetContent().BaseElement;

                foreach (XmlElement attend in xmlSCAttends.SelectNodes("Student"))
                {
                    SCAttend scattend = new SCAttend(attend);
                    objSCAttends.Add(scattend.Identity, scattend);
                }
            }

            return objSCAttends;
        }
    }
}
