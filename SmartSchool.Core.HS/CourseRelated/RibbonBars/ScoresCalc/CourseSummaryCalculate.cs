using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.CourseRelated.RibbonBars.ScoresCalc
{
    class CourseSummaryCalculate
    {
        private CourseCollection _courses;
        private StringBuilder _message;

        public CourseSummaryCalculate(CourseCollection courses)
        {
            _courses = courses;
        }

        public void Calculate()
        {
            _message = new StringBuilder();
            _message.AppendLine("已選擇課程總數：" + _courses.Count.ToString());

            int count = 0;
            count = CalcNotRequiredCount();
            _message.AppendLine("不需評分之課程數：" + count.ToString());

            count = ExamTemplateNullCount();
            _message.AppendLine("未設定評分樣版之課程數：" + count.ToString());

            count = CalcLackScoreCount();
            _message.AppendLine("含有「成績缺漏」之課程數：" + count.ToString());
        }

        // 選取課程數
        public int GetCourseCount
        {
            get { return _courses.Count; }
        }

        // 未設定評分樣版課程
        public int GetExamTemplateNullCount
        {
            get { return ExamTemplateNullCount(); }
        }

        //  由教師繳交課程成績課程數
        public int GetExamTemplateAllowUploadCount
        {
            get { return ExamTemplateAllowUploadCount(); }
        }

        // 含評量成績缺漏課程
        public int GetLackScoreCount
        {
            get { return CalcLackScoreCount(); }
        }

        private int CalcLackScoreCount()
        {
            int count = 0;
            bool breakCurrent = false;
            foreach (Course each in _courses.Values)
            {
                //  沒有評分樣板就不算
                if (each.ExamTemplate == null) continue;

                // 不需評分的課程不算
                if (each.ExamRequired == false) continue;

                breakCurrent = false;
                foreach (SCAttend attend in each.SCAttends.Values)
                {
                    foreach (TEInclude exam in each.RefExams)
                    {
                        if (!attend.SCETakes.ContainsKey(exam.ExamId))
                        {
                            count++;
                            breakCurrent = true;
                            break;
                        }
                    }
                    if (breakCurrent) break;
                }
            }
            return count;
        }

        private int CalcNotRequiredCount()
        {
            int count = 0;
            foreach (Course each in _courses.Values)
            {
                if (!each.ExamRequired)
                    count++;
            }
            return count;
        }

        private int ExamTemplateNullCount()
        {
            int count = 0;
            foreach (Course each in _courses.Values)
            {
                if (each.ExamTemplate == null)
                    count++;
            }
            return count;
        }

        // 由教師提供
        private int ExamTemplateAllowUploadCount()
        {
            int count = 0;
            foreach (Course each in _courses.Values)
            {
                if (each.ExamTemplate != null)
                {
                    if (each.ExamTemplate.AllowUpload)
                        count++;
                }                    
            }
            return count;
        }


        public string Message()
        {
            return _message.ToString();
        }
    }
}
