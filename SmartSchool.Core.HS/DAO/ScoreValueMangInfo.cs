using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSchool.DAO
{
    public class ScoreValueMangInfo
    {
        // 輸入內容
        public string UseText { get; set; }
        // 分數設定
        public string ScoreType { get; set; }

        // 缺考原因
        public string ReportValue { get; set; }

        // 資料庫儲存值 (-1:0分，-2:免試)
        public string UseValue { get; set; }
    }
}
