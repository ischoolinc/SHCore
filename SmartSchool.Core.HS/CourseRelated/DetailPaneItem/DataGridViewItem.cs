using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;
using SmartSchool.CourseRelated.ScoreDataGridView;
using DevComponents.DotNetBar;
using FISCA.DSAUtil;
using SmartSchool.Feature.Course;
using K12.Data;
using System.Linq;
using System.Xml.Linq;
using FISCA.Data;

namespace SmartSchool.CourseRelated.DetailPaneItem
{
    internal partial class DataGridViewItem : PalmerwormItem
    {
        private DataGridViewHelper _helper;
        private Dictionary<string, bool> _showItems;
        private List<string> colSCList = new List<string>();
        private List<string> colSCEList = new List<string>();
        Dictionary<string, Dictionary<string, string>> oldLogDataDict = new Dictionary<string, Dictionary<string, string>>();
        Dictionary<string, Dictionary<string, string>> newLogDataDict = new Dictionary<string, Dictionary<string, string>>();

        // �ʦҳ]�w��T
        List<SmartSchool.DAO.ScoreValueMangInfo> ScoreValueMangInfoList = new List<SmartSchool.DAO.ScoreValueMangInfo>();

        // �ʦҳ]�w��r��ӭ�
        Dictionary<string, string> ScoreValueMangTextDict = new Dictionary<string, string>();

        public DataGridViewItem()
        {
            InitializeComponent();
            Title = "�s�覨�Z";
        }

        public bool IsValid
        {
            get
            {
                return _helper.IsValid();
            }
        }

        public override void Save()
        {
            colSCList.Clear();
            colSCEList.Clear();
            colSCList.Add("�ҵ{���Z");
            colSCList.Add("�ή�з�");
            colSCList.Add("�ɦҼз�");
            colSCList.Add("�������w�`���Z");
            colSCList.Add("�Ƶ�");

            // ���o�q�����
            for (int col = 5; col < dataGridView1.ColumnCount; col++)
            {
                string colName = dataGridView1.Columns[col].Name;
                if (!colSCList.Contains(colName))
                    colSCEList.Add(colName);
            }

            // �¦s���g�k
            // ���ͷs�W���Z Request
            //int insertStudentCount = 0;
            //int updateStudentCount = 0;
            //int deleteStudentCount = 0;
            //int usCount = 0;

            //DSXmlHelper insertHelper = new DSXmlHelper("Request");
            //DSXmlHelper updateHelper = new DSXmlHelper("Request");
            //DSXmlHelper deleteHelper = new DSXmlHelper("Request");
            //DSXmlHelper usHelper = new DSXmlHelper("Request");

            //insertHelper.AddElement("ScoreSheetList");
            //updateHelper.AddElement("ScoreSheetList");
            //deleteHelper.AddElement("ScoreSheet");

            UpdateHelper insertSCETake = new UpdateHelper();
            UpdateHelper updateSCETake = new UpdateHelper();
            UpdateHelper deleteSCETake = new UpdateHelper();
            List<string> insertSQLList = new List<string>();
            List<string> updateSQLList = new List<string>();
            List<string> deleteSQLList = new List<string>();


            newLogDataDict.Clear();

            DataTable newScAttend = new DataTable();
            newScAttend.Columns.Add("sc_attend_id");
            newScAttend.Columns.Add("passing_standard");
            newScAttend.Columns.Add("makeup_standard");
            newScAttend.Columns.Add("remark");
            newScAttend.Columns.Add("designate_final_score");
            newScAttend.Columns.Add("score");

            // �B�z�s�W����s
            Dictionary<string, int> newColIdxDict = new Dictionary<string, int>();
            foreach (DataGridViewColumn dgc in dataGridView1.Columns)
            {
                if (!newColIdxDict.ContainsKey(dgc.Name))
                    newColIdxDict.Add(dgc.Name, dgc.Index);
            }


            newLogDataDict.Clear();
            foreach (DataGridViewRow drv in dataGridView1.Rows)
            {
                Dictionary<string, string> value = new Dictionary<string, string>();
                foreach (string str in newColIdxDict.Keys)
                {
                    string val = "";
                    if (drv.Cells[str].Value != null)
                        val = drv.Cells[str].Value.ToString();
                    value.Add(str, val);
                }

                string key = drv.Cells["�ǥͨt�νs��"].Value.ToString();
                if (!newLogDataDict.ContainsKey(key))
                    newLogDataDict.Add(key, value);
            }


            insertSQLList.Clear(); updateSQLList.Clear(); deleteSQLList.Clear();

            Dictionary<string, string> sce_take_extDDict = new Dictionary<string, string>();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    IExamCell ic = cell.Tag as IExamCell;
                    if (ic == null) continue;

                    if (string.IsNullOrEmpty(ic.Key))
                        continue;

                    if (!sce_take_extDDict.ContainsKey(ic.Key))
                        sce_take_extDDict.Add(ic.Key, "");
                }
            }

            if (sce_take_extDDict.Count > 0)
            {
                string strSQL = string.Format(@"
                SELECT
                    id,
                    extension
                FROM
                    sce_take
                WHERE
                    extension <>'' AND id IN({0})", string.Join(",", sce_take_extDDict.Keys.ToArray()));

                sce_take_extDDict.Clear();

                QueryHelper qh1 = new QueryHelper();
                DataTable dt = qh1.Select(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    string id = dr["id"] + "";
                    string ext = dr["extension"] + "";
                    if (!sce_take_extDDict.ContainsKey(id))
                        sce_take_extDDict.Add(id, ext);
                }
            }            


            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                string attendid = row.Tag.ToString();

                // �B�z�s�W���
                DataRow dr = newScAttend.NewRow();

                dr["sc_attend_id"] = attendid;



                foreach (DataGridViewCell cell in row.Cells)
                {
                    // �쥻�ҵ{���Z
                    if (newColIdxDict.ContainsKey("�ҵ{���Z"))
                    {
                        if (cell.ColumnIndex == newColIdxDict["�ҵ{���Z"])
                        {
                            if (cell.Value == null)
                                dr["score"] = "";
                            else
                                dr["score"] = cell.Value.ToString();
                        }
                    }

                    // �B�z�s�W������
                    if (newColIdxDict.ContainsKey("�ή�з�"))
                    {
                        if (cell.ColumnIndex == newColIdxDict["�ή�з�"])
                        {
                            if (cell.Value == null)
                                dr["passing_standard"] = "";
                            else
                                dr["passing_standard"] = cell.Value.ToString();
                        }
                    }
                    if (newColIdxDict.ContainsKey("�ɦҼз�"))
                    {
                        if (cell.ColumnIndex == newColIdxDict["�ɦҼз�"])
                        {
                            if (cell.Value == null)
                                dr["makeup_standard"] = "";
                            else
                                dr["makeup_standard"] = cell.Value.ToString();
                        }
                    }
                    if (newColIdxDict.ContainsKey("�������w�`���Z"))
                    {
                        if (cell.ColumnIndex == newColIdxDict["�������w�`���Z"])
                        {
                            if (cell.Value == null)
                                dr["designate_final_score"] = "";
                            else
                                dr["designate_final_score"] = cell.Value.ToString();
                        }
                    }
                    if (newColIdxDict.ContainsKey("�Ƶ�"))
                    {
                        if (cell.ColumnIndex == newColIdxDict["�Ƶ�"])
                        {
                            if (cell.Value == null)
                                dr["remark"] = "";
                            else
                                dr["remark"] = cell.Value.ToString();
                        }
                    }


                    IExamCell ic = cell.Tag as IExamCell;
                    if (ic == null) continue;
                    ColumnSetting setting = dataGridView1.Columns[cell.ColumnIndex].Tag as ColumnSetting;
                    string examid = setting.Key;

                    // �B�z�ʦ�
                    string score = "null";
                    string strValue = "";
                    if (!string.IsNullOrWhiteSpace(ic.GetValue()))
                    {
                        string value = ic.GetValue().Replace(" ", "");
                        if (ScoreValueMangTextDict.ContainsKey(value))
                        {
                            //score = "-1";
                            score = ScoreValueMangTextDict[value];
                            strValue = value;
                        }
                        else
                        {
                            score = ic.GetValue();
                        }
                    }

                    if (ic is ScoreExamCell && ic.IsDirty)
                    {
                        //usCount++;
                        //usHelper.AddElement("Attend");
                        //usHelper.AddElement("Attend", "Score", ic.GetValue());
                        //usHelper.AddElement("Attend", "ID", attendid);
                        // �b�ҵ{�Τ@�B�z
                    }
                    else if (string.IsNullOrEmpty(ic.Key) && ic.IsDirty)
                    {
                        //insertStudentCount++;
                        //insertHelper.AddElement("ScoreSheetList", "ScoreSheet");
                        //insertHelper.AddElement("ScoreSheetList/ScoreSheet", "ExamID", examid);
                        //insertHelper.AddElement("ScoreSheetList/ScoreSheet", "AttendID", attendid);
                        //insertHelper.AddElement("ScoreSheetList/ScoreSheet", "Score", ic.GetValue());                       
                        // �ݭn�g�J�ʦ�
                        string insertSQL = "";
                        if (string.IsNullOrEmpty(strValue))
                        {
                            // �@��
                            insertSQL = string.Format(@"
                            INSERT INTO
                                sce_take(
                                    ref_exam_id,
                                    ref_sc_attend_id,
                                    score
                                )
                            VALUES
                                ({0}, {1}, {2});
                            ", examid, attendid, score);
                        }
                        else
                        {
                            // ���� xml �N�ʦ�key��Ƽg�J extension
                            XElement elmRoot = new XElement("Extension");
                            elmRoot.SetElementValue("UseText", strValue);
                            insertSQL = string.Format(@"
                            INSERT INTO
                                sce_take(
                                    ref_exam_id,
                                    ref_sc_attend_id,
                                    score,
                                    extension
                                )
                            VALUES
                                ({0}, {1}, {2},{3});
                            ", examid, attendid, score, elmRoot.ToString());
                        }

                        insertSQLList.Add(insertSQL);
                    }
                    else if (!string.IsNullOrEmpty(ic.Key) && ic.IsDirty && !string.IsNullOrEmpty(ic.GetValue()))
                    {
                        //updateStudentCount++;
                        //updateHelper.AddElement("ScoreSheetList", "ScoreSheet");
                        //updateHelper.AddElement("ScoreSheetList/ScoreSheet", "Score", ic.GetValue());
                        //updateHelper.AddElement("ScoreSheetList/ScoreSheet", "ID", ic.Key);
                        string updateSQL = "";

                        if (string.IsNullOrEmpty(strValue))
                        {
                            updateSQL = string.Format(@"
                            UPDATE
                                sce_take
                            SET
                                score = {0}
                            WHERE
                                ID = {1};
                            ", score, ic.Key);
                        }
                        else
                        {
                            try
                            {
                                string ext = "";
                                if (sce_take_extDDict.ContainsKey(ic.Key))
                                    ext = sce_take_extDDict[ic.Key];

                                XElement elmRoot = null;
                                if (ext == "")
                                {
                                    elmRoot = new XElement("Extension");
                                }
                                else
                                {
                                    // �쥻�w�����
                                    elmRoot = XElement.Parse(ext);
                                }

                                elmRoot.SetElementValue("UseText", strValue);
                                updateSQL = string.Format(@"
                            UPDATE
                                sce_take
                            SET
                                score = {0},
                                extension = '{2}' 
                            WHERE
                                ID = {1};
                            ", score, ic.Key, elmRoot.ToString());
                            }
                            catch (Exception ex)
                            { Console.WriteLine(ex.Message); }

                        }

                        updateSQLList.Add(updateSQL);
                    }
                    else if (!string.IsNullOrEmpty(ic.Key) && ic.IsDirty && string.IsNullOrEmpty(ic.GetValue()))
                    {
                        //deleteStudentCount++;
                        //deleteHelper.AddElement("ScoreSheet", "ID", ic.Key);
                        string deleteSQL = "DELETE FROM sce_take WHERE ID = " + ic.Key + ";";
                        deleteSQLList.Add(deleteSQL);
                    }
                }

                newScAttend.Rows.Add(dr);

            }

            //if (insertStudentCount > 0)
            //    EditCourse.InsertSCEScore(new DSRequest(insertHelper));
            //if (updateStudentCount > 0)
            //    EditCourse.UpdateSCEScore(new DSRequest(updateHelper));
            //if (deleteStudentCount > 0)
            //    EditCourse.DeleteSCEScore(new DSRequest(deleteHelper));
            //if (usCount > 0)
            //    EditCourse.UpdateAttend(usHelper);

            if (insertSQLList.Count > 0)
            {
                try
                {
                    insertSCETake.Execute(insertSQLList);

                    CurrentUser.Instance.AppLog.Write(SmartSchool.ApplicationLog.EntityType.Course, "�ҵ{���Z��J", RunningID, "�s�W" + insertSQLList.Count + "�����q���Z", "�ҵ{", "");

                }
                catch (Exception ex)
                {
                    MsgBox.Show(ex.Message);
                }
            }

            if (updateSQLList.Count > 0)
            {
                try
                {
                    updateSCETake.Execute(updateSQLList);
                    CurrentUser.Instance.AppLog.Write(SmartSchool.ApplicationLog.EntityType.Course, "�ҵ{���Z��J", RunningID, "��s" + updateSQLList.Count + "�����q���Z", "�ҵ{", "");
                }
                catch (Exception ex)
                {
                    MsgBox.Show(ex.Message);
                }
            }

            if (deleteSQLList.Count > 0)
            {
                try
                {
                    deleteSCETake.Execute(deleteSQLList);
                    CurrentUser.Instance.AppLog.Write(SmartSchool.ApplicationLog.EntityType.Course, "�ҵ{���Z��J", RunningID, "�R��" + deleteSQLList.Count + "�����q���Z", "�ҵ{", "");
                }
                catch (Exception ex)
                {
                    MsgBox.Show(ex.Message);
                }
            }

            if (newScAttend.Rows.Count > 0)
            {
                K12.Data.UpdateHelper upSCattend = new K12.Data.UpdateHelper();
                List<string> updateList = new List<string>();
                foreach (DataRow dr in newScAttend.Rows)
                {
                    string passing_standard = "null", makeup_standard = "null", designate_final_score = "null", course_score = "null";

                    if (dr["passing_standard"] != null && dr["passing_standard"].ToString() != "")
                        passing_standard = dr["passing_standard"].ToString();

                    if (dr["makeup_standard"] != null && dr["makeup_standard"].ToString() != "")
                        makeup_standard = dr["makeup_standard"].ToString();

                    if (dr["designate_final_score"] != null && dr["designate_final_score"].ToString() != "")
                        designate_final_score = dr["designate_final_score"].ToString();

                    if (dr["score"] != null && dr["score"].ToString() != "")
                    {
                        course_score = dr["score"].ToString();
                    }

                    string qry = "UPDATE " +
                        "sc_attend " +
                        "SET " +
                        "passing_standard=" + passing_standard +
                        ",makeup_standard=" + makeup_standard +
                        ",remark='" + dr["remark"].ToString() + "'" +
                        ",designate_final_score=" + designate_final_score + "" +
                        ",score = " + course_score + " " +
                        "WHERE " +
                        "id = " + dr["sc_attend_id"].ToString() + ";";
                    updateList.Add(qry);
                }


                //CurrentUser.Instance.AppLog.Write()

                if (updateList.Count > 0)
                {
                    // �B�z�ҵ{���Z����s
                    StringBuilder sbLog = new StringBuilder();

                    List<string> msgList = new List<string>();
                    foreach (string skey in oldLogDataDict.Keys)
                    {
                        msgList.Clear();
                        string class_name = oldLogDataDict[skey]["�Z��"].ToString();
                        string seat_no = oldLogDataDict[skey]["�y��"].ToString();
                        string name = oldLogDataDict[skey]["�m�W"].ToString();
                        if (newLogDataDict.ContainsKey(skey))
                        {
                            foreach (string colKey in colSCList)
                            {
                                if (oldLogDataDict[skey].ContainsKey(colKey) && newLogDataDict[skey].ContainsKey(colKey))
                                {
                                    if (oldLogDataDict[skey][colKey] != newLogDataDict[skey][colKey])
                                    {
                                        string msg = "���u" + colKey + "�v�ѡu" + oldLogDataDict[skey][colKey] + "�v�ܧ󬰡u" + newLogDataDict[skey][colKey] + "�v";
                                        msgList.Add(msg);
                                    }
                                }
                            }
                        }
                        if (msgList.Count > 0)
                        {
                            sbLog.AppendLine("�Z�šG" + class_name + "�A�y���G" + seat_no + "�A�m�W�G" + name + "," + string.Join(",", msgList.ToArray()));
                        }
                    }
                    CurrentUser.Instance.AppLog.Write(SmartSchool.ApplicationLog.EntityType.Course, "�ҵ{���Z��J", RunningID, sbLog.ToString(), "�ҵ{", "");


                    upSCattend.Execute(updateList);
                }
            }

            SaveButtonVisible = false;
            LoadContent(RunningID);
        }

        protected override object OnBackgroundWorkerWorking()
        {
            // ���o�ʦҳ]�w���
            ScoreValueMangInfoList = QueryData.GetScoreValueMangInfoList();
            return new SmartSchoolDataProvider(RunningID, cbShowAllStudent.Checked);
        }

        protected override void OnBackgroundWorkerCompleted(object result)
        {
            //�]���o�O�ĤG�Ӱ������ Callback�A�ܥi��ϥΪ̾ާ@�ܧ֡A�ҥH�b�S������o���ɡA�e���N�Q���F�A���ˬd�i��|�y�����~�I
            if (IsDisposed) return;

            // �ʦҳ]�w����            
            ScoreValueMangTextDict.Clear();
            foreach (SmartSchool.DAO.ScoreValueMangInfo info in ScoreValueMangInfoList)
            {
                if (!ScoreValueMangTextDict.ContainsKey(info.UseText))
                    ScoreValueMangTextDict.Add(info.UseText, info.UseValue);
            }

            _valueManager.AddValue("IsDirty", false.ToString());
            IDataProvider provider = result as IDataProvider;

            _helper = new DataGridViewHelper(dataGridView1, provider);
            // �]�w�ʦҤ�r�i�H��J���ǭ�
            _helper.SetStringValues(ScoreValueMangTextDict.Keys.ToList());

            _helper.DirtyChanged += new EventHandler<DirtyChangedEventArgs>(_helper_DirtyChanged);
            _helper.Fill();




            if (_showItems == null)
            {
                _showItems = new Dictionary<string, bool>();
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    if (column.Name == "�ǥͨt�νs��")
                        _showItems.Add(column.Name, false);
                    else
                        _showItems.Add(column.Name, true);
                }
            }
            // �]�w��ܶ���
            btnShowItems.SubItems.Clear();
            btnShowItems.AutoExpandOnClick = true;
            List<string> displayItem = new List<string>();
            Dictionary<string, int> colIdxDict = new Dictionary<string, int>();

            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                if (!colIdxDict.ContainsKey(column.Name))
                    colIdxDict.Add(column.Name, column.Index);

                bool visible = _showItems[column.Name];
                CheckBoxItem item = new CheckBoxItem(column.Name, column.Name);
                btnShowItems.SubItems.Add(item);
                item.AutoCollapseOnClick = false;
                item.Checked = visible;
                item.CheckedChanged += new CheckBoxChangeEventHandler(item_CheckedChanged);
                if (visible) displayItem.Add(column.Name);
            }

            // log �ϥ�
            oldLogDataDict.Clear();
            foreach (DataGridViewRow drv in dataGridView1.Rows)
            {
                Dictionary<string, string> value = new Dictionary<string, string>();
                foreach (string str in colIdxDict.Keys)
                {
                    string val = "";
                    if (drv.Cells[str].Value != null)
                        val = drv.Cells[str].Value.ToString();
                    value.Add(str, val);
                }

                string key = drv.Cells["�ǥͨt�νs��"].Value.ToString();
                if (!oldLogDataDict.ContainsKey(key))
                    oldLogDataDict.Add(key, value);
            }
            _helper.ResetDisplayColumn(displayItem);
        }

        void _helper_DirtyChanged(object sender, DirtyChangedEventArgs e)
        {
            OnValueChanged("IsDirty", e.Dirty.ToString());
        }

        void item_CheckedChanged(object sender, CheckBoxChangeEventArgs e)
        {
            List<string> displays = new List<string>();
            foreach (BaseItem item in btnShowItems.SubItems)
            {
                CheckBoxItem citem = (CheckBoxItem)item;
                if (citem.Checked)
                    displays.Add(citem.Text);
                _showItems[item.Text] = citem.Checked;
            }
            _helper.ResetDisplayColumn(displays);
        }

        public override void Undo()
        {
            _helper.ResetValue();
            _valueManager.AddValue("IsDirty", false.ToString());
            SaveButtonVisible = false;
        }

        private void cbShowAllStudent_CheckedChanged(object sender, EventArgs e)
        {
            LoadContent(RunningID);
        }
    }
}
