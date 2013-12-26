using System;
using System.Collections.Generic;
using System.Text;
using FISCA.DSAUtil;
using SmartSchool.Common;

namespace SmartSchool.Feature.ScoreCalcRule
{
    [QueryRequest()]
    public class RemoveScoreCalcRule
    {
        public static void Delete(string id)
        {
            DSRequest dsreq = new DSRequest("<DeleteRequest><ScoreCalcRule><ID>" + id + "</ID></ScoreCalcRule></DeleteRequest>");
            CurrentUser.Instance.CallService("SmartSchool.ScoreCalcRule.DeleteScoreCalcRule", dsreq);
        }
    }
}
