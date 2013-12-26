using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartSchool.Common;

namespace SmartSchool.StudentRelated.SourceProvider
{
    class AllStudentSourceProvider : DragDropTreeNode, ISourceProvider<BriefStudentData, SmartSchool.StudentRelated.Search.ISearchStudent>
    {
        public List<BriefStudentData> Source
        {
            get
            {
                return _Source;
            }
            set
            {
                _Source = value;
                this.Text = "所有學生(" + _Source.Count + ")";
                if (SourceChanged != null)
                    SourceChanged.Invoke(this, new EventArgs());
            }
        }

        private bool _DisplaySource;

        private List<BriefStudentData> _Source = new List<BriefStudentData>();

        private string _Text = "所有學生";

        new public string Text
        {
            get { return _Text; }
            set
            {
                _Text = value;
                base.Text = Text;
            }
        }

        public event EventHandler SourceChanged;

        public bool DisplaySource
        {
            get { return _DisplaySource; }
            set { _DisplaySource = value; }
        }

        public bool ImmediatelySearch { get { return true; } }

        private Search.SearchStudentInMetadata _SearchProvider;

        public Search.ISearchStudent SearchProvider
        {
            get
            {
                if (_SearchProvider == null)
                {
                    _SearchProvider = new SmartSchool.StudentRelated.Search.SearchStudentInMetadata();
                }
                _SearchProvider.Source = _Source;
                return _SearchProvider;
            }
        }

        public string SearchWatermark { get { return "在\"" + _Text + "\"中搜尋"; } }
    }
}
