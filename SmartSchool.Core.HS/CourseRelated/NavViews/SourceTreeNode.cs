using System;
using System.Collections.Generic;

using System.Text;

namespace SmartSchool.CourseRelated.NavViews
{
    class SourceTreeNode : DevComponents.AdvTree.Node
    {
        public SourceTreeNode()
        {
            AutoCount = true;
            CreditCount = false;
        }
        public bool AutoCount { get; set; }
        public bool CreditCount { get; set; }
        private string _Text = "";
        public new string Text
        {
            get { return _Text; }
            set
            {
                _Text = value;
                UpdateText();
            }
        }
        private List<string> _Courses = new List<string>();
        public List<string> Courses { get { return _Courses; } }
        public void UpdateText()
        {
            if ( AutoCount && CreditCount )
            {
                decimal? creditCount = 0;
                foreach ( var item in _Courses )
                {
                    creditCount += Course.Instance[item].Credit;
                }
                base.Text = _Text + "{" + creditCount + "學分}(" + _Courses.Count + ")";
            }
            else if ( AutoCount )
            {
                base.Text = _Text + "(" + _Courses.Count + ")";
            }
            else if ( CreditCount )
            {
                decimal? creditCount = 0;
                foreach ( var item in _Courses )
                {
                    creditCount += Course.Instance[item].Credit;
                }
                base.Text = _Text + "(共" + creditCount + "學分)";
            }
            else
                base.Text = _Text;
        }
    }
}
