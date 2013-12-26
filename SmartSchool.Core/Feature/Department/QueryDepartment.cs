using System;
using System.Collections.Generic;
using System.Text;
using FISCA.DSAUtil;

namespace SmartSchool.Feature.Department
{
    [QueryRequest()]
    public class QueryDepartment
    {
        public static DSResponse GetAbstractList()
        {
            return FeatureBase.CallService("SmartSchool.Department.GetAbstractList", new DSRequest());
        }

        public static DSResponse GetUsedDepartment()
        {
            return FeatureBase.CallService("SmartSchool.Department.GetUsedDepartment", new DSRequest());
        }
    }
}
