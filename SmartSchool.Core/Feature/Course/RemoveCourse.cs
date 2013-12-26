using System;
using System.Collections.Generic;
using System.Text;
using FISCA.DSAUtil;

namespace SmartSchool.Feature.Course
{
    [QueryRequest()]
    public class RemoveCourse
    {
        public static void DeleteCourse(string courseId)
        {
            DSXmlHelper helper = new DSXmlHelper("DeleteRequest");
            helper.AddElement("Course");
            helper.AddElement("Course", "ID", courseId);

            DSRequest dsreq = new DSRequest(helper);
            DSResponse rsp = FeatureBase.CallService("SmartSchool.Course.Delete", dsreq);
        }
    }
}
