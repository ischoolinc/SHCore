using System;
using System.Collections.Generic;
using System.Text;
using FISCA.DSAUtil;
using SmartSchool.Common;
using System.Xml;

namespace SmartSchool.Feature.GraduationPlan
{
    [QueryRequest()]
    public class RemoveGraduationPlan
    {
        public static void Delete(string id)
        {
            DSRequest dsreq = new DSRequest("<DeleteRequest><GraduationPlan><ID>" + id + "</ID></GraduationPlan></DeleteRequest>");
            CurrentUser.Instance.CallService("SmartSchool.GraduationPlan.Delete", dsreq);
        }
    }
}
