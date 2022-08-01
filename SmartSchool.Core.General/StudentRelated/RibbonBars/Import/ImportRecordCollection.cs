using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SmartSchool.StudentRelated.RibbonBars.Import
{
    internal class ImportRecordCollection
    {
        Dictionary<string, ImportRecord> _by_identity;
        Dictionary<string, ImportRecord> _by_id_number;
        Dictionary<string, ImportRecord> _by_student_number;
        Dictionary<string, ImportRecord> _by_login_name;

        public ImportRecordCollection()
        {
            _by_identity = new Dictionary<string, ImportRecord>();
            _by_id_number = new Dictionary<string, ImportRecord>();
            _by_student_number = new Dictionary<string, ImportRecord>();
            _by_login_name = new Dictionary<string, ImportRecord>();
        }

        public void Add(ImportRecord record)
        {
            // 學生系統編號可以各種都可以
            _by_identity.Add(record.Identity, record);

            // 這三類目前只支援學生狀態唯一般。
            if (record.StudentStatus == "一般")
            {
                if (!string.IsNullOrEmpty(record.IDNumber))
                    _by_id_number.Add(record.IDNumber, record);

                if (!string.IsNullOrEmpty(record.StudentNumber))
                    _by_student_number.Add(record.StudentNumber, record);

                if (!string.IsNullOrEmpty(record.SALoginName))
                    _by_login_name.Add(record.SALoginName.ToLower(), record);
            }
        }

        public ImportRecord GetByIdentity(string key)
        {
            return _by_identity[key];
        }

        public ImportRecord GetByIdNumber(string key)
        {
            return _by_id_number[key];
        }

        public ImportRecord GetByStudentNumber(string key)
        {
            return _by_student_number[key];
        }

        public ImportRecord GetBySALoginName(string key)
        {
            return _by_login_name[key];
        }

        public bool ContainIdentity(string key)
        {
            return _by_identity.ContainsKey(key);
        }

        public bool ContainIdNumber(string key)
        {
            return _by_id_number.ContainsKey(key);
        }

        public bool ContainStudentNumber(string key)
        {
            return _by_student_number.ContainsKey(key);
        }

        public bool ContainSALoginName(string key)
        {
            return _by_login_name.ContainsKey(key);
        }

        public List<ImportRecord> GetDuplicateIdNumberList(bool chkID)
        {
            // 檢查身分證號有2筆以上回傳
            List<ImportRecord> Values = new List<ImportRecord>();

            Dictionary<string, List<ImportRecord>> checkDict = new Dictionary<string, List<ImportRecord>>();

            List<ImportRecord> CheckDataList = new List<ImportRecord>();

            if (chkID)
                CheckDataList = _by_identity.Values.ToList();
            else
                CheckDataList = _by_id_number.Values.ToList();

            foreach (ImportRecord each in CheckDataList)
            {
                if (!checkDict.ContainsKey(each.IDNumber))
                    checkDict.Add(each.IDNumber, new List<ImportRecord>());

                checkDict[each.IDNumber].Add(each);
            }

            foreach (string key in checkDict.Keys)
            {
                if (checkDict[key].Count > 1 && key != "")
                {
                    foreach (ImportRecord each in checkDict[key])
                        Values.Add(each);
                }
            }

            return Values;
            //Dictionary<string, ImportRecord> duplicateList = new Dictionary<string, ImportRecord>();
            //Dictionary<string, ImportRecord> checkList = new Dictionary<string, ImportRecord>();

            //foreach (ImportRecord each in _by_identity.Values)
            //{

            //    if (string.IsNullOrEmpty(each.IDNumber)) continue;

            //    if (checkList.ContainsKey(each.IDNumber))
            //        duplicateList.Add(each.Identity, each);
            //    else
            //        checkList.Add(each.IDNumber, each);
            //}

            ////將重覆資料的第一筆也加入到 Duplicate 清單中，因為在檢查時會略過第一筆(因為出現第二筆才當重覆)。
            //List<ImportRecord> copyList = new List<ImportRecord>(duplicateList.Values);
            //foreach (ImportRecord each in copyList)
            //{
            //    ImportRecord origin = checkList[each.IDNumber];



            //    duplicateList.Add(origin.Identity, origin);
            //}
            //return new List<ImportRecord>(duplicateList.Values);
        }

        public List<ImportRecord> GetDuplicateStudentNumberList(bool chkID)
        {
            // 檢查學號有2筆以上回傳
            List<ImportRecord> Values = new List<ImportRecord>();

            Dictionary<string, List<ImportRecord>> checkDict = new Dictionary<string, List<ImportRecord>>();

            List<ImportRecord> CheckDataList = new List<ImportRecord>();

            if (chkID)
                CheckDataList = _by_identity.Values.ToList();
            else
                CheckDataList = _by_student_number.Values.ToList();

            foreach (ImportRecord each in CheckDataList)
            {
                if (!checkDict.ContainsKey(each.StudentNumber))
                    checkDict.Add(each.StudentNumber, new List<ImportRecord>());

                checkDict[each.StudentNumber].Add(each);
            }

            foreach (string key in checkDict.Keys)
            {
                if (checkDict[key].Count > 1 && key != "")
                {
                    foreach (ImportRecord each in checkDict[key])
                        Values.Add(each);
                }
            }

            return Values;


            //Dictionary<string, ImportRecord> duplicateList = new Dictionary<string, ImportRecord>();
            //Dictionary<string, ImportRecord> checkList = new Dictionary<string, ImportRecord>();

            //foreach (ImportRecord each in _by_identity.Values)
            //{
            //    if (string.IsNullOrEmpty(each.StudentNumber)) continue;

            //    if (checkList.ContainsKey(each.StudentNumber))
            //        duplicateList.Add(each.Identity, each);
            //    else
            //        checkList.Add(each.StudentNumber, each);
            //}

            ////將重覆資料的第一筆也加入到 Duplicate 清單中，因為在檢查時會略過第一筆(因為出現第二筆才當重覆)。
            //List<ImportRecord> copyList = new List<ImportRecord>(duplicateList.Values);
            //foreach (ImportRecord each in copyList)
            //{
            //    ImportRecord origin = checkList[each.StudentNumber];
            //    duplicateList.Add(origin.Identity, origin);
            //}

            //return new List<ImportRecord>(duplicateList.Values);

        }

        public List<ImportRecord> GetDuplicateSALoginNameList(bool chkID)
        {
            // 檢查學號有2筆以上回傳
            List<ImportRecord> Values = new List<ImportRecord>();

            Dictionary<string, List<ImportRecord>> checkDict = new Dictionary<string, List<ImportRecord>>();

            List<ImportRecord> CheckDataList = new List<ImportRecord>();

            if (chkID)
                CheckDataList = _by_identity.Values.ToList();
            else
                CheckDataList = _by_login_name.Values.ToList();

            foreach (ImportRecord each in CheckDataList)
            {
                if (!checkDict.ContainsKey(each.SALoginName.ToLower()))
                    checkDict.Add(each.SALoginName.ToLower(), new List<ImportRecord>());

                checkDict[each.SALoginName.ToLower()].Add(each);
            }

            foreach (string key in checkDict.Keys)
            {
                if (checkDict[key].Count > 1 && key != "")
                {
                    foreach (ImportRecord each in checkDict[key])
                        Values.Add(each);
                }
            }

            return Values;

            //Dictionary<string, ImportRecord> duplicateList = new Dictionary<string, ImportRecord>();
            //Dictionary<string, ImportRecord> checkList = new Dictionary<string, ImportRecord>();

            //foreach (ImportRecord each in _by_identity.Values)
            //{
            //    if (string.IsNullOrEmpty(each.SALoginName)) continue;

            //    if (checkList.ContainsKey(each.SALoginName))
            //        duplicateList.Add(each.Identity, each);
            //    else
            //        checkList.Add(each.SALoginName, each);
            //}

            ////將重覆資料的第一筆也加入到 Duplicate 清單中，因為在檢查時會略過第一筆(因為出現第二筆才當重覆)。
            //List<ImportRecord> copyList = new List<ImportRecord>(duplicateList.Values);
            //foreach (ImportRecord each in copyList)
            //{
            //    ImportRecord origin = checkList[each.SALoginName];
            //    duplicateList.Add(origin.Identity, origin);
            //}

            //return new List<ImportRecord>(duplicateList.Values);

        }
    }
}
