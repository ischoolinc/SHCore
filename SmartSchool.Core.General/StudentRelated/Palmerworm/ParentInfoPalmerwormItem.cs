using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SmartSchool.StudentRelated.Palmerworm;
using FISCA.DSAUtil;
using SmartSchool.Common;
using SmartSchool.Feature.Basic;
using System.Xml;
using SmartSchool.Feature;
using SmartSchool.AccessControl;
using SmartSchool.StudentRelated.Palmerworm.Model;
using FISCA.Data;
using FISCA.LogAgent;
using System.Text.RegularExpressions;
//using Framework;
using Framework.DataChangeManage;
using FISCA.Presentation;
using SmartSchool;
using Framework;
//using Campus.Windows;

namespace SmartSchool.StudentRelated.Palmerworm
{
    [FeatureCode("Content0030")]
    internal partial class ParentInfoPalmerwormItem : PalmerwormItem
    {
        private string _fatherName;
        private string _fatherNationality;
        private string _fatherAlive;
        private string _fatherEduDegree;
        private string _fatherJob;
        private string _fatherPhone;
        private string _fatherIDNumber;
        private string _fatherBirthYear;
        private string _fatherBirthCountry;
        private string _fatherEmail;
        private string _fatherFromChina;
        private string _fatherFromForeign;
        private string _fatherIsUnemployed;

        private XmlNode _fatherOtherInfoNode;

        private string _motherName;
        private string _motherNationality;
        private string _motherAlive;
        private string _motherEduDegree;
        private string _motherJob;
        private string _motherPhone;
        private string _motherIDNumber;
        private string _motherBirthYear;
        private string _motherBirthCountry;
        private string _motherEmail;
        private string _motherFromChina;
        private string _motherFromForeign;
        private string _motherIsUnemployed;
        private XmlNode _motherOtherInfoNode;

        private string _guardianName;
        private string _guardianNationality;
        private string _guardianEduDegree;
        private string _guardianJob;
        private string _guardianRelationship;
        private string _guardianPhone;
        private string _guardianIDNumber;
        private string _guardianEmail;
        private XmlNode _guardianOtherInfoNode;

        private BackgroundWorker _getRelationshipBackgroundWorker;
        private BackgroundWorker _getJobBackgroundWorker;
        private BackgroundWorker _getEduDegreeBackgroundWorker;
        private BackgroundWorker _getNationalityBackgroundWorker;

        private DataValueManager _valueManager2 = new DataValueManager();

        //private DSResponse _relationshipList;

        private bool _isInitialized = false;

        //2020/07新增
        private ParentExtentionInfo ParentExtentionInfo;//抓資料庫抓出來的家長延伸資料
        private ParentExtentionInfo WrapParentExtentionInfo;//修改的家長延伸資料

        string birthCountryCheckPattern = @"^\d{3}$";
        string birthYearCheckpattern = @"^\d{4}$";

        private QueryHelper _Qp = new QueryHelper();
        ErrorProvider errorBrithCountry = new ErrorProvider();
        ErrorProvider errorBrithYear = new ErrorProvider();

        private ChangeListener _DataListener_Father;
        private ChangeListener _DataListener_Mother;
        private ChangeListener _DataListener_Guardian;

        public override object Clone()
        {
            return new ParentInfoPalmerwormItem();
        }
        public ParentInfoPalmerwormItem()
        {
            InitializeComponent();
            Title = "父母親及監護人資料";

            _getRelationshipBackgroundWorker = new BackgroundWorker();
            _getRelationshipBackgroundWorker.DoWork += new DoWorkEventHandler(_getRelationshipBackgroundWorker_DoWork);
            _getRelationshipBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_getRelationshipBackgroundWorker_RunWorkerCompleted);
            _getRelationshipBackgroundWorker.RunWorkerAsync();

            _getJobBackgroundWorker = new BackgroundWorker();
            _getJobBackgroundWorker.DoWork += new DoWorkEventHandler(_getJobBackgroundWorker_DoWork);
            _getJobBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_getJobBackgroundWorker_RunWorkerCompleted);
            _getJobBackgroundWorker.RunWorkerAsync();

            _getEduDegreeBackgroundWorker = new BackgroundWorker();
            _getEduDegreeBackgroundWorker.DoWork += new DoWorkEventHandler(_getEduDegreeBackgroundWorker_DoWork);
            _getEduDegreeBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_getEduDegreeBackgroundWorker_RunWorkerCompleted);
            _getEduDegreeBackgroundWorker.RunWorkerAsync();

            _getNationalityBackgroundWorker = new BackgroundWorker();
            _getNationalityBackgroundWorker.DoWork += new DoWorkEventHandler(_getNationalityBackgroundWorker_DoWork);
            _getNationalityBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_getNationalityBackgroundWorker_RunWorkerCompleted);
            _getNationalityBackgroundWorker.RunWorkerAsync();

            //2020/07新增
            _DataListener_Father = new ChangeListener();
            _DataListener_Guardian = new ChangeListener();
            _DataListener_Mother = new ChangeListener();
            _DataListener_Father.Add(new TextBoxSource(textBirthYear));
            _DataListener_Father.Add(new TextBoxSource(textBirthCountry));
            _DataListener_Father.Add(new TextBoxSource(textEmail));
            _DataListener_Father.Add(new TextBoxSource(textFromChina));
            _DataListener_Father.Add(new TextBoxSource(textFromForeign));
            _DataListener_Father.Add(new RadioButtonSource(rdbspuy));
            _DataListener_Father.Add(new RadioButtonSource(rdbspun));

            _DataListener_Mother.Add(new TextBoxSource(textBirthYear));
            _DataListener_Mother.Add(new TextBoxSource(textBirthCountry));
            _DataListener_Mother.Add(new TextBoxSource(textEmail));
            _DataListener_Mother.Add(new TextBoxSource(textFromChina));
            _DataListener_Mother.Add(new TextBoxSource(textFromForeign));
            _DataListener_Mother.Add(new RadioButtonSource(rdbspuy));
            _DataListener_Mother.Add(new RadioButtonSource(rdbspun));

            _DataListener_Guardian.Add(new TextBoxSource(textEmail));

        }

        private void DataListenerPause()
        {
            _DataListener_Father.SuspendListen();
            _DataListener_Mother.SuspendListen();
            _DataListener_Guardian.SuspendListen();
        }


        void _getNationalityBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //DSXmlHelper helper = (e.Result as DSResponse).GetContent();
            //foreach (XmlNode node in helper.GetElements("Nationality"))
            //{
            //    cboNationality.Items.Add(new KeyValuePair<string, string>(node.InnerText, node.InnerText));
            //}
            // 國籍
            foreach (string str in Utility.GetNationalityList())
                cboNationality.Items.Add(new KeyValuePair<string, string>(str, str));
        }

        void _getNationalityBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //e.Result = Config.GetNationalityList();
        }

        void _getEduDegreeBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //DSXmlHelper helper = (e.Result as DSResponse).GetContent();

            //foreach (XmlNode node in helper.GetElements("EducationDegree"))
            //{
            //    cboEduDegree.Items.Add(new KeyValuePair<string, string>(node.InnerText, node.InnerText));
            //}

            // 學歷
            foreach (string str in Utility.GetEducationDegreeList())
                cboEduDegree.Items.Add(new KeyValuePair<string, string>(str, str));
        }

        void _getEduDegreeBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //e.Result = Config.GetEduDegreeList();
        }

        void _getJobBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //DSXmlHelper helper = (e.Result as DSResponse).GetContent();

            //foreach (XmlNode node in helper.GetElements("Job"))
            //{
            //    cboJob.Items.Add(new KeyValuePair<string, string>(node.InnerText, node.InnerText));
            //}

            // 職業
            foreach (string str in Utility.GetJobList())
                cboJob.Items.Add(new KeyValuePair<string, string>(str, str));
        }

        void _getJobBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //e.Result = Config.GetJobList();
        }

        void _getRelationshipBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //DSXmlHelper helper = (e.Result as DSResponse).GetContent();

            //foreach (XmlNode node in helper.GetElements("Relationship"))
            //{
            //    cboRelationship.Items.Add(new KeyValuePair<string, string>(node.InnerText, node.InnerText));
            //}

            // 關係
            foreach (string str in Utility.GetRelationshipList())
                cboRelationship.Items.Add(new KeyValuePair<string, string>(str, str));
        }

        void _getRelationshipBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //e.Result = Config.GetRelationship();
        }

        protected override object OnBackgroundWorkerWorking()
        {
            return QueryStudent.GetParentInfo(RunningID).GetContent();
        }

        protected override void OnBackgroundWorkerCompleted(object result)
        {
            Initialize();
            GetParentExtentionInfo();
            errorBrithCountry.Clear();
            errorBrithYear.Clear();
            DSXmlHelper helper = result as DSXmlHelper;
            _fatherAlive = helper.GetText("FatherLiving");
            _fatherOtherInfoNode = helper.GetElement("FatherOtherInfo");
            _fatherEduDegree = helper.GetText("FatherOtherInfo/FatherEducationDegree");
            _fatherJob = helper.GetText("FatherOtherInfo/FatherJob");
            //_fatherPhone = helper.GetText("FatherOtherInfo/FatherPhone");
            _fatherPhone = helper.GetText("FatherOtherInfo/Phone");
            _fatherName = helper.GetText("FatherName");
            _fatherNationality = helper.GetText("FatherNationality");
            _fatherIDNumber = helper.GetText("FatherIDNumber");
            

            _motherAlive = helper.GetText("MotherLiving");
            _motherOtherInfoNode = helper.GetElement("MotherOtherInfo");
            _motherEduDegree = helper.GetText("MotherOtherInfo/MotherEducationDegree");
            _motherJob = helper.GetText("MotherOtherInfo/MotherJob");
            //_motherPhone = helper.GetText("MotherOtherInfo/MotherPhone");
            _motherPhone = helper.GetText("MotherOtherInfo/Phone");
            _motherName = helper.GetText("MotherName");
            _motherNationality = helper.GetText("MotherNationality");
            _motherIDNumber = helper.GetText("MotherIDNumber");

            _guardianOtherInfoNode = helper.GetElement("CustodianOtherInfo");
            _guardianEduDegree = helper.GetText("CustodianOtherInfo/EducationDegree");
            _guardianJob = helper.GetText("CustodianOtherInfo/Job");
            //_guardianPhone = helper.GetText("CustodianOtherInfo/Phone");
            _guardianPhone = helper.GetText("CustodianOtherInfo/Phone");
            _guardianName = helper.GetText("CustodianName");
            _guardianNationality = helper.GetText("CustodianNationality");
            _guardianRelationship = helper.GetText("CustodianRelationship");
            _guardianIDNumber = helper.GetText("CustodianIDNumber");

            _fatherBirthYear = this.ParentExtentionInfo.FatherBirthYear;
            _fatherBirthCountry = this.ParentExtentionInfo.FatherBirthCountry;
            _fatherEmail = this.ParentExtentionInfo.FatherEmail;
            _fatherFromChina = this.ParentExtentionInfo.FatherFromChina;
            _fatherFromForeign = this.ParentExtentionInfo.FatherFromForeign;
            if(this.ParentExtentionInfo.FatherIsUnemployed == "true")
            {
                _fatherIsUnemployed = "是";
            }else if(this.ParentExtentionInfo.FatherIsUnemployed == "false")
            {
                _fatherIsUnemployed = "否";
            }
            else
            {
                _fatherIsUnemployed = "";
            }
           
            _motherBirthYear = this.ParentExtentionInfo.MotherBirthYear;
            _motherBirthCountry = this.ParentExtentionInfo.MotherBirthCountry;
            _motherEmail = this.ParentExtentionInfo.MotherEmail;
            _motherFromChina = this.ParentExtentionInfo.MotherFromChina;
            _motherFromForeign = this.ParentExtentionInfo.MotherFromForeign;

            if (this.ParentExtentionInfo.MotherIsUnemployed == "true")
            {
                _motherIsUnemployed = "是";
            }
            else if (this.ParentExtentionInfo.MotherIsUnemployed == "false")
            {
                _motherIsUnemployed = "否";
            }
            else
            {
                _motherIsUnemployed = "";
            }



            _guardianEmail = this.ParentExtentionInfo.GuardianEmail;

            _valueManager.AddValue("FatherLiving", _fatherAlive, "父存歿");
            _valueManager.AddValue("FatherEducationDegree", _fatherEduDegree, "父最高學歷");
            _valueManager.AddValue("FatherJob", _fatherJob, "父職業");
            _valueManager.AddValue("FatherName", _fatherName, "父姓名");
            _valueManager.AddValue("FatherNationality", _fatherNationality, "父國籍");
            _valueManager.AddValue("FatherIDNumber", _fatherIDNumber, "父身分證號");
            _valueManager.AddValue("FatherPhone", _fatherPhone, "父電話");

            _valueManager.AddValue("MotherLiving", _motherAlive, "母存歿");
            _valueManager.AddValue("MotherEducationDegree", _motherEduDegree, "母最高學歷");
            _valueManager.AddValue("MotherJob", _motherJob, "母職業");
            _valueManager.AddValue("MotherName", _motherName, "母姓名");
            _valueManager.AddValue("MotherNationality", _motherNationality, "母國籍");
            _valueManager.AddValue("MotherIDNumber", _motherIDNumber, "母身分證號");
            _valueManager.AddValue("MotherPhone", _motherPhone, "母電話");

            _valueManager.AddValue("CustodianRelationship", _guardianRelationship, "監護人稱謂");
            _valueManager.AddValue("EducationDegree", _guardianEduDegree, "監護人最高學歷");
            _valueManager.AddValue("Job", _guardianJob, "監護人職業");
            _valueManager.AddValue("CustodianName", _guardianName, "監護人姓名");
            _valueManager.AddValue("CustodianNationality", _guardianNationality, "監護人國籍");
            _valueManager.AddValue("CustodianIDNumber", _guardianIDNumber, "監護人身分證號");
            _valueManager.AddValue("CustodianPhone", _guardianPhone, "監護人電話");

            _valueManager.AddValue("GuardianEmail", _guardianEmail, "監護人Email");
            _valueManager.AddValue("FatherBirthYear", _fatherBirthYear, "父親出生年次");
            _valueManager.AddValue("FatherBirthCountry", _fatherBirthCountry, "父親原屬國家地區");
            _valueManager.AddValue("FatherEmail", _fatherEmail, "父親Email");
            _valueManager.AddValue("FatherFromChina", _fatherFromChina, "父親為大陸配偶_省份");
            _valueManager.AddValue("FatherFromForeign", _fatherFromForeign, "父親為外籍配偶_國籍");
            _valueManager.AddValue("FatherIsUnemployed", _fatherIsUnemployed, "父為失業勞工");
            _valueManager.AddValue("MotherBirthYear", _motherBirthYear, "母親出生年次");
            _valueManager.AddValue("MotherBirthCountry", _motherBirthCountry, "母親原屬國家地區");
            _valueManager.AddValue("MotherEmail", _motherEmail, "母親Email");
            _valueManager.AddValue("MotherFromChina", _motherFromChina, "母親為大陸配偶_省份");
            _valueManager.AddValue("MotherFromForeign", _motherFromForeign, "母親為外籍配偶_國籍");
            _valueManager.AddValue("MotherIsUnemployed", _motherIsUnemployed, "母為失業勞工");

            LoadGuardian();
        }

        private void Initialize()
        {
            if (_isInitialized) return;

            //載入稱謂
            //DSResponse dsrsp = Config.GetRelationship();
            //DSXmlHelper helper = dsrsp.GetContent();
            //DSXmlHelper helper = _relationshipList.GetContent();
            //DSXmlHelper helper;
            KeyValuePair<string, string> kvp = new KeyValuePair<string, string>("", "請選擇");
            cboRelationship.Items.Add(kvp);
            //foreach (XmlNode node in helper.GetElements("Relationship"))
            //{
            //    kvp = new KeyValuePair<string, string>(node.InnerText, node.InnerText);
            //    cboRelationship.Items.Add(kvp);
            //}
            cboRelationship.DisplayMember = "Value";
            cboRelationship.ValueMember = "Key";

            //載入職業
            //DSResponse dsrsp = Config.GetJobList();
            //helper = dsrsp.GetContent();
            //kvp = new KeyValuePair<string, string>("", "請選擇");
            cboJob.Items.Add(kvp);
            //foreach (XmlNode node in helper.GetElements("Job"))
            //{
            //    kvp = new KeyValuePair<string, string>(node.InnerText, node.InnerText);
            //    cboJob.Items.Add(kvp);
            //}
            cboJob.DisplayMember = "Value";
            cboJob.ValueMember = "Key";


            //載入學歷
            //dsrsp = Config.GetEduDegreeList();
            //helper = dsrsp.GetContent();
            //kvp = new KeyValuePair<string, string>("", "請選擇");
            cboEduDegree.Items.Add(kvp);
            //foreach (XmlNode node in helper.GetElements("EducationDegree"))
            //{
            //    kvp = new KeyValuePair<string, string>(node.InnerText, node.InnerText);
            //    cboEduDegree.Items.Add(kvp);
            //}
            cboEduDegree.DisplayMember = "Value";
            cboEduDegree.ValueMember = "Key";


            //載入國籍
            //dsrsp = Config.GetNationalityList();
            //helper = dsrsp.GetContent();
            //kvp = new KeyValuePair<string, string>("", "請選擇");
            cboNationality.Items.Add(kvp);
            //foreach (XmlNode node in helper.GetElements("Nationality"))
            //{
            //    kvp = new KeyValuePair<string, string>(node.InnerText, node.InnerText);
            //    cboNationality.Items.Add(kvp);
            //}
            cboNationality.DisplayMember = "Value";
            cboNationality.ValueMember = "Key";



            //載入存殁
            kvp = new KeyValuePair<string, string>("存", "存");
            cboAlive.Items.Add(kvp);
            kvp = new KeyValuePair<string, string>("歿", "歿");
            cboAlive.Items.Add(kvp);
            cboAlive.DisplayMember = "Value";
            cboAlive.ValueMember = "Key";

            _isInitialized = true;
        }

        public override void Save()
        {
            DSXmlHelper helper = new DSXmlHelper("UpdateParentInfoRequest");
            helper.AddElement("Student");
            helper.AddElement("Student", "Field");

            Dictionary<string, string> changes = _valueManager.GetDirtyItems();
            foreach (string key in changes.Keys)
            {
                if (!key.EndsWith("EducationDegree") && !key.EndsWith("Job") && !key.EndsWith("Phone") && !key.EndsWith("BirthYear") && !key.EndsWith("BirthCountry") && !key.EndsWith("Email") && !key.EndsWith("FromChina") && !key.EndsWith("FromForeign") && !key.EndsWith("IsUnemployed"))
                    helper.AddElement("Student/Field", key, changes[key]);
            }

            if (_valueManager.IsDirtyItem("FatherEducationDegree"))
            {
                string info = GetEduDegreeResponse(_fatherOtherInfoNode, "FatherEducationDegree", _fatherEduDegree, "FatherOtherInfo");
                _fatherOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            if (_valueManager.IsDirtyItem("MotherEducationDegree"))
            {
                string info = GetEduDegreeResponse(_motherOtherInfoNode, "MotherEducationDegree", _motherEduDegree, "MotherOtherInfo");
                _motherOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            if (_valueManager.IsDirtyItem("EducationDegree"))
            {
                string info = GetEduDegreeResponse(_guardianOtherInfoNode, "EducationDegree", _guardianEduDegree, "CustodianOtherInfo");
                _guardianOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            //if (_valueManager.IsDirtyItem("CustodianRelationship"))
            //{
            //    string info = GetEduDegreeResponse(_guardianOtherInfoNode, "CustodianRelationship", _guardianRelationship, "CustodianOtherInfo");
            //    _guardianOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            //}

            if (_valueManager.IsDirtyItem("FatherJob"))
            {
                string info = GetEduDegreeResponse(_fatherOtherInfoNode, "FatherJob", _fatherJob, "FatherOtherInfo");
                _fatherOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            if (_valueManager.IsDirtyItem("MotherJob"))
            {
                string info = GetEduDegreeResponse(_motherOtherInfoNode, "MotherJob", _motherJob, "MotherOtherInfo");
                _motherOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            if (_valueManager.IsDirtyItem("Job"))
            {
                string info = GetEduDegreeResponse(_guardianOtherInfoNode, "Job", _guardianJob, "CustodianOtherInfo");
                _guardianOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            //FatherPhone
            if (_valueManager.IsDirtyItem("FatherPhone"))
            {
                string info = GetEduDegreeResponse(_fatherOtherInfoNode, "Phone", _fatherPhone, "FatherOtherInfo");
                _fatherOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }
            //2020/07 新增
            //MotherPhone
            if (_valueManager.IsDirtyItem("MotherPhone"))
            {
                string info = GetEduDegreeResponse(_motherOtherInfoNode, "Phone", _motherPhone, "MotherOtherInfo");
                _motherOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            //CustodianPhone
            if (_valueManager.IsDirtyItem("CustodianPhone"))
            {
                string info = GetEduDegreeResponse(_guardianOtherInfoNode, "Phone", _guardianPhone, "CustodianOtherInfo");
                _guardianOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            //FatherEmail
            if (_valueManager.IsDirtyItem("FatherEmail"))
            {
                string info = GetEduDegreeResponse(_fatherOtherInfoNode, "FatherEmail", _fatherEmail, "FatherOtherInfo");
                _fatherOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            //MotherEmail
            if (_valueManager.IsDirtyItem("MotherEmail"))
            {
                string info = GetEduDegreeResponse(_motherOtherInfoNode, "MotherEmail", _motherEmail, "MotherOtherInfo");
                _motherOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            //GuardianEmail
            if (_valueManager.IsDirtyItem("GuardianEmail"))
            {
                string info = GetEduDegreeResponse(_guardianOtherInfoNode, "GuardianEmail", _guardianEmail, "CustodianOtherInfo");
                _guardianOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            //FatherBirthYear
            if (_valueManager.IsDirtyItem("FatherBirthYear"))
            {
                string info = GetEduDegreeResponse(_fatherOtherInfoNode, "FatherBirthYear", _fatherBirthYear, "FatherOtherInfo");
                _fatherOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            //MotherBirthYear
            if (_valueManager.IsDirtyItem("MotherBirthYear"))
            {
                string info = GetEduDegreeResponse(_motherOtherInfoNode, "MotherBirthYear", _motherBirthYear, "MotherOtherInfo");
                _motherOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            //FatherBirthCountry
            if (_valueManager.IsDirtyItem("FatherBirthCountry"))
            {
                string info = GetEduDegreeResponse(_fatherOtherInfoNode, "FatherBirthCountry", _fatherBirthCountry, "FatherOtherInfo");
                _fatherOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            //MotherBirthCountry
            if (_valueManager.IsDirtyItem("MotherBirthCountry"))
            {
                string info = GetEduDegreeResponse(_motherOtherInfoNode, "MotherBirthCountry", _motherBirthCountry, "MotherOtherInfo");
                _motherOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            //FatherFromChina
            if (_valueManager.IsDirtyItem("FatherFromChina"))
            {
                string info = GetEduDegreeResponse(_fatherOtherInfoNode, "FatherFromChina", _fatherFromChina, "FatherOtherInfo");
                _fatherOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            //MotherFromChina
            if (_valueManager.IsDirtyItem("MotherFromChina"))
            {
                string info = GetEduDegreeResponse(_motherOtherInfoNode, "MotherFromChina", _motherFromChina, "MotherOtherInfo");
                _motherOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            //FatherFromForeign
            if (_valueManager.IsDirtyItem("FatherFromForeign"))
            {
                string info = GetEduDegreeResponse(_fatherOtherInfoNode, "FatherFromForeign", _fatherFromForeign, "FatherOtherInfo");
                _fatherOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            //MotherFromForeign
            if (_valueManager.IsDirtyItem("MotherFromForeign"))
            {
                string info = GetEduDegreeResponse(_motherOtherInfoNode, "MotherFromForeign", _motherFromForeign, "MotherOtherInfo");
                _motherOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            //FatherIsUnemployed
            if (_valueManager.IsDirtyItem("FatherIsUnemployed"))
            {
                string info = GetEduDegreeResponse(_fatherOtherInfoNode, "FatherIsUnemployed", _fatherIsUnemployed, "FatherOtherInfo");
                _fatherOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }

            //MotherIsUnemployed
            if (_valueManager.IsDirtyItem("MotherIsUnemployed"))
            {
                string info = GetEduDegreeResponse(_motherOtherInfoNode, "MotherIsUnemployed", _motherIsUnemployed, "MotherOtherInfo");
                _motherOtherInfoNode = DSXmlHelper.LoadXml(info, true);
            }


            if (_valueManager.IsDirtyItem("FatherJob") || _valueManager.IsDirtyItem("FatherEducationDegree") || _valueManager.IsDirtyItem("FatherPhone") || _valueManager.IsDirtyItem("FatherBirthYear") || _valueManager.IsDirtyItem("FatherBirthCountry") || _valueManager.IsDirtyItem("FatherEmail") || _valueManager.IsDirtyItem("FatherFromChina") || _valueManager.IsDirtyItem("FatherFromForeign") || _valueManager.IsDirtyItem("FatherIsUnemployed"))
            {
                helper.AddElement("Student/Field", "FatherOtherInfo");
                helper.AddCDataSection("Student/Field/FatherOtherInfo", _fatherOtherInfoNode.OuterXml);
            }

            if (_valueManager.IsDirtyItem("MotherJob") || _valueManager.IsDirtyItem("MotherEducationDegree") || _valueManager.IsDirtyItem("MotherPhone") || _valueManager.IsDirtyItem("MotherBirthYear") || _valueManager.IsDirtyItem("MotherBirthCountry") || _valueManager.IsDirtyItem("MotherEmail") || _valueManager.IsDirtyItem("MotherFromChina") || _valueManager.IsDirtyItem("MotherFromForeign") || _valueManager.IsDirtyItem("MotherIsUnemployed"))
            {
                helper.AddElement("Student/Field", "MotherOtherInfo");
                helper.AddCDataSection("Student/Field/MotherOtherInfo", _motherOtherInfoNode.OuterXml);
            }

            if (_valueManager.IsDirtyItem("Job") || _valueManager.IsDirtyItem("EducationDegree") || _valueManager.IsDirtyItem("CustodianPhone") || _valueManager.IsDirtyItem("GuardianEmail"))
            {
                helper.AddElement("Student/Field", "CustodianOtherInfo");
                helper.AddCDataSection("Student/Field/CustodianOtherInfo", _guardianOtherInfoNode.OuterXml);
            }

            helper.AddElement("Student", "Condition");
            helper.AddElement("Student/Condition", "ID", RunningID);
            EditStudent.Update(new DSRequest(helper));

            LogUtility.LogChange(_valueManager, RunningID, GetStudentName());

            // 辨識資料
            if (btnParentType.Text == "父親")
            {
                if (this.textBirthCountry.Text != "" && !Regex.IsMatch(this.textBirthCountry.Text, birthCountryCheckPattern))
                {
                    if (Regex.IsMatch(this.textBirthYear.Text, birthYearCheckpattern) || this.textBirthCountry.Text == "" || this.textBirthYear.Text == "")
                    {
                        errorBrithYear.Clear();
                        errorBrithCountry.SetError(this.textBirthCountry, "請輸入3碼的國籍及出生代碼");
                        return;
                    }
                    else
                    {
                        errorBrithYear.SetError(this.textBirthYear, "請輸入西元紀年(4碼)");
                        errorBrithCountry.SetError(this.textBirthCountry, "請輸入3碼的國籍及出生代碼");
                        return;
                    }
                }
                else
                {
                    if (this.textBirthYear.Text != "" && !Regex.IsMatch(this.textBirthYear.Text, birthYearCheckpattern))
                    {
                        errorBrithCountry.Clear();
                        errorBrithYear.SetError(this.textBirthYear, "請輸入西元紀年(4碼)");
                        return;
                    }

                }
            }

            if (btnParentType.Text == "母親")
            {
                if (this.textBirthCountry.Text != "" && !Regex.IsMatch(this.textBirthCountry.Text, birthCountryCheckPattern))
                {
                    if (Regex.IsMatch(this.textBirthYear.Text, birthYearCheckpattern) || this.textBirthCountry.Text == "" || this.textBirthYear.Text == "")
                    {
                        errorBrithYear.Clear();
                        errorBrithCountry.SetError(this.textBirthCountry, "請輸入3碼的國籍及出生代碼");
                        return;
                    }
                    else
                    {
                        errorBrithYear.SetError(this.textBirthYear, "請輸入西元紀年(4碼)");
                        errorBrithCountry.SetError(this.textBirthCountry, "請輸入3碼的國籍及出生代碼");
                        return;
                    }
                }
                else
                {
                    if (this.textBirthYear.Text != "" && !Regex.IsMatch(this.textBirthYear.Text, birthYearCheckpattern))
                    {
                        errorBrithCountry.Clear();
                        errorBrithYear.SetError(this.textBirthYear, "請輸入西元紀年(4碼)");
                        return;
                    }

                }
            }

            //回存資料
            WrapFromValue();
            ParentExtenstionUpdate();

            SaveButtonVisible = false;
        }

        private string GetStudentName()
        {
            try
            {
                BriefStudentData student = Student.Instance.Items[RunningID];
                return student.Name;
            }
            catch (Exception)
            {
                return "<" + RunningID + ">";
            }
        }

        private void btnGuardian_Click(object sender, EventArgs e)
        {
            LoadGuardian();
        }

        private void btnFather_Click(object sender, EventArgs e)
        {
            DataListenerPause();
            btnGuardian.Enabled = true;
            btnFather.Enabled = false;
            btnMother.Enabled = true;

            cboAlive.Visible = true;
            lblAlive.Visible = true;
            cboRelationship.Visible = false;
            lblRelationship.Visible = false;

            btnParentType.Text = btnFather.Text;
            txtParentName.Text = _fatherName;
            txtIDNumber.Text = _fatherIDNumber;
            cboAlive.Text = _fatherAlive;
            cboEduDegree.Text = _fatherEduDegree;
            cboJob.Text = _fatherJob;
            cboNationality.Text = _fatherNationality;
            txtPhone.Text = _fatherPhone;

            cboAlive.SetComboBoxValue(_fatherAlive);
            cboNationality.SetComboBoxValue(_fatherNationality);
            cboJob.SetComboBoxValue(_fatherJob);
            cboEduDegree.SetComboBoxValue(_fatherEduDegree);

            lnkCopyGuardian.Visible = true;
            lnkCopyFather.Visible = false;
            lnkCopyMother.Visible = false;

            this.textBirthYear.Enabled = true;
            this.textBirthCountry.Enabled = true;
            this.textFromChina.Enabled = true;
            this.textFromForeign.Enabled = true;
            this.rdbspun.Enabled = true;
            this.rdbspuy.Enabled = true;

            if (ParentExtentionInfo != null)
            {
                this.textEmail.Text = ParentExtentionInfo.FatherEmail;
                this.textBirthYear.Text = ParentExtentionInfo.FatherBirthYear;
                this.textBirthCountry.Text = ParentExtentionInfo.FatherBirthCountry;
                this.textFromChina.Text = ParentExtentionInfo.FatherFromChina;
                this.textFromForeign.Text = ParentExtentionInfo.FatherFromForeign;

                if (ParentExtentionInfo.FatherIsUnemployed == "true")
                {
                    this.rdbspuy.Checked = true;
                }
                else if (ParentExtentionInfo.FatherIsUnemployed == "false")
                {
                    this.rdbspun.Checked = true;
                }
                else
                {
                    this.rdbspun.Checked = false;
                    this.rdbspuy.Checked = false;
                }
            }
            else
            {
                this.textEmail.Text = "";
                this.textBirthYear.Text = "";
                this.textBirthCountry.Text = "";
                this.textFromChina.Text = "";
                this.textFromForeign.Text = "";
                this.rdbspun.Checked = false;
                this.rdbspuy.Checked = false;
            }

            _DataListener_Father.Reset();
            _DataListener_Father.ResumeListen();
        }

        private void btnMother_Click(object sender, EventArgs e)
        {
            DataListenerPause();

            btnGuardian.Enabled = true;
            btnFather.Enabled = true;
            btnMother.Enabled = false;

            cboAlive.Visible = true;
            lblAlive.Visible = true;
            cboRelationship.Visible = false;
            lblRelationship.Visible = false;

            btnParentType.Text = btnMother.Text;
            txtParentName.Text = _motherName;
            txtIDNumber.Text = _motherIDNumber;
            cboAlive.Text = _motherAlive;
            cboEduDegree.Text = _motherEduDegree;
            cboJob.Text = _motherJob;
            cboNationality.Text = _motherNationality;
            txtPhone.Text = _motherPhone;

            cboAlive.SetComboBoxValue(_motherAlive);
            cboNationality.SetComboBoxValue(_motherNationality);
            cboJob.SetComboBoxValue(_motherJob);
            cboEduDegree.SetComboBoxValue(_motherEduDegree);

            lnkCopyGuardian.Visible = true;
            lnkCopyFather.Visible = false;
            lnkCopyMother.Visible = false;

            this.textBirthYear.Enabled = true;
            this.textBirthCountry.Enabled = true;
            this.textFromChina.Enabled = true;
            this.textFromForeign.Enabled = true;
            this.rdbspun.Enabled = true;
            this.rdbspuy.Enabled = true;
            if (ParentExtentionInfo != null)
            {
                this.textEmail.Text = ParentExtentionInfo.MotherEmail;
                this.textBirthYear.Text = ParentExtentionInfo.MotherBirthYear;
                this.textBirthCountry.Text = ParentExtentionInfo.MotherBirthCountry;
                this.textFromChina.Text = ParentExtentionInfo.MotherFromChina;
                this.textFromForeign.Text = ParentExtentionInfo.MotherFromForeign;

                if (ParentExtentionInfo.MotherIsUnemployed == "true")
                {
                    this.rdbspuy.Checked = true;
                }
                else if (ParentExtentionInfo.MotherIsUnemployed == "false")
                {
                    this.rdbspun.Checked = true;
                }
                else
                {
                    this.rdbspun.Checked = false;
                    this.rdbspuy.Checked = false;
                }
            }
            else
            {
                this.textEmail.Text = "";
                this.textBirthYear.Text = "";
                this.textBirthCountry.Text = "";
                this.textFromChina.Text = "";
                this.textFromForeign.Text = "";
                this.rdbspun.Checked = false;
                this.rdbspuy.Checked = false;
            }
            _DataListener_Mother.Reset();
            _DataListener_Mother.ResumeListen();
        }

        private void LoadGuardian()
        {
            DataListenerPause();
            btnGuardian.Enabled = false;
            btnFather.Enabled = true;
            btnMother.Enabled = true;

            cboAlive.Visible = false;
            lblAlive.Visible = false;
            cboRelationship.Visible = true;
            lblRelationship.Visible = true;

            btnParentType.Text = btnGuardian.Text;
            txtParentName.Text = _guardianName;
            txtIDNumber.Text = _guardianIDNumber;

            cboEduDegree.Text = _guardianEduDegree;
            cboJob.Text = _guardianJob;
            cboNationality.Text = _guardianNationality;
            cboRelationship.Text = _guardianRelationship;
            txtPhone.Text = _guardianPhone;

            cboRelationship.SetComboBoxValue(_guardianRelationship);
            cboNationality.SetComboBoxValue(_guardianNationality);
            cboJob.SetComboBoxValue(_guardianJob);
            cboEduDegree.SetComboBoxValue(_guardianEduDegree);

            lnkCopyGuardian.Visible = false;
            lnkCopyFather.Visible = true;
            lnkCopyMother.Visible = true;

            if (ParentExtentionInfo != null)
            {
                this.textEmail.Text = ParentExtentionInfo.GuardianEmail;
                this.textBirthYear.Enabled = false;
                this.textBirthCountry.Enabled = false;
                this.textFromChina.Enabled = false;
                this.textFromForeign.Enabled = false;
                this.rdbspun.Enabled = false;
                this.rdbspuy.Enabled = false;
                this.textBirthYear.Text = "";
                this.textBirthCountry.Text = "";
                this.textFromChina.Text = "";
                this.textFromForeign.Text = "";
                this.rdbspun.Checked = false;
                this.rdbspuy.Checked = false;
            }
            else
            {
                this.textEmail.Text = "";
                this.textBirthYear.Enabled = false;
                this.textBirthCountry.Enabled = false;
                this.textFromChina.Enabled = false;
                this.textFromForeign.Enabled = false;
                this.rdbspun.Enabled = false;
                this.rdbspuy.Enabled = false;
                this.textBirthYear.Text = "";
                this.textBirthCountry.Text = "";
                this.textFromChina.Text = "";
                this.textFromForeign.Text = "";
                this.rdbspun.Checked = false;
                this.rdbspuy.Checked = false;
            }


            _DataListener_Guardian.Reset();
            _DataListener_Guardian.ResumeListen();


        }

        private void txtParentName_TextChanged(object sender, EventArgs e)
        {
            string typeName;
            string value = txtParentName.Text;

            if (btnParentType.Text == "監護人")
            {
                typeName = "CustodianName";
                _guardianName = value;
            }
            else if (btnParentType.Text == "父親")
            {
                typeName = "FatherName";
                _fatherName = value;
            }
            else
            {
                typeName = "MotherName";
                _motherName = value;
            }
            OnValueChanged(typeName, value);
        }

        private void cboNationality_SelectedIndexChanged(object sender, EventArgs e)
        {
            string typeName;
            string value = cboNationality.GetValue();

            if (btnParentType.Text == "監護人")
            {
                typeName = "CustodianNationality";
                _guardianNationality = value;
            }
            else if (btnParentType.Text == "父親")
            {
                typeName = "FatherNationality";
                _fatherNationality = value;
            }
            else
            {
                typeName = "MotherNationality";
                _motherNationality = value;
            }
            OnValueChanged(typeName, value);
        }

        private string GetEduDegreeResponse(XmlNode infoNode, string degreeNodeName, string eduDegree, string otherInfoNodeName)
        {
            if (infoNode != null)
            {
                XmlNode node = infoNode.SelectSingleNode(degreeNodeName);
                if (node != null)
                {
                    //node.InnerText = _valueManager.GetDirtyItems()[degreeNodeName];
                    node.InnerText = eduDegree;
                }
                else
                {
                    XmlNode n = infoNode.OwnerDocument.CreateElement(degreeNodeName);
                    n.InnerText = eduDegree;
                    infoNode.AppendChild(n);
                }
                return infoNode.OuterXml;
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                XmlNode n = doc.CreateElement(otherInfoNodeName);
                XmlNode n1 = doc.CreateElement(degreeNodeName);
                n1.InnerText = eduDegree;
                n.AppendChild(n1);
                return n.OuterXml;
            }
        }

        private void lnkCopyGuardian_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            txtParentName.Text = _guardianName;
            txtIDNumber.Text = _guardianIDNumber;
            cboNationality.Text = _guardianNationality;
            cboJob.Text = _guardianJob;
            cboEduDegree.Text = _guardianEduDegree;
            txtPhone.Text = _guardianPhone;

            cboNationality.SetComboBoxValue(_guardianNationality);
            cboJob.SetComboBoxValue(_guardianJob);
            cboEduDegree.SetComboBoxValue(_guardianEduDegree);
            textEmail.Text = ParentExtentionInfo.GuardianEmail;
            textBirthYear.Text = "";
            textBirthCountry.Text = "";
            textFromChina.Text = "";
            textFromForeign.Text = "";
            rdbspun.Checked = false;
            rdbspuy.Checked = false;
        }

        private void lnkCopyFather_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            txtParentName.Text = _fatherName;
            txtIDNumber.Text = _fatherIDNumber;
            cboNationality.Text = _fatherNationality;
            cboJob.Text = _fatherJob;
            cboEduDegree.Text = _fatherEduDegree;
            txtPhone.Text = _fatherPhone;

            cboNationality.SetComboBoxValue(_fatherNationality);
            cboJob.SetComboBoxValue(_fatherJob);
            cboEduDegree.SetComboBoxValue(_fatherEduDegree);
            if (btnParentType.Text == "監護人")
            {
                cboRelationship.SetComboBoxValue("父");
                cboRelationship.Text = "父";
            }
            textEmail.Text = ParentExtentionInfo.FatherEmail;
        }

        private void lnkCopyMother_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            txtParentName.Text = _motherName;
            txtIDNumber.Text = _motherIDNumber;
            cboNationality.Text = _motherNationality;
            cboJob.Text = _motherJob;
            cboEduDegree.Text = _motherEduDegree;
            txtPhone.Text = _motherPhone;

            cboNationality.SetComboBoxValue(_motherNationality);
            cboJob.SetComboBoxValue(_motherJob);
            cboEduDegree.SetComboBoxValue(_motherEduDegree);
            if (btnParentType.Text == "監護人")
            {
                cboRelationship.SetComboBoxValue("母");
                cboRelationship.Text = "母";
            }
            textEmail.Text = ParentExtentionInfo.FatherEmail;
        }

        private void cboNationality_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            string typeName;
            //string value = cboNationality.GetValue();
            string value = cboNationality.Text;

            if (btnParentType.Text == "監護人")
            {
                typeName = "CustodianNationality";
                _guardianNationality = value;
            }
            else if (btnParentType.Text == "父親")
            {
                typeName = "FatherNationality";
                _fatherNationality = value;
            }
            else
            {
                typeName = "MotherNationality";
                _motherNationality = value;
            }
            OnValueChanged(typeName, value);
        }

        private void cboRelationship_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            string typeName;
            //string value = cboRelationship.GetValue();
            string value = cboRelationship.Text;

            if (btnParentType.Text == "監護人")
            {
                typeName = "CustodianRelationship";
                _guardianRelationship = value;
            }
            else if (btnParentType.Text == "父親")
            {
                return;
            }
            else
            {
                return;
            }
            OnValueChanged(typeName, value);
        }

        private void cboAlive_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            string typeName;
            string value = cboAlive.GetValue();

            if (btnParentType.Text == "監護人")
            {
                return;
            }
            else if (btnParentType.Text == "父親")
            {
                typeName = "FatherLiving";
                _fatherAlive = value;
            }
            else
            {
                typeName = "MotherLiving";
                _motherAlive = value;
            }
            OnValueChanged(typeName, value);
        }

        private void cboEduDegree_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            string typeName;
            //string value = cboEduDegree.GetValue();
            string value = cboEduDegree.Text;

            if (btnParentType.Text == "監護人")
            {
                typeName = "EducationDegree";
                _guardianEduDegree = value;
            }
            else if (btnParentType.Text == "父親")
            {
                typeName = "FatherEducationDegree";
                _fatherEduDegree = value;
            }
            else
            {
                typeName = "MotherEducationDegree";
                _motherEduDegree = value;
            }
            OnValueChanged(typeName, value);
        }

        private void cboJob_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            string typeName;
            //string value = cboJob.GetValue();
            string value = cboJob.Text;

            if (btnParentType.Text == "監護人")
            {
                typeName = "Job";
                _guardianJob = value;
            }
            else if (btnParentType.Text == "父親")
            {
                typeName = "FatherJob";
                _fatherJob = value;
            }
            else
            {
                typeName = "MotherJob";
                _motherJob = value;
            }
            OnValueChanged(typeName, value);
        }

        private void txtIDNumber_TextChanged(object sender, EventArgs e)
        {
            string typeName;
            string value = txtIDNumber.Text;

            if (btnParentType.Text == "監護人")
            {
                typeName = "CustodianIDNumber";
                _guardianIDNumber = value;
            }
            else if (btnParentType.Text == "父親")
            {
                typeName = "FatherIDNumber";
                _fatherIDNumber = value;
            }
            else
            {
                typeName = "MotherIDNumber";
                _motherIDNumber = value;
            }
            OnValueChanged(typeName, value);
        }

        private void txtPhone_TextChanged(object sender, EventArgs e)
        {
            string typeName;
            string value = txtPhone.Text;

            if (btnParentType.Text == "監護人")
            {
                typeName = "CustodianPhone";
                _guardianPhone = value;
            }
            else if (btnParentType.Text == "父親")
            {
                typeName = "FatherPhone";
                _fatherPhone = value;
            }
            else
            {
                typeName = "MotherPhone";
                _motherPhone = value;
            }
            OnValueChanged(typeName, value);
        }

        private void GetParentExtentionInfo()
        {
            string sql = @"SELECT 
	ref_student_id 
	,guardian_email
	,father_birth_year
 	,father_birth_country
 	,father_email
	,father_from_china
	,father_from_foreign
	,father_is_unemployed
	,mother_birth_year
	,mother_birth_country
	,mother_email
	,mother_from_china
	,mother_from_foreign
	,mother_is_unemployed
FROM  
    student_info_ext
WHERE ref_student_id = {0}";

            sql = string.Format(sql, RunningID);
            DataTable dt = _Qp.Select(sql);
            Console.WriteLine(dt.Rows.Count);

            if (dt.Rows.Count == 0)
            {
                this.ParentExtentionInfo = new ParentExtentionInfo(this.RunningID);
                ParentExtentionInfo.NoExtensoinRecord = true;
            }
            else
            {
                DataRow dr = dt.Rows[0];
                this.ParentExtentionInfo = new ParentExtentionInfo("" + dr["ref_student_id"]);
                this.ParentExtentionInfo.GuardianEmail = "" + dr["guardian_email"];
                this.ParentExtentionInfo.FatherBirthYear = "" + dr["father_birth_year"];
                this.ParentExtentionInfo.FatherBirthCountry = "" + dr["father_birth_country"];
                this.ParentExtentionInfo.FatherEmail = "" + dr["father_email"];
                this.ParentExtentionInfo.FatherFromChina = "" + dr["father_from_china"];
                this.ParentExtentionInfo.FatherFromForeign = "" + dr["father_from_foreign"];
                this.ParentExtentionInfo.FatherIsUnemployed = "" + dr["father_is_unemployed"];
                this.ParentExtentionInfo.MotherBirthYear = "" + dr["mother_birth_year"];
                this.ParentExtentionInfo.MotherBirthCountry = "" + dr["mother_birth_country"];
                this.ParentExtentionInfo.MotherEmail = "" + dr["mother_email"];
                this.ParentExtentionInfo.MotherFromChina = "" + dr["mother_from_china"];
                this.ParentExtentionInfo.MotherFromForeign = "" + dr["mother_from_foreign"];
                this.ParentExtentionInfo.MotherIsUnemployed = "" + dr["mother_is_unemployed"];
                ParentExtentionInfo.NoExtensoinRecord = false;
            }
        }

        private void WrapFromValue()
        {
            this.WrapParentExtentionInfo = new ParentExtentionInfo(this.RunningID);
            switch (btnParentType.Text)
            {
                case "監護人":
                    WrapParentExtentionInfo.GuardianEmail = this.textEmail.Text;
                    WrapParentExtentionInfo.FatherBirthYear = this.ParentExtentionInfo.FatherBirthYear;
                    WrapParentExtentionInfo.FatherBirthCountry = this.ParentExtentionInfo.FatherBirthCountry;
                    WrapParentExtentionInfo.FatherEmail = this.ParentExtentionInfo.FatherEmail;
                    WrapParentExtentionInfo.FatherFromChina = this.ParentExtentionInfo.FatherFromChina;
                    WrapParentExtentionInfo.FatherFromForeign = this.ParentExtentionInfo.FatherFromForeign;
                    WrapParentExtentionInfo.MotherBirthYear = this.ParentExtentionInfo.MotherBirthYear;
                    WrapParentExtentionInfo.MotherBirthCountry = this.ParentExtentionInfo.MotherBirthCountry;
                    WrapParentExtentionInfo.MotherEmail = this.ParentExtentionInfo.MotherEmail;
                    WrapParentExtentionInfo.MotherFromChina = this.ParentExtentionInfo.MotherFromChina;
                    WrapParentExtentionInfo.MotherFromForeign = this.ParentExtentionInfo.MotherFromForeign;
                    break;
                case "父親":
                    WrapParentExtentionInfo.FatherBirthYear = this.textBirthYear.Text;
                    WrapParentExtentionInfo.FatherBirthCountry = this.textBirthCountry.Text;
                    WrapParentExtentionInfo.FatherEmail = this.textEmail.Text;
                    WrapParentExtentionInfo.FatherFromChina = this.textFromChina.Text;
                    WrapParentExtentionInfo.FatherFromForeign = this.textFromForeign.Text;
                    if (rdbspuy.Checked)
                    {
                        WrapParentExtentionInfo.FatherIsUnemployed = "true";
                    }
                    else if (rdbspun.Checked)
                    {
                        WrapParentExtentionInfo.FatherIsUnemployed = "false";
                    }
                    else
                    {
                        WrapParentExtentionInfo.FatherIsUnemployed = "";
                    }
                    WrapParentExtentionInfo.MotherBirthYear = this.ParentExtentionInfo.MotherBirthYear;
                    WrapParentExtentionInfo.MotherBirthCountry = this.ParentExtentionInfo.MotherBirthCountry;
                    WrapParentExtentionInfo.MotherEmail = this.ParentExtentionInfo.MotherEmail;
                    WrapParentExtentionInfo.MotherFromChina = this.ParentExtentionInfo.MotherFromChina;
                    WrapParentExtentionInfo.MotherFromForeign = this.ParentExtentionInfo.MotherFromForeign;
                    WrapParentExtentionInfo.GuardianEmail = this.ParentExtentionInfo.GuardianEmail;
                    break;
                case "母親":
                    WrapParentExtentionInfo.MotherBirthYear = this.textBirthYear.Text;
                    WrapParentExtentionInfo.MotherBirthCountry = this.textBirthCountry.Text;
                    WrapParentExtentionInfo.MotherEmail = this.textEmail.Text;
                    WrapParentExtentionInfo.MotherFromChina = this.textFromChina.Text;
                    WrapParentExtentionInfo.MotherFromForeign = this.textFromForeign.Text;
                    if (rdbspuy.Checked)
                    {
                        WrapParentExtentionInfo.MotherIsUnemployed = "true";
                    }
                    else if (rdbspun.Checked)
                    {
                        WrapParentExtentionInfo.MotherIsUnemployed = "false";
                    }
                    else
                    {
                        WrapParentExtentionInfo.FatherIsUnemployed = "";
                    }
                    WrapParentExtentionInfo.FatherBirthYear = this.ParentExtentionInfo.FatherBirthYear;
                    WrapParentExtentionInfo.FatherBirthCountry = this.ParentExtentionInfo.FatherBirthCountry;
                    WrapParentExtentionInfo.FatherEmail = this.ParentExtentionInfo.FatherEmail;
                    WrapParentExtentionInfo.FatherFromChina = this.ParentExtentionInfo.FatherFromChina;
                    WrapParentExtentionInfo.FatherFromForeign = this.ParentExtentionInfo.FatherFromForeign;
                    WrapParentExtentionInfo.GuardianEmail = this.ParentExtentionInfo.GuardianEmail;
                    break;
                default:
                    break;

            }
        }

        private void ParentExtenstionUpdate()
        {
            string sql = "";
            string sql2 = "";
            string sql3 = "";
            if (this.ParentExtentionInfo.NoExtensoinRecord == true)
            {
                if (btnParentType.Text == "監護人")
                {
                    sql = @"            
INSERT INTO  
                student_info_ext
                (
                  ref_student_id 
                  , guardian_email
                )VALUES
                (
                   {0} 
                  , {1}
                    )
RETURNING *
";
                    sql = string.Format(sql
                        , WrapParentExtentionInfo.StudentID
                        , WrapParentExtentionInfo.GuardianEmail != "" ? $"'{WrapParentExtentionInfo.GuardianEmail}'" : "NULL");
                }
                else if (btnParentType.Text == "父親")
                {
                    sql2 = @"            INSERT INTO  
                student_info_ext
                (
                  ref_student_id 
                  , father_birth_year
                  , father_birth_country
                  , father_email
                  , father_from_china
                  , father_from_foreign
                  , father_is_unemployed
                )VALUES
                (
                   {0} 
                  , {1}
                  , {2}
                  , {3}
                  , {4}
                  , {5}
                  , {6}
                    )
RETURNING *
";
                    sql2 = string.Format(sql2
                        , WrapParentExtentionInfo.StudentID
                        , WrapParentExtentionInfo.FatherBirthYear != "" ? $"'{WrapParentExtentionInfo.FatherBirthYear}'" : "NULL"
                        , WrapParentExtentionInfo.FatherBirthCountry != "" ? $"'{WrapParentExtentionInfo.FatherBirthCountry}'" : "NULL"
                        , WrapParentExtentionInfo.FatherEmail != "" ? $"'{WrapParentExtentionInfo.FatherEmail}'" : "NULL"
                        , WrapParentExtentionInfo.FatherFromChina != "" ? $"'{WrapParentExtentionInfo.FatherFromChina}'" : "NULL"
                        , WrapParentExtentionInfo.FatherFromForeign != "" ? $"'{WrapParentExtentionInfo.FatherFromForeign}'" : "NULL"
                        , WrapParentExtentionInfo.FatherIsUnemployed != "" ? $"'{WrapParentExtentionInfo.FatherIsUnemployed}'" : "NULL");
                }
                else if (btnParentType.Text == "母親")
                {
                    sql3 = @"            INSERT INTO  
                student_info_ext
                (
                  ref_student_id 
                  , mother_birth_year
                  , mother_birth_country
                  , mother_email
                  , mother_from_china
                  , mother_from_foreign
                  , mother_is_unemployed
                )VALUES
                (
                   {0} 
                  , {1}
                  , {2}
                  , {3}
                  , {4}
                  , {5}
                  , {6}
                    )
RETURNING *
";
                    sql3 = string.Format(sql3
                        , WrapParentExtentionInfo.StudentID
                        , WrapParentExtentionInfo.MotherBirthYear != "" ? $"'{WrapParentExtentionInfo.MotherBirthYear}'" : "NULL"
                        , WrapParentExtentionInfo.MotherBirthCountry != "" ? $"'{WrapParentExtentionInfo.MotherBirthCountry}'" : "NULL"
                        , WrapParentExtentionInfo.MotherEmail != "" ? $"'{WrapParentExtentionInfo.MotherEmail}'" : "NULL"
                        , WrapParentExtentionInfo.MotherFromChina != "" ? $"'{WrapParentExtentionInfo.MotherFromChina}'" : "NULL"
                        , WrapParentExtentionInfo.MotherFromForeign != "" ? $"'{WrapParentExtentionInfo.MotherFromForeign}'" : "NULL"
                        , WrapParentExtentionInfo.MotherIsUnemployed != "" ? $"'{WrapParentExtentionInfo.MotherIsUnemployed}'" : "NULL");
                }
            }
            else if (this.ParentExtentionInfo.NoExtensoinRecord == false)
            {
                if (btnParentType.Text == "監護人")
                {
                    sql = @"UPDATE 
     student_info_ext
SET  
	 guardian_email = {1}
WHERE 
     ref_student_id = {0}
returning *";
                    sql = string.Format(sql
                        , WrapParentExtentionInfo.StudentID
                        , WrapParentExtentionInfo.GuardianEmail != "" ? $"'{WrapParentExtentionInfo.GuardianEmail}'" : "NULL"
                        );
                }
                else if (btnParentType.Text == "父親")
                {
                    sql2 = @"UPDATE 
     student_info_ext
SET  
	 father_birth_year= {1}
 	 ,father_birth_country = {2}
  	 ,father_email = {3}
  	 ,father_from_china = {4}
  	 ,father_from_foreign = {5}
  	 ,father_is_unemployed = {6}
WHERE 
     ref_student_id = {0}
returning *";
                    sql2 = string.Format(sql2
                        , WrapParentExtentionInfo.StudentID
                        , WrapParentExtentionInfo.FatherBirthYear != "" ? $"'{WrapParentExtentionInfo.FatherBirthYear}'" : "NULL"
                        , WrapParentExtentionInfo.FatherBirthCountry != "" ? $"'{WrapParentExtentionInfo.FatherBirthCountry}'" : "NULL"
                        , WrapParentExtentionInfo.FatherEmail != "" ? $"'{WrapParentExtentionInfo.FatherEmail}'" : "NULL"
                        , WrapParentExtentionInfo.FatherFromChina != "" ? $"'{WrapParentExtentionInfo.FatherFromChina}'" : "NULL"
                        , WrapParentExtentionInfo.FatherFromForeign != "" ? $"'{WrapParentExtentionInfo.FatherFromForeign}'" : "NULL"
                        , WrapParentExtentionInfo.FatherIsUnemployed != "" ? $"'{WrapParentExtentionInfo.FatherIsUnemployed}'" : "NULL");
                }
                else if (btnParentType.Text == "母親")
                {
                    sql3 = @"UPDATE 
     student_info_ext
SET  
	 mother_birth_year= {1}
 	 ,mother_birth_country = {2}
  	 ,mother_email = {3}
  	 ,mother_from_china = {4}
  	 ,mother_from_foreign = {5}
  	 ,mother_is_unemployed = {6}
WHERE 
     ref_student_id = {0}
returning *";
                    sql3 = string.Format(sql3
                        , WrapParentExtentionInfo.StudentID
                        , WrapParentExtentionInfo.MotherBirthYear != "" ? $"'{WrapParentExtentionInfo.MotherBirthYear}'" : "NULL"
                        , WrapParentExtentionInfo.MotherBirthCountry != "" ? $"'{WrapParentExtentionInfo.MotherBirthCountry}'" : "NULL"
                        , WrapParentExtentionInfo.MotherEmail != "" ? $"'{WrapParentExtentionInfo.MotherEmail}'" : "NULL"
                        , WrapParentExtentionInfo.MotherFromChina != "" ? $"'{WrapParentExtentionInfo.MotherFromChina}'" : "NULL"
                        , WrapParentExtentionInfo.MotherFromForeign != "" ? $"'{WrapParentExtentionInfo.MotherFromForeign}'" : "NULL"
                        , WrapParentExtentionInfo.MotherIsUnemployed != "" ? $"'{WrapParentExtentionInfo.MotherIsUnemployed}'" : "NULL");
                }
            }

            try
            {
                K12.Data.StudentRecord studentRecord = K12.Data.Student.SelectByID(WrapParentExtentionInfo.StudentID);
                StringBuilder sb = new StringBuilder();
                if (btnParentType.Text == "監護人")
                {
                    DataTable dt = _Qp.Select(sql);
                    //sb.AppendLine($"{studentRecord.Name}，學號：{studentRecord.StudentNumber}");
                    //if (this.ParentExtentionInfo.GuardianEmail != WrapParentExtentionInfo.GuardianEmail)
                    //{
                    //    sb.AppendLine($"【學生監護人Email】由「{ParentExtentionInfo.GuardianEmail}」改為「{WrapParentExtentionInfo.GuardianEmail}」");
                    //}
                    //FISCA.LogAgent.ApplicationLog.Log("學生-資料項目-父母親及監護人資料", "修改", "student", WrapParentExtentionInfo.StudentID, sb.ToString());
                    DataRow dr = dt.Rows[0];
                    string studentID = "" + dr["ref_student_id"];
                    if (this.RunningID == studentID)
                    {
                        this.ParentExtentionInfo = new ParentExtentionInfo("" + dr["ref_student_id"]);
                        this.ParentExtentionInfo.GuardianEmail = "" + dr["guardian_email"];
                        this.ParentExtentionInfo.FatherBirthYear = "" + dr["father_birth_year"];
                        this.ParentExtentionInfo.FatherBirthCountry = "" + dr["father_birth_country"];
                        this.ParentExtentionInfo.FatherEmail = "" + dr["father_email"];
                        this.ParentExtentionInfo.FatherFromChina = "" + dr["father_from_china"];
                        this.ParentExtentionInfo.FatherFromForeign = "" + dr["father_from_foreign"];
                        this.ParentExtentionInfo.FatherIsUnemployed = "" + dr["father_is_unemployed"];
                        this.ParentExtentionInfo.MotherBirthYear = "" + dr["mother_birth_year"];
                        this.ParentExtentionInfo.MotherBirthCountry = "" + dr["mother_birth_country"];
                        this.ParentExtentionInfo.MotherEmail = "" + dr["mother_email"];
                        this.ParentExtentionInfo.MotherFromChina = "" + dr["mother_from_china"];
                        this.ParentExtentionInfo.MotherFromForeign = "" + dr["mother_from_foreign"];
                        this.ParentExtentionInfo.MotherIsUnemployed = "" + dr["mother_is_unemployed"];
                        ParentExtentionInfo.NoExtensoinRecord = false;
                    }
                }
                else if (btnParentType.Text == "父親")
                {
                    DataTable dt = _Qp.Select(sql2);
                    //sb.AppendLine($"{studentRecord.Name}，學號：{studentRecord.StudentNumber}");
                    //if (this.ParentExtentionInfo.FatherBirthYear != WrapParentExtentionInfo.FatherBirthYear)
                    //{
                    //    sb.AppendLine($"【學生父親出生年次】由「{ParentExtentionInfo.FatherBirthYear}」改為「{WrapParentExtentionInfo.FatherBirthYear}」");
                    //}
                    //if (this.ParentExtentionInfo.FatherBirthCountry != WrapParentExtentionInfo.FatherBirthCountry)
                    //{
                    //    sb.AppendLine($"【學生父親原屬國家地區】由「{ParentExtentionInfo.FatherBirthCountry}」改為「{WrapParentExtentionInfo.FatherBirthCountry}」");
                    //}
                    //if (this.ParentExtentionInfo.FatherEmail != WrapParentExtentionInfo.FatherEmail)
                    //{
                    //    sb.AppendLine($"【學生父親Email】由「{ParentExtentionInfo.FatherEmail}」改為「{WrapParentExtentionInfo.FatherEmail}」");
                    //}
                    //if (this.ParentExtentionInfo.FatherFromChina != WrapParentExtentionInfo.FatherFromChina)
                    //{
                    //    sb.AppendLine($"【學生父親為大陸配偶_省份】由「{ParentExtentionInfo.FatherFromChina}」改為「{WrapParentExtentionInfo.FatherFromChina}」");
                    //}
                    //if (this.ParentExtentionInfo.FatherFromForeign != WrapParentExtentionInfo.FatherFromForeign)
                    //{
                    //    sb.AppendLine($"【學生父親為外籍配偶_國籍】由「{ParentExtentionInfo.FatherFromForeign}」改為「{WrapParentExtentionInfo.FatherFromForeign}」");
                    //}
                    //if (this.ParentExtentionInfo.FatherIsUnemployed != WrapParentExtentionInfo.FatherIsUnemployed)
                    //{
                    //    sb.AppendLine($"【學生父親為失業勞工】由「{ParentExtentionInfo.FatherIsUnemployed}」改為「{WrapParentExtentionInfo.FatherIsUnemployed}」");
                    //}
                    //FISCA.LogAgent.ApplicationLog.Log("學生-資料項目-父母親及監護人資料", "修改", "student", WrapParentExtentionInfo.StudentID, sb.ToString());
                    DataRow dr = dt.Rows[0];
                    string studentID = "" + dr["ref_student_id"];
                    if (this.RunningID == studentID)
                    {
                        this.ParentExtentionInfo = new ParentExtentionInfo("" + dr["ref_student_id"]);
                        this.ParentExtentionInfo.GuardianEmail = "" + dr["guardian_email"];
                        this.ParentExtentionInfo.FatherBirthYear = "" + dr["father_birth_year"];
                        this.ParentExtentionInfo.FatherBirthCountry = "" + dr["father_birth_country"];
                        this.ParentExtentionInfo.FatherEmail = "" + dr["father_email"];
                        this.ParentExtentionInfo.FatherFromChina = "" + dr["father_from_china"];
                        this.ParentExtentionInfo.FatherFromForeign = "" + dr["father_from_foreign"];
                        this.ParentExtentionInfo.FatherIsUnemployed = "" + dr["father_is_unemployed"];
                        this.ParentExtentionInfo.MotherBirthYear = "" + dr["mother_birth_year"];
                        this.ParentExtentionInfo.MotherBirthCountry = "" + dr["mother_birth_country"];
                        this.ParentExtentionInfo.MotherEmail = "" + dr["mother_email"];
                        this.ParentExtentionInfo.MotherFromChina = "" + dr["mother_from_china"];
                        this.ParentExtentionInfo.MotherFromForeign = "" + dr["mother_from_foreign"];
                        this.ParentExtentionInfo.MotherIsUnemployed = "" + dr["mother_is_unemployed"];
                        ParentExtentionInfo.NoExtensoinRecord = false;
                    }
                }
                else if (btnParentType.Text == "母親")
                {
                    DataTable dt = _Qp.Select(sql3);
                    //sb.AppendLine($"{studentRecord.Name}，學號：{studentRecord.StudentNumber}");
                    //if (this.ParentExtentionInfo.MotherBirthYear != WrapParentExtentionInfo.MotherBirthYear)
                    //{
                    //    sb.AppendLine($"【學生母親出生年次】由「{ParentExtentionInfo.MotherBirthYear}」改為「{WrapParentExtentionInfo.MotherBirthYear}」");
                    //}
                    //if (this.ParentExtentionInfo.MotherBirthCountry != WrapParentExtentionInfo.MotherBirthCountry)
                    //{
                    //    sb.AppendLine($"【學生母親原屬國家地區】由「{ParentExtentionInfo.MotherBirthCountry}」改為「{WrapParentExtentionInfo.MotherBirthCountry}」");
                    //}
                    //if (this.ParentExtentionInfo.MotherEmail != WrapParentExtentionInfo.MotherEmail)
                    //{
                    //    sb.AppendLine($"【學生母親Email】由「{ParentExtentionInfo.MotherEmail}」改為「{WrapParentExtentionInfo.MotherEmail}」");
                    //}
                    //if (this.ParentExtentionInfo.MotherFromChina != WrapParentExtentionInfo.MotherFromChina)
                    //{
                    //    sb.AppendLine($"【學生母親為大陸配偶_省份】由「{ParentExtentionInfo.MotherFromChina}」改為「{WrapParentExtentionInfo.MotherFromChina}」");
                    //}
                    //if (this.ParentExtentionInfo.MotherFromForeign != WrapParentExtentionInfo.MotherFromForeign)
                    //{
                    //    sb.AppendLine($"【學生母親為外籍配偶_國籍】由「{ParentExtentionInfo.MotherFromForeign}」改為「{WrapParentExtentionInfo.MotherFromForeign}」");
                    //}
                    //if (this.ParentExtentionInfo.MotherIsUnemployed != WrapParentExtentionInfo.MotherIsUnemployed)
                    //{
                    //    sb.AppendLine($"【學生母親為失業勞工】由「{ParentExtentionInfo.MotherIsUnemployed}」改為「{WrapParentExtentionInfo.MotherIsUnemployed}」");
                    //}
                    //FISCA.LogAgent.ApplicationLog.Log("學生-資料項目-父母親及監護人資料", "修改", "student", WrapParentExtentionInfo.StudentID, sb.ToString());

                    DataRow dr = dt.Rows[0];
                    string studentID = "" + dr["ref_student_id"];
                    if (this.RunningID == studentID)
                    {
                        this.ParentExtentionInfo = new ParentExtentionInfo("" + dr["ref_student_id"]);
                        this.ParentExtentionInfo.GuardianEmail = "" + dr["guardian_email"];
                        this.ParentExtentionInfo.FatherBirthYear = "" + dr["father_birth_year"];
                        this.ParentExtentionInfo.FatherBirthCountry = "" + dr["father_birth_country"];
                        this.ParentExtentionInfo.FatherEmail = "" + dr["father_email"];
                        this.ParentExtentionInfo.FatherFromChina = "" + dr["father_from_china"];
                        this.ParentExtentionInfo.FatherFromForeign = "" + dr["father_from_foreign"];
                        this.ParentExtentionInfo.FatherIsUnemployed = "" + dr["father_is_unemployed"];
                        this.ParentExtentionInfo.MotherBirthYear = "" + dr["mother_birth_year"];
                        this.ParentExtentionInfo.MotherBirthCountry = "" + dr["mother_birth_country"];
                        this.ParentExtentionInfo.MotherEmail = "" + dr["mother_email"];
                        this.ParentExtentionInfo.MotherFromChina = "" + dr["mother_from_china"];
                        this.ParentExtentionInfo.MotherFromForeign = "" + dr["mother_from_foreign"];
                        this.ParentExtentionInfo.MotherIsUnemployed = "" + dr["mother_is_unemployed"];
                        ParentExtentionInfo.NoExtensoinRecord = false;
                    }
                }

            }
            catch (Exception ex)
            {
                FISCA.Presentation.Controls.MsgBox.Show("儲存發生錯誤! \r\n" + "錯誤訊息" + ex.Message);
            }

        }

        private void TextBirthYear_TextChanged(object sender, EventArgs e)
        {
            if (this.textBirthYear.Text != "" && !Regex.IsMatch(this.textBirthYear.Text, birthYearCheckpattern))
            {
                errorBrithYear.SetError(this.textBirthYear, "請輸入西元紀年(4碼)");
            }
            else
            {
                errorBrithYear.Clear();
            }

            string typeName;
            string value = textBirthYear.Text;

            if (btnParentType.Text == "監護人")
            {
                return;
            }
            else if (btnParentType.Text == "父親")
            {
                typeName = "FatherBirthYear";
                _fatherBirthYear = value;
            }
            else
            {
                typeName = "MotherBirthYear";
                _motherBirthYear = value;
            }
            OnValueChanged(typeName, value);
        }

        private void TextBirthCountry_TextChanged(object sender, EventArgs e)
        {
            if (this.textBirthCountry.Text != "" && !Regex.IsMatch(this.textBirthCountry.Text, birthCountryCheckPattern))
            {
                errorBrithCountry.SetError(this.textBirthCountry, "請輸入3碼的國籍及出生代碼");
            }
            else
            {
                errorBrithCountry.Clear();
            }
            string typeName;
            string value = textBirthCountry.Text;

            if (btnParentType.Text == "監護人")
            {
                return;
            }
            else if (btnParentType.Text == "父親")
            {
                typeName = "FatherBirthCountry";
                _fatherBirthCountry = value;
            }
            else
            {
                typeName = "MotherBirthCountry";
                _motherBirthCountry = value;
            }
            OnValueChanged(typeName, value);

        }

        private void TextEmail_TextChanged(object sender, EventArgs e)
        {
            string typeName;
            string value = textEmail.Text;

            if (btnParentType.Text == "監護人")
            {
                typeName = "GuardianEmail";
                _guardianEmail = value;
            }
            else if (btnParentType.Text == "父親")
            {
                typeName = "FatherEmail";
                _fatherEmail = value;
            }
            else
            {
                typeName = "MotherEmail";
                _motherEmail = value;
            }
            OnValueChanged(typeName, value);
        }

        private void TextFromChina_TextChanged(object sender, EventArgs e)
        {
            string typeName;
            string value = textFromChina.Text;

            if (btnParentType.Text == "監護人")
            {
                return;
            }
            else if (btnParentType.Text == "父親")
            {
                typeName = "FatherFromChina";
                _fatherFromChina = value;
            }
            else
            {
                typeName = "MotherFromChina";
                _motherFromChina = value;
            }
            OnValueChanged(typeName, value);
        }

        private void TextFromForeign_TextChanged(object sender, EventArgs e)
        {
            string typeName;
            string value = textFromForeign.Text;

            if (btnParentType.Text == "監護人")
            {
                return;
            }
            else if (btnParentType.Text == "父親")
            {
                typeName = "FatherFromForeign";
                _fatherFromForeign = value;
            }
            else
            {
                typeName = "MotherFromForeign";
                _motherFromForeign = value;
            }
            OnValueChanged(typeName, value);
        }

        private void Rdbspuy_CheckedChanged(object sender, EventArgs e)
        {
            string typeName;
            string value;
            if (rdbspuy.Checked)
            {
                value = "是";
            }
            else
            {
                value = "否";
            }

            if (btnParentType.Text == "監護人")
            {
                return;
            }
            else if (btnParentType.Text == "父親")
            {
                typeName = "FatherIsUnemployed";
                _fatherIsUnemployed = value;
            }
            else
            {
                typeName = "MotherIsUnemployed";
                _motherIsUnemployed = value;
            }
            OnValueChanged(typeName, value);
        }

        private void Rdbspun_CheckedChanged(object sender, EventArgs e)
        {
            string typeName;
            string value;
            if (btnParentType.Text == "監護人")
            {
                return;
            }
            else if (rdbspun.Checked)
            {
                value = "否";
            }
            else
            {
                value = "是";
            }

            if (btnParentType.Text == "父親")
            {
                typeName = "FatherIsUnemployed";
                _fatherIsUnemployed = value;
            }
            else
            {
                typeName = "MotherIsUnemployed";
                _motherIsUnemployed = value;
            }
            OnValueChanged(typeName, value);
        }
    }
}
