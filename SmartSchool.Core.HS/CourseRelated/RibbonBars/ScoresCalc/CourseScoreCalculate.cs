using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.CourseRelated.RibbonBars.ScoresCalc
{
    class CourseScoreCalculate
    {
        private CourseCollection _courses;

        // 缺考設定
        private Dictionary<string, string> _UseTextScoreTypeDict;

        public CourseScoreCalculate(CourseCollection courses)
        {
            _courses = courses;
            _UseTextScoreTypeDict = new Dictionary<string, string>();
        }

        public void Calculate() 
        {
            // Calculate(false);
            Calculate(_UseTextScoreTypeDict);
        }


        public void Calculate(bool absentEqualZero)
        {
            foreach (Course course in _courses.Values)
            {
                foreach (SCAttend attend in course.SCAttends.Values)
                    attend.CalculateScore(absentEqualZero);
            }
        }

        // 使用缺考設定
        public void Calculate(Dictionary<string,string> UseTextScoreTypeDict)
        {
            foreach (Course course in _courses.Values)
            {
                foreach (SCAttend attend in course.SCAttends.Values)
                    attend.CalculateScore(UseTextScoreTypeDict);
            }
        }

    }
}
