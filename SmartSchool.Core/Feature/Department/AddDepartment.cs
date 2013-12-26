using System;
using System.Collections.Generic;
using System.Text;
using FISCA.DSAUtil;

namespace SmartSchool.Feature.Department
{
    public class AddDepartment
    {
        public static void Insert(DSRequest request)
        {
            FeatureBase.CallService("SmartSchool.Department.Insert", request);
        }
    }
}
