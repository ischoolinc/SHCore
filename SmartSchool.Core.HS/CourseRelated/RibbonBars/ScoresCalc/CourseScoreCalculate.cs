using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.CourseRelated.RibbonBars.ScoresCalc
{
    class CourseScoreCalculate
    {
        private CourseCollection _courses;

        public CourseScoreCalculate(CourseCollection courses)
        {
            _courses = courses;
        }

        public void Calculate() 
        {
            Calculate(false);
        }


        public void Calculate(bool absentEqualZero)
        {
            foreach (Course course in _courses.Values)
            {
                foreach (SCAttend attend in course.SCAttends.Values)
                    attend.CalculateScore(absentEqualZero);
            }
        }
    }
}
