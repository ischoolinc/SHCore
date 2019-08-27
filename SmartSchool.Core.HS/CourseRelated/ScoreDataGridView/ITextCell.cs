using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartSchool.CourseRelated.ScoreDataGridView
{
    interface ITextCell:ICell
    {
    }

    class TextCell : ITextCell
    {
        private string _oldValue;
        private string _nowValue;
  

        public TextCell(string value)
        {
            _nowValue = value;
            _oldValue = value;
        }


        public string GetValue()
        {
            return _nowValue;
        }

        public void SetValue(string value)
        {
            _nowValue = value;
        }

        public string DefaultValue
        {
            get { return _oldValue; }
        }

        protected void OnInitialized(string text)
        {
            _oldValue = text;
            _nowValue = text;
            
        }

        public virtual bool IsDirty
        {
            get { return _oldValue != _nowValue; }
        }

        public virtual void Reset()
        {
            _nowValue = _oldValue;
        }
    }
}
