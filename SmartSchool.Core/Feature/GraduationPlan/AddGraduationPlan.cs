using System;
using System.Collections.Generic;
using System.Text;
using FISCA.DSAUtil;
using SmartSchool.Common;
using System.Xml;

namespace SmartSchool.Feature.GraduationPlan
{
    public class AddGraduationPlan
    {
        public static void Insert(string name, XmlElement graduationPlan)
        {
            DSRequest dsreq = new DSRequest("<InsertRequest><GraduationPlan><Name>" + name + "</Name><Content>" + graduationPlan.OuterXml + "</Content></GraduationPlan></InsertRequest>");
            CurrentUser.Instance.CallService("SmartSchool.GraduationPlan.Insert", dsreq);
        }
    }
}
