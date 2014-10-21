using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SmartSchool.API.StudentExtension
{
    internal class SemesterSubjectScore : Customization.Data.StudentExtension.SemesterSubjectScoreInfo_New
    {
        private readonly int _SchoolYear;
        private readonly int _Semester;
        private readonly int _GradeYear;
        private readonly string _Subject;
        private readonly string _Level;
        private readonly decimal _CreditDec;
        private readonly bool _Require;
        private readonly decimal _Score;
        private readonly XmlElement _Detail;
        private readonly bool _Pass;
        /// <summary>
        /// 學年度
        /// </summary>
        public int SchoolYear
        {
            get
            {
                return _SchoolYear;
            }
        }

        /// <summary>
        /// 學期
        /// </summary>
        public int Semester
        {
            get
            {
                return _Semester;
            }
        }

        /// <summary>
        /// 成績年級
        /// </summary>
        public int GradeYear
        {
            get
            {
                return _GradeYear;
            }
        }

        /// <summary>
        /// 科目
        /// </summary>
        public string Subject
        {
            get
            {
                return _Subject;
            }
        }

        /// <summary>
        /// 級別
        /// </summary>
        public string Level
        {
            get
            {
                return _Level;
            }
        }

        /// <summary>
        /// 學分數
        /// </summary>
        public int Credit
        {
            get
            {
                return (Int32)_CreditDec;
            }
        }

        /// <summary>
        /// 學分數
        /// </summary>
        public decimal CreditDec
        {
            get
            {
                return _CreditDec;
            }
        }

        /// <summary>
        /// 必修
        /// </summary>
        public bool Require
        {
            get
            {
                return _Require;
            }
        }

        /// <summary>
        /// 分數
        /// </summary>
        public decimal Score
        {
            get
            {
                return _Score;
            }
        }

        /// <summary>
        /// 詳細資料
        /// </summary>
        public XmlElement Detail
        {
            get
            {
                return _Detail;
            }
        }

        /// <summary>
        /// 取得學分
        /// </summary>
        public bool Pass
        {
            get { return _Pass; }
        }

        public SemesterSubjectScore(
         int schoolYear,
         int semester,
         int gradeYear,
         string subject,
         string level,
         decimal creditDec,
         bool require,
         decimal score,
         XmlElement detail,
         bool pass
            )
        {

            _SchoolYear = schoolYear;
            _Semester = semester;
            _GradeYear = gradeYear;
            _Subject = subject;
            _Level = level;
            _CreditDec = creditDec;
            _Require = require;
            _Score = score;
            _Detail = detail;
            _Pass = pass;
        }
    }
}
