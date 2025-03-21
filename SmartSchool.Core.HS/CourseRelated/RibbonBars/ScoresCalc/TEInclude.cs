﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.Feature.ExamTemplate;

namespace SmartSchool.CourseRelated.RibbonBars.ScoresCalc
{
    class TEInclude
    {
        private string _identity, _exam_id, _exam_template_id, _exam_name;
        private bool _input_required;
        private float _weight;

        // 使用評量群組
        private bool use_group;

        public TEInclude(XmlElement data)
        {
            DSXmlHelper obj = new DSXmlHelper(data);
            _identity = obj.GetText("@ID");
            _exam_id = obj.GetText("RefExamID");
            _exam_template_id = obj.GetText("ExamTemplateID");
            _input_required = (obj.GetText("InputRequired") == "是" ? true : false);
            _exam_name = obj.GetText("ExamName");
            use_group = (obj.GetText("Extension/Extension/UseGroup") == "是" ? true : false);

            _weight = 0;
            float.TryParse(obj.GetText("Weight"), out _weight);
        }

        public string Identity
        {
            get { return _identity; }
        }

        public string ExamId
        {
            get { return _exam_id; }
        }

        public string ExamTemplateId
        {
            get { return _exam_template_id; }
        }

        public bool InputRequired
        {
            get { return _input_required; }
        }

        public bool UseGroup
        {
            get { return use_group; }
        }

        public float Weight
        {
            get { return _weight; }
        }

        public string ExamName
        {
            get { return _exam_name; }
        }

        public static TEIncludeCollection GetTEIncludes()
        {
            XmlElement xmlRecords = QueryTemplate.GetIncludeExamList();

            TEIncludeCollection includes = new TEIncludeCollection();
            foreach (XmlElement each in xmlRecords.SelectNodes("IncludeExam"))
            {
                TEInclude include = new TEInclude(each);
                includes.Add(include.Identity, include);
            }

            return includes;
        }
    }

    class TEIncludeCollection : Dictionary<string, TEInclude>
    {
    }
}
