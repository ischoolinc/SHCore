using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using SmartSchool.Feature.Class;
using SmartSchool.Feature.Teacher;
using FISCA.DSAUtil;
using SmartSchool.Feature.ExamTemplate;
using SmartSchool.ImportSupport;

namespace SmartSchool.ClassRelated.RibbonBars.Import
{
    internal class ImportDataAccess : IDataAccess
    {
        public XmlElement GetImportFieldList()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Properties.Resources.SH_Cls_ImportFieldList);
            return doc.DocumentElement;
            //return SmartSchool.Feature.Class.ClassBulkProcess.GetImportFieldList();
        }

        public XmlElement GetValidateFieldRule()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Properties.Resources.SH_Cls_FieldValidationRule);
            return doc.DocumentElement;
            //return SmartSchool.Feature.Class.ClassBulkProcess.GetValidateFieldRule();
        }

        public XmlElement GetUniqueFieldData()
        {
            return SmartSchool.Feature.Class.ClassBulkProcess.GetUniqueFieldData();
        }

        public XmlElement GetShiftCheckList(params string[] fieldNameList)
        {
            return SmartSchool.Feature.Class.ClassBulkProcess.GetShiftCheckList(fieldNameList);
        }

        public void InsertImportData(XmlElement data)
        {
            SmartSchool.Feature.Class.ClassBulkProcess.InsertImportData(data);
        }

        public void UpdateImportData(XmlElement data)
        {
            SmartSchool.Feature.Class.ClassBulkProcess.UpdateImportData(data);
        }
    }
}
