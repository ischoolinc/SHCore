using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.StudentRelated.Placing.Score.SubjectScore
{
    public class SuperSubjectScore : AbstractSubjectScore
    {
         private decimal _credit;
        public SuperSubjectScore(string subjectName, decimal score, decimal credit)
        {
            SetSubjectName(subjectName);
            SetScore(score);
            _credit = credit;
        }

        public decimal Credit
        {
            get { return _credit; }
        }
    }
}
