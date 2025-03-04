using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSchool.DAO
{
    public class ExamScoreNewInfo
    {
        // 試別id
        public string ExamID { get; set; }

        // 試別名稱
        public string ExamName { get; set; }

        // 試別狀態試缺或免
        public string ExamStatus { get; set; }

        // 試別成績
        public decimal? ExamScore { get; set; }

        // 試別比重
        public decimal? ExamWeight { get; set; }

        // 是否使用群組
        public bool UseGroup { get; set; }
    }
}
