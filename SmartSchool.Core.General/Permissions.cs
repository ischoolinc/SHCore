using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSchool
{
    internal class Permissions
    {
        public static string 部別管理 { get { return "SmartSchool.Core.General.DeptGroupSetup"; } }
        public static bool 部別管理權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[部別管理].Executable;
            }
        }

        public static string 上課地點管理 { get { return "SmartSchool.Core.General.ClassRoomConfig"; } }
        public static bool 上課地點管理權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[上課地點管理].Executable;
            }
        }
    }
}
