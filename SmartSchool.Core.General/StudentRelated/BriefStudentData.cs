using System;
using System.Collections.Generic;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.Common;
using SmartSchool.TagManage;

namespace SmartSchool.StudentRelated
{
    public class BriefStudentData : IComparable<BriefStudentData>
    {
        private readonly string _ID;
        private readonly string _Status;
        private readonly string _SeatNo;
        private readonly string _Name;
        private readonly string _StudentNumber;
        private readonly string _Gender;
        private readonly string _IDNumber;
        private readonly string _PermanentPhone;
        private readonly string _ContactPhone;
        private readonly string _RefClassID;
        private readonly string _Birthday;
        private readonly string _OverrideDepartment;
        private readonly string _LeaveSchoolYear;
        private readonly string _LeaveReason;
        private readonly string _LeaveDepartment;
        private readonly string _LeaveClassName;
        private Dictionary<int, TagInfo> _Tags;

        public string ID { get { return _ID; } }
        public string Status { get { return _Status; } }
        public string SeatNo { get { return _SeatNo; } }
        public string Name { get { return _Name; } }
        public string StudentNumber { get { return _StudentNumber; } }
        public string Gender { get { return _Gender; } }
        public string IDNumber { get { return _IDNumber; } }
        public string PermanentPhone { get { return _PermanentPhone; } }
        public string ContactPhone { get { return _ContactPhone; } }
        public string Birthday { get { return _Birthday; } }
        public string ChineseBirthday { get { return _Birthday; } }
        internal string NonCheckedRefClassID { get { return _RefClassID; } }
        public string RefClassID { get { return ( _RefClassID == "" || ClassRelated.Class.Instance.Items[_RefClassID] == null ) ? "" : _RefClassID; } }
        //public string RefGraduationPlanID { get { return RefClassID == "" ? "" : ClassRelated.Class.Instance.Items[_RefClassID].RefGraduationPlanID; } }
        //public string RefScoreCalcRuleID { get { return RefClassID == "" ? "" : ClassRelated.Class.Instance.Items[_RefClassID].RefScoreCalcRuleID; } }
        public string ClassName { get { return ( _RefClassID == "" || ClassRelated.Class.Instance.Items[_RefClassID] == null ) ? "" : ClassRelated.Class.Instance.Items[_RefClassID].ClassName; } }
        public string GradeYear { get { return ( _RefClassID == "" || ClassRelated.Class.Instance.Items[_RefClassID] == null ) ? "" : ClassRelated.Class.Instance.Items[_RefClassID].GradeYear; } }
        public string Department { get { return _OverrideDepartment == "" ? ( ( _RefClassID == "" || ClassRelated.Class.Instance.Items[_RefClassID] == null ) ? "" : ClassRelated.Class.Instance.Items[_RefClassID].Department ) : _OverrideDepartment; } }
        //public string GraduationPlanName { get { return RefClassID == "" ? "" : ClassRelated.Class.Instance.Items[_RefClassID].GraduationPlanName; } }
        //public GraduationPlanInfo GraduationPlanInfo { get { return RefClassID == "" ? null : ClassRelated.Class.Instance.Items[_RefClassID].GraduationPlanInfo; } }
        //public ScoreCalcRuleInfo ScoreCalcRuleInfo { get { return RefClassID == "" ? null : ClassRelated.Class.Instance.Items[_RefClassID].ScoreCalcRuleInfo; } }
        public string LeaveSchoolYear { get { return _LeaveSchoolYear; } }
        public string LeaveReason { get { return _LeaveReason; } }
        public string LeaveDepartment { get { return _LeaveDepartment; } }
        public string LeaveClassName { get { return _LeaveClassName; } }
        public ReadOnlyCollection<int, TagInfo> Tags { get { return new ReadOnlyCollection<int, TagInfo>(_Tags); } }

        /// <summary>
        /// 在校學生
        /// </summary>
        public bool IsNormal { get { return ( _Status == "一般" || _Status == "延修" || _Status == "輟學" ); } }
        /// <summary>
        /// 延修生
        /// </summary>
        public bool IsExtending { get { return ( _Status == "延修" ); } }
        /// <summary>
        /// 休學生
        /// </summary>
        public bool IsOnLeave { get { return ( _Status == "休學" ); } }
        /// <summary>
        /// 已刪除學生
        /// </summary>
        public bool IsDeleted { get { return ( _Status == "刪除" ); } }
        /// <summary>
        /// 畢業或離校生
        /// </summary>
        public bool IsGraduated { get { return ( _Status == "畢業或離校" ); } }
        /// <summary>
        /// 輟學生
        /// </summary>
        public bool IsDiscontinued { get { return ( _Status == "輟學" ); } }


        internal BriefStudentData(XmlElement element)
        {
            _ID = element.GetAttribute("ID");
            
            // Initialize fields to default
            _Status = "";
            _SeatNo = "";
            _Name = "";
            _StudentNumber = "";
            _Gender = "";
            _IDNumber = "";
            _PermanentPhone = "";
            _ContactPhone = "";
            _RefClassID = "";
            _Birthday = "";
            _OverrideDepartment = "";
            _LeaveSchoolYear = "";
            _LeaveReason = "";
            _LeaveDepartment = "";
            _LeaveClassName = "";
            _Tags = new Dictionary<int, TagInfo>();

            foreach (XmlNode node in element.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element) continue;

                switch (node.Name)
                {
                    case "Status":
                        _Status = node.InnerText;
                        break;
                    case "SeatNo":
                        _SeatNo = node.InnerText;
                        break;
                    case "Name":
                        _Name = node.InnerText;
                        break;
                    case "StudentNumber":
                        _StudentNumber = node.InnerText;
                        break;
                    case "Gender":
                        _Gender = node.InnerText;
                        break;
                    case "IDNumber":
                        _IDNumber = node.InnerText;
                        break;
                    case "PermanentPhone":
                        _PermanentPhone = node.InnerText;
                        break;
                    case "ContactPhone":
                        _ContactPhone = node.InnerText;
                        break;
                    case "RefClassID":
                        _RefClassID = node.InnerText;
                        break;
                    case "Birthdate":
                        _Birthday = node.InnerText;
                        break;
                    case "OverrideDeptName":
                        _OverrideDepartment = node.InnerText;
                        break;
                    case "LeaveInfo":
                         // LeaveInfo/LeaveInfo structure
                         foreach(XmlNode inner in node.ChildNodes)
                         {
                             if (inner.Name == "LeaveInfo" && inner is XmlElement)
                             {
                                 XmlElement li = (XmlElement)inner;
                                 _LeaveSchoolYear = li.GetAttribute("SchoolYear");
                                 _LeaveReason = li.GetAttribute("Reason");
                                 _LeaveDepartment = li.GetAttribute("Department");
                                 _LeaveClassName = li.GetAttribute("ClassName");
                             }
                         }
                        break;
                    case "Tags":
                        foreach ( XmlNode tagNode in node.ChildNodes )
                        {
                            if (tagNode.Name == "Tag" && tagNode is XmlElement)
                            {
                                XmlElement tagElement = (XmlElement)tagNode;
                                string id = tagElement.GetAttribute("ID");
                                int key = 0;
                                if ( int.TryParse(id, out key) )
                                {
                                    if (!_Tags.ContainsKey(key))
                                    {
                                        string color = tagElement.GetAttribute("Color");
                                        string name = tagElement.GetAttribute("Name");
                                        string prefix = tagElement.GetAttribute("Prefix");
                                        _Tags.Add(key, new TagInfo(key, prefix, name, color));
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }


        #region IComparable<BriefStudentData> 成員

        public int CompareTo(BriefStudentData other)
        {
            if ( !SmartSchool.ClassRelated.Class.Instance.Loaded )
                return 0;
            if ( this.RefClassID == other.RefClassID )
            {
                if ( this.RefClassID != "" )
                {
                    int seatNo1, seatNo2;
                    int.TryParse(this.SeatNo, out seatNo1);
                    int.TryParse(other.SeatNo, out seatNo2);
                    return seatNo1.CompareTo(seatNo2);
                }
                else
                {
                    return this.StudentNumber.CompareTo(other.StudentNumber);
                }
            }
            else
            {
                if ( this.RefClassID == "" ) return -1;
                if ( other.RefClassID == "" ) return 1;
                return ClassRelated.Class.Instance.Items[this.RefClassID].CompareTo(ClassRelated.Class.Instance.Items[other.RefClassID]);
            }
        }

        #endregion
    }
}
