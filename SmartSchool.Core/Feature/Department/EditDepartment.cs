using System;
using System.Collections.Generic;
using System.Text;
using FISCA.DSAUtil;

namespace SmartSchool.Feature.Department
{
    [QueryRequest()]
    public class EditDepartment
    {
        public static void Update(DSRequest request){
            FeatureBase.CallService("SmartSchool.Department.Update", request);
        }
    }
}
