using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartSchool
{
    public static class CreditConvertHelper
    {
        /// <summary>
        /// 2014/10/7 - 新增擴充方法
        /// 用以取得小數點型態的學分數內容
        /// </summary>
        public static decimal CreditDec(this SemesterSubjectScoreInfo info)
        {
            if (info is SemesterSubjectScoreInfo_New)
                return (info as SemesterSubjectScoreInfo_New).CreditDec;
            else
                return info.Credit;
        }

        /// <summary>
        /// 2014/10/8 - 新增擴充方法
        /// 用以取得小數點型態的學分數內容
        /// </summary>
        public static decimal CreditDec(this CourseRecord info)
        {
             if (info is CourseRecord_New)
                  return (info as CourseRecord_New).CreditDec;
             else
                  return info.Credit;
        }
    }
}
