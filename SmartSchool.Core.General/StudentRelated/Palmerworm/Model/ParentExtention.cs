using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartSchool.StudentRelated.Palmerworm.Model
{
    class ParentExtentionInfo
    {
        /// <summary>
        /// 學生系統編號 
        /// </summary>
        public string StudentID { get; set; }

        /// <summary>
        /// 父親出生年次
        /// </summary>
        public string FatherBirthYear { get; set; }

        /// <summary>
        /// 母親出生年次
        /// </summary>
        public string MotherBirthYear { get; set; }

        /// <summary>
        /// 父親原屬國家地區
        /// </summary>
        public string FatherBirthCountry { get; set; }

        /// <summary>
        /// 母親原屬國家地區
        /// </summary>
        public string MotherBirthCountry { get; set; }

        /// <summary>
        /// 父親email
        /// </summary>
        public string FatherEmail { get; set; }

        /// <summary>
        /// 母親email
        /// </summary>
        public string MotherEmail { get; set; }

        /// <summary>
        /// 監護人email
        /// </summary>
        public string GuardianEmail { get; set; }

        /// <summary>
        /// 父親為大陸配偶_省份
        /// </summary>
        public string FatherFromChina { get; set; }

        /// <summary>
        /// 母親為大陸配偶_省份
        /// </summary>
        public string MotherFromChina { get; set; }

        /// <summary>
        /// 父親為外籍配偶_國籍
        /// </summary>
        public string FatherFromForeign { get; set; }

        /// <summary>
        /// 母親為外籍配偶_國籍
        /// </summary>
        public string MotherFromForeign { get; set; }

        /// <summary>
        /// 父為失業勞工
        /// </summary>
        public string FatherIsUnemployed { get; set; }

        /// <summary>
        /// 母為失業勞工
        /// </summary>
        public string MotherIsUnemployed { get; set; }

        /// <summary>
        ///  Table 【student_info_ext】 是否此學生之資料
        /// </summary>
        public Boolean NoExtensoinRecord { get; set; }

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="studentID"></param>
        internal ParentExtentionInfo(string studentID)
        {
            this.StudentID = studentID;
            this.NoExtensoinRecord = false;
            this.FatherBirthYear = "";
            this.MotherBirthYear = "";
            this.FatherBirthCountry = "";
            this.MotherBirthCountry = "";
            this.FatherEmail = "";
            this.MotherEmail = "";
            this.GuardianEmail = "";
            this.FatherFromChina = "";
            this.MotherFromChina = "";
            this.FatherFromForeign = "";
            this.MotherFromForeign = "";
            this.FatherIsUnemployed = "";
            this.MotherIsUnemployed = "";

        }
    }
}
