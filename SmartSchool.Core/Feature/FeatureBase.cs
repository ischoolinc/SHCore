using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.Common;
using FISCA.DSAUtil;

namespace SmartSchool.Feature
{
    public class FeatureBase
    {
        public static DSResponse CallService(string serviceName, DSRequest request)
        {
            return CurrentUser.Instance.CallService(serviceName, request);            
        }
    }
}
