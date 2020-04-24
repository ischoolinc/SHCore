﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SmartSchool;
using FISCA.DSAUtil;
using System.Xml;
using System.Threading;
using SmartSchool.StudentRelated;
using SmartSchool.StudentRelated.RibbonBars.GMap;
using StuEntity = SmartSchool.StudentRelated.Student;
using SmartSchool.ApplicationLog.Forms;
using SmartSchool.ApplicationLog;
//using SmartSchool.SmartPlugIn.Common;
using DevComponents.DotNetBar;
using SmartSchool.Security;
using SmartSchool.Common;
using FISCA.Presentation;

namespace SmartSchool.StudentRelated.RibbonBars
{
    public partial class Others : RibbonBarBase
    {
        ButtonItemPlugInManager reportManager;

        FeatureAccessControl coorCtrl;
        FeatureAccessControl mapCtrl;

        private BackgroundWorker bkwGetGlobalPoint;

        public override string ProcessTabName
        {
            get
            {
                return "學生";
            }
        }

        public Others()
        {
            //InitializeComponent();


        }

        internal void Setup()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Others));

            var bar = K12.Presentation.NLDPanels.Student.RibbonBarItems["其它"];
            bar.OverflowButtonImage = ( (System.Drawing.Image)( resources.GetObject("MainRibbonBar.OverflowButtonImage") ) );
            bar.ResizeOrderIndex = -500;

            // 不使用先註解
            //var btnQueryCoor = bar["查詢經緯度"];
            //btnQueryCoor.Image = Properties.Resources.world_zoom_64;
            //btnQueryCoor.Click += new System.EventHandler(this.buttonItem3_Click);
            //btnQueryCoor.Enable = false;
            //btnQueryCoor.Image = ( (System.Drawing.Image)( resources.GetObject("btnQueryCoor.Image") ) );

            //var btnMap = bar["學生分佈圖"];
            //btnMap.Image = ((System.Drawing.Image)(resources.GetObject("btnMap.Image")));
            //btnMap.Enable = false;

            //var btnPerm = btnMap["戶籍地址"];
            //btnPerm.Click += new System.EventHandler(this.btnPerm_Click);

            //var btnConn = btnMap["聯絡地址"];
            //btnConn.Click += new System.EventHandler(this.btnConn_Click);

            //var btnOther = btnMap["其他地址"];
            //btnOther.Click += new System.EventHandler(this.btnOther_Click);

            bar.SetTopContainer(FISCA.Presentation.ContainerType.Medium);

            //權限判斷 - 修改歷程, 查詢經緯度, 學生分佈圖

            //2010/11/16 - 發現與FeatureDefinition.xml定義的不同
            //因此調整權限
            //Button0320是修改歷程
            //Button0300是查詢經緯度
            //Button0310是學生分佈圖 by dylan

            coorCtrl = new FeatureAccessControl("Button0300");
            mapCtrl = new FeatureAccessControl("Button0310");

            K12.Presentation.NLDPanels.Student.SelectedSourceChanged += delegate
            {

                bool isEnabled = SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;

                // 不使用
                //btnQueryCoor.Enable = isEnabled;
                //btnMap.Enable = isEnabled;
                //btnQueryCoor.Enable = isEnabled & coorCtrl.Executable();
                //btnMap.Enable = isEnabled & mapCtrl.Executable();
            };

            bkwGetGlobalPoint = new BackgroundWorker();
            bkwGetGlobalPoint.WorkerReportsProgress = true;
            bkwGetGlobalPoint.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bkwGetGlobalPoint_RunWorkerCompleted);
            bkwGetGlobalPoint.ProgressChanged += new ProgressChangedEventHandler(bkwGetGlobalPoint_ProgressChanged);
            bkwGetGlobalPoint.DoWork += new DoWorkEventHandler(bkwGetGlobalPoint_DoWork);


            //#region 設定為 "學生/其它" 的外掛處理者
            //reportManager = new ButtonItemPlugInManager(itemContainer1);
            //reportManager.LayoutMode = LayoutMode.Auto;
            //reportManager.ItemsChanged += new EventHandler(reportManager_ItemsChanged);
            //SmartSchool.Customization.PlugIn.GeneralizationPluhgInManager<ButtonItem>.Instance.Add("學生/其它", reportManager);
            //#endregion
        }

        //void reportManager_ItemsChanged(object sender, EventArgs e)
        //{
        //    this.Visible = itemContainer1.SubItems.Count > 0;
        //}


        void bkwGetGlobalPoint_DoWork(object sender, DoWorkEventArgs e)
        {
            int allSelectionCount = 0;
            //List<List<string>>[] packages = new List<List<string>>[6];
            List<List<string>>[] packages = new List<List<string>>[1];
            packages[0] = new List<List<string>>();
            //packages[1] = new List<List<string>>();
            //packages[2] = new List<List<string>>();
            //packages[3] = new List<List<string>>();
            //packages[4] = new List<List<string>>();
            //packages[5] = new List<List<string>>();

            #region 將選取學生切Package
            {
                int count = 0;
                int pcount = 0;
                List<string> package = null;
                foreach ( SmartSchool.StudentRelated.BriefStudentData var in SmartSchool.StudentRelated.Student.Instance.SelectionStudents )
                {
                    if ( count == 0 )
                    {
                        package = new List<string>(25);
                        count = 25;
                        //packages[pcount % 6].Add(package);
                        packages[0].Add(package);
                        pcount++;
                    }
                    count--;
                    package.Add(var.ID);
                    allSelectionCount++;
                }
            }
            #endregion
            if ( allSelectionCount == 0 ) return;
            //int activePackages = allSelectionCount / 150 + ( allSelectionCount % 150 > 0 ? 1 : 0 );
            int activePackages = 1;

            Thread[] otherThreads = new Thread[activePackages - 1];
            for ( int i = 0 ; i < otherThreads.Length ; i++ )
            {
                otherThreads[i] = new Thread(new ParameterizedThreadStart(loadSubjectScore));
                otherThreads[i].IsBackground = true;
                otherThreads[i].Start(packages[i + 1]);
            }

            int processedCount = 0;
            foreach ( List<string> package in packages[0] )
            {
                DSXmlHelper addressHelper = SmartSchool.Feature.QueryStudent.GetAddressWithID(package.ToArray()).GetContent();
                Dictionary<string, DSXmlHelper> updateStudents = new Dictionary<string, DSXmlHelper>();
                #region 查詢經緯度
                //每一個學生
                foreach ( XmlElement stuElement in addressHelper.GetElements("Student") )
                {
                    DSXmlHelper stuHelper = new DSXmlHelper(stuElement);
                    string id = stuElement.GetAttribute("ID");
                    //三種地址
                    foreach ( string addressType in new string[] { "PermanentAddress", "MailingAddress", "OtherAddresses" } )
                    {
                        XmlElement addressInfo = stuHelper.GetElement(addressType + "/AddressList/Address");
                        if ( addressInfo != null )
                        {
                            DSXmlHelper helper = new DSXmlHelper(addressInfo);
                            string address = helper.GetText("County") + helper.GetText("Town") + helper.GetText("DetailAddress");
                            //沒有經緯度且有地址才查
                            if ( address != "" && helper.GetText("Longitude") == "" && helper.GetText("Latitude") == "" )
                            {
                                #region 取得經緯度，如有經緯度則建立學生修改的Request
                                try
                                {
                                    DSXmlHelper h = new DSXmlHelper("Request");
                                    h.AddText(".", address);
                                    DSResponse rsp = SmartSchool.Feature.FeatureBase.CallService("SmartSchool.Common.QueryCoordinates", new DSRequest(h));
                                    h = rsp.GetContent();
                                    if ( h.GetElement("Error") == null )
                                    {
                                        string latitude = h.GetText("Latitude");
                                        string longitude = h.GetText("Longitude");
                                        //經緯度變更
                                        if ( latitude != "" && longitude != "" )
                                        {
                                            if ( !helper.PathExist("Latitude") )
                                                helper.AddElement("Latitude");

                                            if ( !helper.PathExist("Longitude") )
                                                helper.AddElement("Longitude");

                                            helper.SetText("Latitude", latitude);
                                            helper.SetText("Longitude", longitude);
                                            if ( !updateStudents.ContainsKey(id) )
                                            {
                                                DSXmlHelper newReq = new DSXmlHelper("req");
                                                newReq.AddElement("Student");
                                                newReq.AddElement("Student", "Field");
                                                newReq.AddElement("Student", "Condition");
                                                newReq.AddElement("Student/Condition", "ID", id);
                                                updateStudents.Add(id, newReq);
                                            }
                                            DSXmlHelper req = updateStudents[id];
                                            req.AddElement("Student/Field", addressType);
                                            req.AddElement("Student/Field/" + addressType, "AddressList");
                                            req.AddElement("Student/Field/" + addressType + "/AddressList", "Address");
                                            foreach ( string field in new string[] { "County", "Town", "ZipCode", "DetailAddress", "Longitude", "Latitude" } )
                                            {
                                                req.AddElement("Student/Field/" + addressType + "/AddressList/Address", field, helper.GetText(field));
                                            }
                                        }
                                    }
                                }
                                catch
                                { }
                                #endregion
                            }
                        }
                    }
                    processedCount++;
                    bkwGetGlobalPoint.ReportProgress(processedCount * activePackages * 100 / allSelectionCount, "經緯度查詢中...");
                }
                #endregion
                //更新學生住址資料
                #region 更新學生住址資料
                if ( updateStudents.Count > 0 )
                {
                    XmlDocument doc = new XmlDocument();
                    XmlElement ele = doc.CreateElement("UpdateStudentList");
                    foreach ( DSXmlHelper helper in updateStudents.Values )
                    {
                        ele.AppendChild(doc.ImportNode(helper.GetElement("Student"), true));
                    }
                    try
                    {
                        SmartSchool.Feature.EditStudent.Update(new DSRequest(ele));
                    }
                    catch { }
                }
                #endregion
            }
            for ( int i = 0 ; i < otherThreads.Length ; i++ )
            {
                otherThreads[i].Join();
            }
        }

        private void loadSubjectScore(object item)
        {
            List<List<string>> packages = (List<List<string>>)item;
            try
            {

                foreach ( List<string> package in packages )
                {
                    DSXmlHelper addressHelper = SmartSchool.Feature.QueryStudent.GetAddressWithID(package.ToArray()).GetContent();
                    Dictionary<string, DSXmlHelper> updateStudents = new Dictionary<string, DSXmlHelper>();
                    #region 查詢經緯度
                    //每一個學生
                    foreach ( XmlElement stuElement in addressHelper.GetElements("Student") )
                    {
                        DSXmlHelper stuHelper = new DSXmlHelper(stuElement);
                        string id = stuElement.GetAttribute("ID");
                        //三種地址
                        foreach ( string addressType in new string[] { "PermanentAddress", "MailingAddress", "OtherAddresses" } )
                        {
                            XmlElement addressInfo = stuHelper.GetElement(addressType + "/AddressList/Address");
                            if ( addressInfo != null )
                            {
                                DSXmlHelper helper = new DSXmlHelper(addressInfo);
                                string address = helper.GetText("County") + helper.GetText("Town") + helper.GetText("DetailAddress");
                                //沒有經緯度且有地址才查
                                if ( address != "" && helper.GetText("Longitude") == "" && helper.GetText("Latitude") == "" )
                                {
                                    #region 取得經緯度，如有經緯度則建立學生修改的Request
                                    try
                                    {
                                        DSXmlHelper h = new DSXmlHelper("Request");
                                        h.AddText(".", address);
                                        DSResponse rsp = SmartSchool.Feature.FeatureBase.CallService("SmartSchool.Common.QueryCoordinates", new DSRequest(h));
                                        h = rsp.GetContent();
                                        if ( h.GetElement("Error") == null )
                                        {
                                            string latitude = h.GetText("Latitude");
                                            string longitude = h.GetText("Longitude");
                                            //經緯度變更
                                            if ( latitude != "" && longitude != "" )
                                            {
                                                helper.SetText("Latitude", latitude);
                                                helper.SetText("Longitude", longitude);
                                                if ( !updateStudents.ContainsKey(id) )
                                                {
                                                    DSXmlHelper newReq = new DSXmlHelper("req");
                                                    newReq.AddElement("Student");
                                                    newReq.AddElement("Student", "Field");
                                                    newReq.AddElement("Student", "Condition");
                                                    newReq.AddElement("Student/Condition", "ID", id);
                                                    updateStudents.Add(id, newReq);
                                                }
                                                DSXmlHelper req = updateStudents[id];
                                                req.AddElement("Student/Field", addressType);
                                                req.AddElement("Student/Field/" + addressType, "AddressList");
                                                req.AddElement("Student/Field/" + addressType + "/AddressList", "Address");
                                                foreach ( string field in new string[] { "County", "Town", "ZipCode", "DetailAddress", "Longitude", "Latitude" } )
                                                {
                                                    req.AddElement("Student/Field/" + addressType + "/AddressList/Address", field, helper.GetText(field));
                                                }
                                            }
                                        }
                                    }
                                    catch
                                    { }
                                    #endregion
                                }
                            }
                        }
                    }
                    #endregion
                    //更新學生住址資料
                    #region 更新學生住址資料
                    if ( updateStudents.Count > 0 )
                    {
                        XmlDocument doc = new XmlDocument();
                        XmlElement ele = doc.CreateElement("UpdateStudentList");
                        foreach ( DSXmlHelper helper in updateStudents.Values )
                        {
                            ele.AppendChild(doc.ImportNode(helper.GetElement("Student"), true));
                        }
                        try
                        {
                            SmartSchool.Feature.EditStudent.Update(new DSRequest(ele));
                        }
                        catch { }
                    }
                    #endregion
                }
            }
            catch ( Exception ex ) { }
        }

        void bkwGetGlobalPoint_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MotherForm.SetStatusBarMessage("" + e.UserState, e.ProgressPercentage);
        }

        void bkwGetGlobalPoint_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MotherForm.SetStatusBarMessage("經緯度查詢完成。");
        }

        private void buttonItem3_Click(object sender, EventArgs e)
        {
            if ( bkwGetGlobalPoint.IsBusy )
            {
                MsgBox.Show("功能忙碌中。");
            }
            else
            {
                bkwGetGlobalPoint.RunWorkerAsync();
            }
        }

        private void btnPerm_Click(object sender, EventArgs e)
        {
            List<BriefStudentData> students = SmartSchool.StudentRelated.Student.Instance.SelectionStudents;
            GMapForm map = new GMapForm(students, AddressType.PermanentAddress);
            map.ShowDialog();
        }

        private void btnConn_Click(object sender, EventArgs e)
        {
            List<BriefStudentData> students = SmartSchool.StudentRelated.Student.Instance.SelectionStudents;
            GMapForm map = new GMapForm(students, AddressType.MailingAddress);
            map.ShowDialog();
        }

        private void btnOther_Click(object sender, EventArgs e)
        {
            List<BriefStudentData> students = SmartSchool.StudentRelated.Student.Instance.SelectionStudents;
            GMapForm map = new GMapForm(students, AddressType.OtherAddresses);
            map.ShowDialog();
        }
    }
}
