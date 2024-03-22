using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.Data;
using System.Data;
using SmartSchool.DAO;
using System.Windows.Forms;
using System.Collections;

namespace SmartSchool
{
    class QueryData
    {
        /// <summary>
        /// 取得所有課程的學年度+學期+課程名稱，比對用
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetCourseNameDictV()
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            try
            {
                QueryHelper qh = new QueryHelper();
                string query = "select id,school_year,semester,course_name from course order by school_year,semester;";
                DataTable dt = qh.Select(query);
                foreach (DataRow dr in dt.Rows)
                {
                    string key = dr["school_year"].ToString() + "_" + dr["semester"].ToString() + "_" + dr["course_name"].ToString();
                    if (!retVal.ContainsKey(key))
                        retVal.Add(key, dr["id"].ToString());
                }
            }
            catch (Exception ex)
            {

            }

            return retVal;
        }

        // 取得系統缺可設定值
        public static List<ScoreValueMangInfo> GetScoreValueMangInfoList()
        {
            List<ScoreValueMangInfo> value = new List<ScoreValueMangInfo>();
            try
            {
                QueryHelper qh = new QueryHelper();
                string strSQL = @"
                  SELECT  
                    array_to_string(xpath('//UseText/text()', settings), '') AS use_text  
                    , array_to_string(xpath('//ScoreType/text()', settings), '') AS score_type  
                    , array_to_string(xpath('//ReportValue/text()', settings), '') AS report_value      
                    , array_to_string(xpath('//UseValue/text()', settings), '') AS use_value  
                    FROM  
                    ( 
                      SELECT unnest(xpath('//Configurations/Configuration/Settings/Setting', xmlparse(content replace(  replace(content,'&lt;', '<'),'&gt;', '>')))) as settings  
                      FROM list WHERE name='評量成績缺考設定'   
                    ) AS content  
                ";
                DataTable dt = qh.Select(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    ScoreValueMangInfo si = new ScoreValueMangInfo();
                    si.UseText = dr["use_text"] + "";
                    si.ScoreType = dr["score_type"] + "";
                    si.ReportValue = dr["report_value"] + "";
                    si.UseValue = dr["use_value"] + "";
                    value.Add(si);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return value;
        }

    }
}
