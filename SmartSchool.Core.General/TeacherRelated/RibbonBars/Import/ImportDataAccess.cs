using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using SmartSchool.Feature.Class;
using SmartSchool.Feature.Teacher;
using FISCA.DSAUtil;
using SmartSchool.Feature.ExamTemplate;
using SmartSchool.ImportSupport;

namespace SmartSchool.TeacherRelated.RibbonBars.Import
{
    internal class ImportDataAccess : IDataAccess
    {
        public XmlElement GetImportFieldList()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Properties.Resources.SH_T_ImportFieldList);
            return doc.DocumentElement;
            //return SmartSchool.Feature.Teacher.TeacherBulkProcess.GetImportFieldList();
        }

        public XmlElement GetValidateFieldRule()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Properties.Resources.SH_T_FieldValidationRule);
            return doc.DocumentElement;
            //return SmartSchool.Feature.Teacher.TeacherBulkProcess.GetFieldValidationRule();
        }

        public XmlElement GetUniqueFieldData()
        {
            return SmartSchool.Feature.Teacher.TeacherBulkProcess.GetUniqueFieldData();
        }

        public XmlElement GetShiftCheckList(params string[] fieldNameList)
        {
            return SmartSchool.Feature.Teacher.TeacherBulkProcess.GetShiftCheckList(fieldNameList);
        }

        public void InsertImportData(XmlElement data)
        {
            SmartSchool.Feature.Teacher.TeacherBulkProcess.InsertImportTeacher(data);
        }

        public void UpdateImportData(XmlElement data)
        {
            SmartSchool.Feature.Teacher.TeacherBulkProcess.UpdateImportTeacher(data);
        }
    }
}
