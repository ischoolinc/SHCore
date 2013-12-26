using System;
using System.Collections.Generic;
using System.Text;
using FISCA.DSAUtil;

namespace SmartSchool.Feature.Department
{
    [QueryRequest()]
    public class RemoveDepartment
    {
        public static void Delete(DSRequest request)
        {
            FeatureBase.CallService("SmartSchool.Department.Delete", request);
        }
    }
}
