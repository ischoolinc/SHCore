using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using FISCA.DSAUtil;

namespace SmartSchool.CourseRelated
{
    [Serializable()]
    public class CourseInformation : IComparable<CourseInformation>
    {
        private int _identity, _class_id, _school_year, _semester, _teacher_id;

        /// <summary>
        /// 2014/9/29 - 改為Decimal型態
        /// </summary>
        private decimal _credit;

        private int _class_grade_year, _class_display_order, _ref_exam_template_id;
        private string _subj_level, _course_name, _subject, _class_name, _taught_by, _taught_nickname;
        private string _teacher_category, _exam_template, _required, _requiredby;
        private bool _notIncludedInCalc, _notIncludedInCredit;
        private string _entry;
        private string _courseNumber;
        private List<Teacher> _teachers;

        public CourseInformation(XmlElement rawData)
        {
            _identity = GetIntValue(rawData, "@ID");
            _subj_level = GetStringValue(rawData, "SubjectLevel");
            _class_id = GetIntValue(rawData, "RefClassID");
            _school_year = GetIntValue(rawData, "SchoolYear");
            _semester = GetIntValue(rawData, "Semester");
            _credit = GetDecimalValue(rawData, "Credit"); //2014/9/29 - 改為Decimal型態
            _course_name = GetStringValue(rawData, "CourseName");
            _subject = GetStringValue(rawData, "Subject");
            _class_name = GetStringValue(rawData, "ClassName");
            _teacher_id = GetIntValue(rawData, "MajorTeacherID");
            GetTeacherName(rawData);
            _teacher_category = GetStringValue(rawData, "MajorTeacherCategory");
            _class_grade_year = GetIntValue(rawData, "ClassGradeYear");
            _class_display_order = GetIntValue(rawData, "ClassDisplayOrder");
            _exam_template = GetStringValue(rawData, "ExamTemplateName");
            _required = GetStringValue(rawData, "IsRequired");
            _requiredby = GetStringValue(rawData, "RequiredBy");
            _ref_exam_template_id = GetIntValue(rawData, "RefExamTemplateID");
            _notIncludedInCalc = (GetStringValue(rawData, "NotIncludedInCalc") == "是");
            _notIncludedInCredit = (GetStringValue(rawData, "NotIncludedInCredit") == "是");
            _entry = GetStringValue(rawData, "ScoreType");
            _courseNumber = GetStringValue(rawData, "CourseName");
            _teachers = new List<Teacher>();
            foreach (XmlElement each in rawData.SelectNodes("Teachers/Teacher"))
                _teachers.Add(new Teacher(each));
            _teachers.Sort(sortBySequence);
        }

        private void GetTeacherName(XmlElement rawData)
        {
            _taught_by = GetStringValue(rawData, "MajorTeacherName");
            _taught_nickname = GetStringValue(rawData, "MajorTeacherNickname");
        }

        private static int sortBySequence(Teacher t1, Teacher t2)
        {
            return t1.Sequence.CompareTo(t2.Sequence);
        }
        public bool NotIncludedInCalc
        {
            get { return _notIncludedInCalc; }
        }

        public bool NotIncludedInCredit
        {
            get { return _notIncludedInCredit; }
        }

        public string Required { get { return _required; } }

        public string RequiredBy { get { return _requiredby; } }

        public string Entry
        {
            get { return _entry; }
        }

        public string CourseName
        {
            get { return _course_name; }
        }

        public string Subject
        {
            get { return _subject; }
        }

        public string ClassName
        {
            get { return SmartSchool.ClassRelated.Class.Instance.Items.ContainsKey("" + _class_id) ? SmartSchool.ClassRelated.Class.Instance["" + _class_id].ClassName : ""; }
        }

        public int Identity
        {
            get { return _identity; }
        }

        public string SubjectLevel
        {
            get { return _subj_level; }
        }

        public int ClassID
        {
            get { return _class_id; }
        }

        public int SchoolYear
        {
            get { return _school_year; }
        }

        public int Semester
        {
            get { return _semester; }
        }

        public string CourseNumber
        {
            get { return _courseNumber; }
        }

        [Obsolete("2014/9/29 已過時,請改用CreditDec")]
        public int Credit
        {
            get { return (Int32)CreditDec; }
        }

        public decimal CreditDec
        {
            get { return _credit; }
        }

        public string MajorTeacherName
        {
            get { return _taught_by; }
        }

        public int MajorTeacherID
        {
            get { return _teacher_id; }
        }

        public string MajorTeacherCategory
        {
            get { return _teacher_category; }
        }

        public List<Teacher> Teachers
        {
            get { return _teachers; }
        }

        public int ClassGradeYear
        {
            get { return _class_grade_year; }
        }

        public int ClassDisplayOrder
        {
            get { return _class_display_order; }
        }

        public string ExamTemplateName
        {
            get { return _exam_template; }
        }

        public int RefExamTemplateID
        {
            get { return _ref_exam_template_id; }
        }

        public override bool Equals(object obj)
        {
            CourseInformation course = obj as CourseInformation;

            if (course == null) return false;

            return (course._identity == _identity);
        }

        public override int GetHashCode()
        {
            return _identity;
        }

        private int GetIntValue(XmlElement rawData, string xpath)
        {
            XmlNode value = rawData.SelectSingleNode(xpath);

            ThrowResultNullException(xpath, value);

            //if (string.IsNullOrEmpty(value.InnerText))
            //    return -1;

            //2011/2/23 由騉翔修改，若是空白則傳回0，因為若是傳回-1會造成學分數顯示及計算錯誤
            if (string.IsNullOrEmpty(value.InnerText))
                return 0;

            int result;
            if (int.TryParse(value.InnerText, out result))
                return result;
            else
                throw new ArgumentException("課程欄位資料型態不正確，XPath 路徑：" + xpath);
        }

        private decimal GetDecimalValue(XmlElement rawData, string xpath)
        {
            XmlNode value = rawData.SelectSingleNode(xpath);

            ThrowResultNullException(xpath, value);

            //if (string.IsNullOrEmpty(value.InnerText))
            //    return -1;

            //2011/2/23 由騉翔修改，若是空白則傳回0，因為若是傳回-1會造成學分數顯示及計算錯誤
            if (string.IsNullOrEmpty(value.InnerText))
                return 0;

            decimal result;
            if (decimal.TryParse(value.InnerText, out result))
                return result;
            else
                throw new ArgumentException("課程欄位資料型態不正確，XPath 路徑：" + xpath);
        }

        private string GetStringValue(XmlElement rawData, string xpath)
        {
            XmlNode value = rawData.SelectSingleNode(xpath);

            ThrowResultNullException(xpath, value);

            return value.InnerText;
        }

        private static void ThrowResultNullException(string xpath, XmlNode value)
        {
            if (value == null)
                throw new ArgumentException("課程欄位資料不存在，XPath 路徑：" + xpath);
        }

        public class Teacher
        {
            private int _sequence, _teacher_id;
            private string _teacher_name, _nickname, _teacher_category;

            public Teacher(XmlElement teacherXml)
            {
                _teacher_id = int.Parse(teacherXml.GetAttribute("TeacherID"));
                _teacher_name = teacherXml.GetAttribute("TeacherName");
                _nickname = teacherXml.GetAttribute("TeacherNickname");
                _teacher_category = teacherXml.GetAttribute("TeacherCategory");
                _sequence = int.Parse(teacherXml.GetAttribute("Sequence"));
            }

            public string TeacherName
            {
                get { return _teacher_name; }
            }

            public string Nickname
            {
                get { return _nickname; }
            }

            public string UniqName
            {
                get
                {
                    if (string.IsNullOrEmpty(Nickname))
                        return TeacherName;
                    else
                        return string.Format("{0} ({1})", TeacherName, Nickname);
                }
            }

            public int TeacherID
            {
                get { return _teacher_id; }
            }

            public string TeacherCategory
            {
                get { return _teacher_category; }
            }

            public int Sequence
            {
                get { return _sequence; }
            }
        }

        #region IComparable<CourseInformation> 成員

        public int CompareTo(CourseInformation other)
        {
            //if ( this.RefClassID == other.RefClassID )
            //{
            //    if ( this.RefClassID != "" )
            //    {
            //        int seatNo1, seatNo2;
            //        int.TryParse(this.SeatNo, out seatNo1);
            //        int.TryParse(other.SeatNo, out seatNo2);
            //        return seatNo1.CompareTo(seatNo2);
            //    }
            //    else
            //    {
            //        return this.StudentNumber.CompareTo(other.StudentNumber);
            //    }
            //}
            //else
            //{
            //    if ( this.RefClassID == "" ) return -1;
            //    if ( other.RefClassID == "" ) return 1;
            //    return ClassRelated.Class.Instance.Items[this.RefClassID].CompareTo(ClassRelated.Class.Instance.Items[other.RefClassID]);
            //}
            if (this.SchoolYear == other.SchoolYear)
            {
                if (this.Semester == other.Semester)
                {
                    if (this.ClassName == other.ClassName)
                    {
                        return SmartSchool.Common.StringComparer.Comparer(this.Subject, other.Subject, "國文", "英文", "數學", "物理", "化學", "歷史", "地理", "公民", "基礎", "進階");
                    }
                    else
                    {
                        if (!ClassRelated.Class.Instance.Items.ContainsKey("" + this.ClassID)) return -1;
                        if (!ClassRelated.Class.Instance.Items.ContainsKey("" + other.ClassID)) return 1;
                        return ClassRelated.Class.Instance["" + this.ClassID].CompareTo(ClassRelated.Class.Instance["" + other.ClassID]);
                    }
                }
                else
                {
                    return this.Semester.CompareTo(other.Semester);
                }
            }
            else
            {
                return this.SchoolYear.CompareTo(other.SchoolYear);
            }

        }

        #endregion
    }
}
