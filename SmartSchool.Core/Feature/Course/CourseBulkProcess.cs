using System;
using System.Collections.Generic;
using System.Text;
using FISCA.DSAUtil;
using System.Xml;

namespace SmartSchool.Feature.Course
{
    public class CourseBulkProcess
    {
        public static System.Xml.XmlElement GetExportDescription()
        {
            return CallNoneRequestService("SmartSchool.Course.BulkProcess.GetExportDescription");
        }

        private static XmlElement CallNoneRequestService(string serviceName)
        {
            string strServiceName = serviceName;
            DSResponse rsp = FeatureBase.CallService(serviceName, null);

            if (rsp.GetContent() == null)
                throw new Exception("�A�ȥ��^�ǥ�������T�C(" + strServiceName + ")");

            return rsp.GetContent().BaseElement;
        }

        public static DSResponse GetExportList(DSRequest request)
        {
            return FeatureBase.CallService("SmartSchool.Course.BulkProcess.Export", request);
        }

        public static XmlElement GetImportDescription()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Properties.Resources.SH_Course_ImportFieldList);
            return doc.DocumentElement;
            
            //return CallNoneRequestService("SmartSchool.Course.BulkProcess.GetImportFieldList");
        }

        public static XmlElement GetFieldValidationRule()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Properties.Resources.CourseBulkProcessValidateFieldRule);
            return doc.DocumentElement;
            //return CallNoneRequestService("SmartSchool.Course.BulkProcess.GetValidateFieldRule");
        }

        public static XmlElement GetPrimaryKeyList()
        {
            return CallNoneRequestService("SmartSchool.Course.BulkProcess.GetUniqueFieldData");
        }

        public static XmlElement GetShiftCheckList(params string[] fieldList)
        {
            DSXmlHelper request = new DSXmlHelper("Request");
            foreach (string each in fieldList)
                request.AddElement(".", each);

            string sn = "SmartSchool.Course.BulkProcess.GetShiftCheckList";
            return FeatureBase.CallService(sn, new DSRequest(request)).GetContent().BaseElement;
        }

        public static XmlElement GetCourseTeachers(IEnumerable<string> fieldList)
        {
            DSXmlHelper request = new DSXmlHelper("Request");
            foreach (string each in fieldList)
                request.AddElement(".", each);

            string sn = "SmartSchool.Course.BulkProcess.GetCourseTeachers";
            return FeatureBase.CallService(sn, new DSRequest(request)).GetContent().BaseElement;
        }

        public static void InsertImportCourse(XmlElement data)
        {
            FeatureBase.CallService("SmartSchool.Course.BulkProcess.InsertImportCourse", new DSRequest(data));
        }

        public static void UpdateImportCourse(XmlElement data)
        {
            FeatureBase.CallService("SmartSchool.Course.BulkProcess.UpdateImportCourse", new DSRequest(data));
        }
    }
}
