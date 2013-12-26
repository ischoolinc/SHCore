using System;
using System.Collections.Generic;
using System.Text;
using FISCA.DSAUtil;
using SmartSchool.Common;

namespace SmartSchool.Feature.SubjectTable
{
    [QueryRequest()]
    public class RemoveSubejctTable
    {
        public static void Delete(string id)
        {
            DSRequest dsreq = new DSRequest("<Request><SubjectTable><ID>" + id + "</ID></SubjectTable></Request>");
            CurrentUser.Instance.CallService("SmartSchool.SubjectTable.Delete", dsreq);
        }
    }
}
