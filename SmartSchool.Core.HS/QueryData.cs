using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.Data;
using System.Data;

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
                    if(!retVal.ContainsKey(key))
                        retVal.Add(key,dr["id"].ToString());
                }
            }
            catch (Exception ex)
            { 
            
            }

            return retVal;
        }
    }
}
