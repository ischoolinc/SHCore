using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.Common;

namespace SmartSchool.Feature.GraduationPlan
{
    [QueryRequest()]
    public class EditGraduationPlan
    {
        public static void Update(string id,XmlElement graduationPlan)
        {
            DSRequest dsreq = new DSRequest("<UpdateRequest><GraduationPlan><Field><Content>"+graduationPlan.OuterXml+"</Content></Field><Condition><ID>"+id+"</ID></Condition></GraduationPlan></UpdateRequest>");
            CurrentUser.Instance.CallService("SmartSchool.GraduationPlan.Update", dsreq);
        }

        // 更新課程規劃表
        public static void Update(string id, string name, XmlElement graduationPlan)
        {
            DSRequest dsreq = new DSRequest("<UpdateRequest><GraduationPlan><Field><Name>" + name + "</Name><Content>" + graduationPlan.OuterXml + "</Content></Field><Condition><ID>" + id + "</ID></Condition></GraduationPlan></UpdateRequest>");
            CurrentUser.Instance.CallService("SmartSchool.GraduationPlan.Update", dsreq);
        }

        public static void SetCommon( XmlElement graduationPlan)
        {
            DSRequest dsreq = new DSRequest("<SetCommonRequest><GraduationPlan><Content>"+graduationPlan.OuterXml+"</Content></GraduationPlan></SetCommonRequest>");
            CurrentUser.Instance.CallService("SmartSchool.GraduationPlan.SetCommon", dsreq);
        }
    }
}
