using System;
using System.Collections.Generic;
using System.Text;
using DocValidate;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.CourseRelated.RibbonBars.Import;
using SmartSchool.ImportSupport.Validators;
using SmartSchool.ImportSupport;

namespace SmartSchool.CourseRelated.RibbonBars.Import
{
    internal class TeacherDuplicateRowValidator : IRowVaildator
    {
        private bool _activate_validator;
        private List<string> _teacher_fields;
        private List<string> _selected_fields;
        private ImportMode _import_mode;
        private ConditionKeySet _course_teachers;
        private ImportDataAccess _data_source;

        public TeacherDuplicateRowValidator(WizardContext context)
        {
            _import_mode = context.CurrentMode;
            _data_source = context.DataSource as ImportDataAccess;
            _course_teachers = new ConditionKeySet(context.IdentifyField);
            _selected_fields = context.SelectedFields;
        }

        #region IRowVaildator 成員

        public void InitFromXMLNode(XmlElement XmlNode)
        {
            _activate_validator = false;
            _teacher_fields = new List<string>();
            foreach (XmlElement each in XmlNode.SelectNodes("ActivatorField"))
            {
                string fieldName = each.InnerText;
                if (_selected_fields.Contains(fieldName))
                {
                    _teacher_fields.Add(fieldName);
                    _activate_validator = true;
                }
            }

            if (!_activate_validator) return;

            if (_import_mode == ImportMode.Update)
            {
                XmlElement records = _data_source.GetCourseTeachers(_course_teachers.Condition.Fields.ToInternalNames());
                foreach (XmlElement each in records.SelectNodes("Record"))
                {
                    Record record = new Record(each);
                    _course_teachers.AddRecord(record);
                }
            }
        }

        public void InitFromXMLString(string XmlString)
        {
            throw new Exception("The method or operation is not implemented.(InitFromXMLString)");
        }

        public string Correct(IRowSource Value)
        {
            throw new Exception("The method or operation is not implemented.(Correct)");
        }

        public string KeyField()
        {
            return "<XmlContent>";
        }

        public string ToString(string Description)
        {
            DSXmlHelper result = new DSXmlHelper("Fields");

            foreach (string each in _teacher_fields)
            {
                XmlElement elmField = result.AddElement("Field");
                elmField.SetAttribute("Name", each);
                elmField.SetAttribute("Description", "一位教師不可擔任單一課程二次(含)以上授課教師。");
            }

            return result.GetRawXml();
        }

        public bool Validate(IRowSource Value)
        {
            if (!_activate_validator) return true;

            if (_import_mode == ImportMode.Insert)
            {
                return IsValid(_teacher_fields, Value);
            }
            else
            {
                if (_course_teachers.Contains(Value))
                    return IsValid(_teacher_fields, _course_teachers.GetRecord(Value), Value);
                else
                    return IsValid(_teacher_fields, Value);
            }
        }

        private static bool IsValid(List<string> fields, IRowSource rowSource)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var field in fields)
            {
                var value = (rowSource.GetFieldData(field) ?? string.Empty).Trim();
                if (value == string.Empty) continue;        // allow empty
                if (!seen.Add(value)) return false;         // duplicate non-empty value
            }
            return true;
        }

        private static Dictionary<string, string> _field_map;
        static TeacherDuplicateRowValidator()
        {
            _field_map = new Dictionary<string, string>();
            _field_map.Add("授課教師一", "Teacher1Name");
            _field_map.Add("授課教師二", "Teacher2Name");
            _field_map.Add("授課教師三", "Teacher3Name");
        }

        private static bool IsValid(List<string> fields, Record record, IRowSource rowSource)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);

            // Snapshot current DB values
            var teachers = new[]
            {
                (record[_field_map["授課教師一"]] ?? string.Empty).Trim(),
                (record[_field_map["授課教師二"]] ?? string.Empty).Trim(),
                (record[_field_map["授課教師三"]] ?? string.Empty).Trim()
            };

            // Overlay with incoming row values for any fields present in this import row
            foreach (var field in fields)
            {
                var value = (rowSource.GetFieldData(field) ?? string.Empty).Trim();
                if (field == "授課教師一") teachers[0] = value;
                else if (field == "授課教師二") teachers[1] = value;
                else if (field == "授課教師三") teachers[2] = value;
            }

            foreach (var name in teachers)
            {
                if (name == string.Empty) continue; // allow empty
                if (!seen.Add(name)) return false;  // duplicate detected
            }
            return true;
        }
        #endregion
    }
}