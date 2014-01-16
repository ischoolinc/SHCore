using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SmartSchool.ErrorReporting
{
    /// <summary>
    /// 上傳到 AWS 訊息資料欄位,="",因為 AWS 儲存不允許 NULL or 空字串，所以用"Null"
    /// </summary>
    public class AWSErrMsg
    {
        public string GUID { get; set; }

        public string EnvironmentUserName { get; set; }

        public string EnvironmentMachineName { get; set; }

        public string EnvironmentOSVersion { get; set; }

        public string EnvironmentServicePack { get; set; }

        public string EnvironmentPlatform { get; set; }

        public string ExceptionContent { get; set; }

        public string ExceptionContentHead { get; set; }

        public MemoryStream ImageStream { get; set; }

        public string AuthServer { get; set; }

        public string AuthLoginAccount { get; set; }

        public string ComputerTime { get; set; }

        public string StackTraceMethods { get; set; }

        public string StackTraceAssemblys { get; set; }

        public string DeploySources { get; set; }

        public string OperatorMessage { get; set; }       
    }
}
