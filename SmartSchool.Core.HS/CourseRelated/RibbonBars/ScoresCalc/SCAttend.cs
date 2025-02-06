using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using DevComponents.DotNetBar;
using FISCA.DSAUtil;
using SmartSchool.CourseRelated.DetailPaneItem.OtherEntity;
using SmartSchool.DAO;
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


        // 缺考設定使用
        public void CalculateScore(Dictionary<string, string> UseTextScoreType)
        {
            if (Course.ExamTemplate == null)
                return;

            if (!Course.ExamRequired)
                return;

            //AllowUpload 為 True 時，略過成績計算，因為成績是由老師提供。
            if (Course.ExamTemplate.AllowUpload)
                return;

            decimal total = 0;

            // 當沒有缺考設定，使用原本系統內預設處理方式
            if (UseTextScoreType.Count == 0)
            {
                decimal weight = 0;
                // 
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
                else
                {
                    return; // 完全沒輸入不處理
                }
            }
            else
            {
                // 當有缺考設定，使用缺考設定處理方式

                decimal weight = 0; // 總分比重

                // 計算處理：
                //1.建立新試別成績物件。
                Dictionary<string, ExamScoreNewInfo> ExamScoreDict = new Dictionary<string, ExamScoreNewInfo>();

                //2.取得各試別資料填入新物件。
                foreach (TEInclude exam in Course.RefExams)
                {
                    ExamScoreNewInfo examS = new ExamScoreNewInfo();
                    examS.ExamID = exam.ExamId;
                    examS.ExamWeight = (decimal)exam.Weight;
                    examS.ExamName = exam.ExamName;
                    examS.UseGroup = exam.UseGroup;

                    if (!ExamScoreDict.ContainsKey(exam.ExamId))
                    {
                        ExamScoreDict.Add(exam.ExamId, examS);
                    }
                }

                //3.取得學生成績填入物件，並填入缺或免。
                foreach (ExamScoreNewInfo exam in ExamScoreDict.Values)
                {
                    if (SCETakes.ContainsKey(exam.ExamID))
                    {
                        SCETake take = SCETakes[exam.ExamID];

                        // 取得分數，沒有輸入當作免試
                        if (string.IsNullOrEmpty(take.Score))
                        {
                            exam.ExamStatus = "免試";
                        }
                        else
                        {
                            decimal score = 0;
                            decimal.TryParse(take.Score, out score);

                            // 使用成績本身判斷
                            if (take.Score == "-1")
                            {
                                exam.ExamScore = 0;
                                exam.ExamStatus = "0分";
                            }
                            else if (take.Score == "-2")
                            {
                                exam.ExamScore = -2;
                                exam.ExamStatus = "免試";
                            }
                            else
                            {
                                exam.ExamScore = score;
                                exam.ExamStatus = "";
                            }
                        }

                        //// 讀取缺考設定處理                        
                        //if (take.UseText != null && UseTextScoreType.ContainsKey(take.UseText))
                        //{
                        //    string scoreType = UseTextScoreType[take.UseText];
                        //    exam.ExamStatus = scoreType;
                        //    if (scoreType == "0分")
                        //    {
                        //        exam.ExamScore = 0;
                        //    }
                        //    else
                        //    {    // 免試
                        //        exam.ExamScore = -2;
                        //    }
                        //}
                    }
                    else
                    {
                        _contains_lack = true;
                    }
                }

                //4.檢查免試設定與比重均分
                // 免試群組比數與個數
                decimal wGroup = 0;
                int wCount = 0;
                foreach (ExamScoreNewInfo exam in ExamScoreDict.Values)
                {
                    // 計算使用群組且免試比重
                    if (exam.UseGroup && exam.ExamStatus == "免試")
                    {
                        if (exam.ExamWeight.HasValue)
                        {
                            wGroup += exam.ExamWeight.Value;

                        }
                    }
                    else
                    {
                        // 分給是群組，一般狀態
                        if (exam.UseGroup && exam.ExamStatus == "")
                            wCount += 1;
                    }

                }

                // 均分比重
                decimal wAvg = 0;
                if (wCount > 0)
                    wAvg = wGroup / (decimal)wCount;

                // 群組內比重重新分配
                foreach (ExamScoreNewInfo exam in ExamScoreDict.Values)
                {
                    // 計算使用群組且免試比重
                    if (exam.UseGroup && exam.ExamStatus == "")
                        exam.ExamWeight += wAvg;
                }

                // 重新算成績
                foreach (ExamScoreNewInfo exam in ExamScoreDict.Values)
                {
                    // 計算使用群組且免試比重
                    if (exam.ExamStatus == "")
                    {
                        if (exam.ExamWeight.HasValue)
                        {
                            weight += exam.ExamWeight.Value;
                            if (exam.ExamScore.HasValue)
                                total += exam.ExamScore.Value * exam.ExamWeight.Value;
                        }
                    }

                }

                if (weight != 0)
                {
                    total = total / weight;
                }
                else
                {
                    return;  // 完全沒輸入不處理
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
