using System;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.ExceptionHandler;
using SmartSchool.Feature.Basic;

namespace SmartSchool
{
    public class SchoolConfig
    {
        private bool _loaded = false;
        private XmlElement _default = null;

        private DSXmlHelper _source;
        private DSXmlHelper Source
        {
            get { return _source; }
            set { _source = value; }
        }

        public XmlElement Content
        {
            get { return Source.BaseElement; }
        }

        public void Load()
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    _source = new DSXmlHelper(Config.GetSchoolConfig().GetElement("Content"));
                    _loaded = true;
                    if (_loaded)
                        break;
                }
                catch (Exception ex)
                {
                    CurrentUser user = CurrentUser.Instance;
                    BugReporter.ReportException(user.SystemName, user.SystemVersion, ex, false);
                }
            }
        }

        public void Save()
        {
            try
            {
                Config.SetSchoolConfig(Source.BaseElement);
            }
            catch (Exception ex)
            {
                CurrentUser user = CurrentUser.Instance;
                BugReporter.ReportException(user.SystemName, user.SystemVersion, ex, false);
            }
        }
    }
}
