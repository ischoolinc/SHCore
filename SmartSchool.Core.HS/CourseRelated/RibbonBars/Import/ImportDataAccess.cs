using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using SmartSchool.Feature.Class;
using SmartSchool.Feature.Teacher;
using FISCA.DSAUtil;
using SmartSchool.Feature.ExamTemplate;
using SmartSchool.ImportSupport;

namespace SmartSchool.CourseRelated.RibbonBars.Import
{
    internal class ImportDataAccess : IDataAccess
    {
        public XmlElement GetImportFieldList()
        {
            return SmartSchool.Feature.Course.CourseBulkProcess.GetImportDescription();
        }

        public XmlElement GetValidateFieldRule()
        {
            // 放在Client 端，如果有問題再用Service

            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(Properties.Resources.CourseBulkProcessValidateFieldRule);
            XmlElement elm = Doc.DocumentElement;

            if (elm == null)
                return SmartSchool.Feature.Course.CourseBulkProcess.GetFieldValidationRule();
            else
                return elm;
        }

        public XmlElement GetUniqueFieldData()
        {
            return SmartSchool.Feature.Course.CourseBulkProcess.GetPrimaryKeyList();
        }

        public XmlElement GetShiftCheckList(params string[] fieldNameList)
        {
            return SmartSchool.Feature.Course.CourseBulkProcess.GetShiftCheckList(fieldNameList);
        }

        public void InsertImportData(XmlElement data)
        {
            SmartSchool.Feature.Course.CourseBulkProcess.InsertImportCourse(data);
        }

        public void UpdateImportData(XmlElement data)
        {
            SmartSchool.Feature.Course.CourseBulkProcess.UpdateImportCourse(data);
        }

        public void AddCourseTeachers(XmlElement request)
        {
            SmartSchool.Feature.Course.EditCourse.AddCourseTeacher(new DSXmlHelper(request));
        }

        public void RemoveCourseTeachers(XmlElement request)
        {
            SmartSchool.Feature.Course.EditCourse.RemoveCourseTeachers(new DSXmlHelper(request));
        }

        public XmlElement GetCourseTeachers(IEnumerable<string> fieldNameList)
        {
            return SmartSchool.Feature.Course.CourseBulkProcess.GetCourseTeachers(fieldNameList);
        }
    }
}
