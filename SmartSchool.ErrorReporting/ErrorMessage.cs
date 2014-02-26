using System;
using System.Collections.Generic;
using System.Text;
//using FISCA.UDT;
using System.Xml;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Windows.Forms;
using System.Diagnostics;
using System.Configuration;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2;
using Amazon.SecurityToken;
using Amazon.Util;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;

namespace SmartSchool.ErrorReporting
{
    public class ErrorMessgae //: FISCA.UDT.ActiveRecord
    {
        public AWSErrMsg _AWSErrMsg;
        private PutItemResponse Rsp=null;
        private PutItemResponse RspIdx = null;
        private PutItemResponse RspDateIdx = null;
        public ErrorMessgae()
        {
            // 儲存錯誤相關訊息
            _AWSErrMsg = new AWSErrMsg();

            _AWSErrMsg.EnvironmentUserName = Environment.UserName;
            _AWSErrMsg.EnvironmentMachineName = Environment.MachineName;
            _AWSErrMsg.EnvironmentOSVersion = Environment.OSVersion.VersionString;
            _AWSErrMsg.EnvironmentServicePack = Environment.OSVersion.ServicePack;
            _AWSErrMsg.EnvironmentPlatform = Environment.OSVersion.Platform.ToString();

            _AWSErrMsg.ImageStream = null;
            _AWSErrMsg.ExceptionContent = "";
            _AWSErrMsg.ExceptionContentHead = "";
            _AWSErrMsg.AuthServer = FISCA.Authentication.DSAServices.IsLogined ? FISCA.Authentication.DSAServices.AccessPoint : "NotLogin";
            _AWSErrMsg.AuthLoginAccount = FISCA.Authentication.DSAServices.IsLogined ? FISCA.Authentication.DSAServices.UserAccount : "NotLogin";
            _AWSErrMsg.ComputerTime = DateTime.Now.ToString();
            _AWSErrMsg.StackTraceMethods = "";
            _AWSErrMsg.StackTraceAssemblys = "";
            _AWSErrMsg.DeploySources = "";
        }
        public ErrorMessgae(Exception ex)
            : this()
        {
            SetContent(ex);
            // Save
            try
            {
                GetApplicationSnapShot();
                GetDeploySources();
                Save();              
            }
            catch (Exception ex1)
            { 
                
            }
        }
        private void FillInStackTrace(Exception ex)
        {
            if (ex.InnerException != null)
                FillInStackTrace(ex.InnerException);
            var st = new StackTrace(ex);
            if (st != null && st.FrameCount > 0 && st.GetFrames() != null)
            {
                foreach (StackFrame frame in (st).GetFrames())
                {
                    _AWSErrMsg.StackTraceMethods += (_AWSErrMsg.StackTraceMethods == "" ? "" : "\n") + frame.GetMethod().ToString() + " in " + frame.GetMethod().DeclaringType.FullName;
                    _AWSErrMsg.StackTraceAssemblys += (_AWSErrMsg.StackTraceMethods == "" ? "" : "\n") + frame.GetMethod().Module.Assembly.FullName;
                }
            }            
        }

       
        #region 傳送到AWS DynamoDB, S3
        /// <summary>
        /// Save to AWS
        /// </summary>
        public void Save()
        {
            // AWS 連線，登入 DynamoDB,S3
            AmazonDynamoDBClient client;
            AmazonS3Client clientS3;
            var config = new AmazonDynamoDBConfig();

            // Login AWS name:ischool_user
            config.ServiceURL = "http://dynamodb.us-west-2.amazonaws.com";
            var AWSAccessKey = "AKIAIT7SNOYGBM5HM4KA";
            var AWSSecretKey = "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";

            bool isUploadImage = true;
            bool isUploadExceptionXML = true;

            // AWS 連線登入 DynamoDB
            client = new AmazonDynamoDBClient(AWSAccessKey, AWSSecretKey, config);
            // AWS 連線登入 S3
            clientS3 = new AmazonS3Client(AWSAccessKey, AWSSecretKey, Amazon.RegionEndpoint.USWest2);

            _AWSErrMsg.GUID = Guid.NewGuid().ToString();

            // 檢查是否有 Exception
            if (string.IsNullOrEmpty(_AWSErrMsg.ExceptionContent))
                isUploadExceptionXML = false;
            
            // 檢查是否有圖片
            if (_AWSErrMsg.ImageStream == null || _AWSErrMsg.ImageStream.Length == 0)
                isUploadImage = false;            

            // 有 Exception 才上傳
            if (isUploadExceptionXML)
            {
                // XML convert Stream
                byte[] ExceptionContentByte = Encoding.UTF8.GetBytes(_AWSErrMsg.ExceptionContent);
                MemoryStream ExceptionContentStream = new MemoryStream(ExceptionContentByte);

                PutObjectRequest putRequest1 = new PutObjectRequest
                {
                    BucketName = "ischool-error-report",
                    Key = _AWSErrMsg.GUID.ToString() + "_xml.xml",
                    InputStream = ExceptionContentStream
                };

                PutObjectResponse response1 = clientS3.PutObject(putRequest1);
            }
            
            // 有圖片才上傳
            if (isUploadImage)
            {
                // 圖片
                PutObjectRequest putRequest2 = new PutObjectRequest
                {
                    BucketName = "ischool-error-report",
                    Key = _AWSErrMsg.GUID.ToString() + "_image.png",
                    InputStream = _AWSErrMsg.ImageStream
                };
                PutObjectResponse response2 = clientS3.PutObject(putRequest2);
            }

            var Req = new PutItemRequest
            {
                TableName = "ischoolErrorReport",
                Item = new Dictionary<string, AttributeValue>() {
                {"GUID",new AttributeValue{S=_AWSErrMsg.GUID}},                   
                {"EnvironmentUserName",new AttributeValue{S=ParseString1(_AWSErrMsg.EnvironmentUserName)}},
                {"EnvironmentMachineName",new AttributeValue{S=ParseString1(_AWSErrMsg.EnvironmentMachineName)}},
                {"EnvironmentOSVersion",new AttributeValue{S=ParseString1(_AWSErrMsg.EnvironmentOSVersion)}},
                {"EnvironmentServicePack",new AttributeValue{S=ParseString1(_AWSErrMsg.EnvironmentServicePack)}},
                {"EnvironmentPlatform",new AttributeValue{S=ParseString1(_AWSErrMsg.EnvironmentPlatform)}},
//                {"ExceptionContent",new AttributeValue{S=ParseString1(_AWSErrMsg.ExceptionContent)}},
//                {"ImageString",new AttributeValue{B=_AWSErrMsg.ImageString}},
                {"AuthServer",new AttributeValue{S=ParseString1(_AWSErrMsg.AuthServer)}},
                {"AuthLoginAccount",new AttributeValue{S=ParseString1(_AWSErrMsg.AuthLoginAccount)}},
                {"ComputerTime",new AttributeValue{S=ParseDateTime1(_AWSErrMsg.ComputerTime)}},
                {"StackTraceMethods",new AttributeValue{S=ParseString1(_AWSErrMsg.StackTraceMethods)}},
                {"StackTraceAssemblys",new AttributeValue{S=ParseString1(_AWSErrMsg.StackTraceAssemblys)}},
                {"DeploySources",new AttributeValue{S=ParseString1(_AWSErrMsg.DeploySources)}},
                {"OperatorMessage",new AttributeValue{S=ParseString1(_AWSErrMsg.OperatorMessage)}},
                {"ExceptionContentHead",new AttributeValue{S=ParseString1(_AWSErrMsg.ExceptionContentHead)}},
                {"InsertDateTime",new AttributeValue{S=System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:FFF")}}
            }
            };
            // 上傳到 AWS
            Rsp = client.PutItem(Req);

            var ReqIdx = new PutItemRequest
            {
                TableName = "ischoolErrorReportIdx",
                Item = new Dictionary<string, AttributeValue>() {
                {"GUID",new AttributeValue{S=_AWSErrMsg.GUID}},
                {"AuthServer",new AttributeValue{S=ParseString1(_AWSErrMsg.AuthServer)}},                
                {"ComputerTime",new AttributeValue{S=ParseDateTime1( _AWSErrMsg.ComputerTime)}},
                {"ExceptionContentHead",new AttributeValue{S=ParseString1(_AWSErrMsg.ExceptionContentHead)}},
                {"DateIdx",new AttributeValue{S=System.DateTime.Now.ToString("yyyy-MM-dd")}}
            }
            };
            // 上傳到 AWS ischoolErrorReportIdx
            RspIdx = client.PutItem(ReqIdx);


            // 上傳到 aws ischoolErrorReportDateIdx 主要建學校主機與日期索引，加速查詢用
            var ReqDateIdx = new PutItemRequest
            {
                TableName = "ischoolErrorReportDateIdx",
                Item = new Dictionary<string, AttributeValue>() {
                {"AuthServer",new AttributeValue{S=ParseString1(_AWSErrMsg.AuthServer)}},
                {"ComputerTime",new AttributeValue{S=ParseDateTime1( _AWSErrMsg.ComputerTime)}},
                {"GUID",new AttributeValue{S=_AWSErrMsg.GUID}}}
            };
            RspDateIdx = client.PutItem(ReqDateIdx);
        }
        #endregion        
        //[Field]
        //public string EnvironmentUserName { get; set; }
        //[Field]
        //public string EnvironmentMachineName { get; set; }
        //[Field]
        //public string EnvironmentOSVersion { get; set; }
        //[Field]
        //public string EnvironmentServicePack { get; set; }
        //[Field]
        //public string EnvironmentPlatform { get; set; }
        //[Field]
        //public string ExceptionContent { get; set; }
        //[Field]
        //private string ImageString { get; set; }
        //[Field]
        //public string AuthServer { get; set; }
        //[Field]
        //public string AuthLoginAccount { get; set; }
        //[Field]
        //public DateTime ComputerTime { get; set; }
        //[Field]
        //public string StackTraceMethods { get; set; }
        //[Field]
        //public string StackTraceAssemblys { get; set; }
        //[Field]
        //public string DeploySources { get; set; }
        //[Field]
        //public string OperatorMessage { get; set; }

        /// <summary>
        /// 檢查字串內是否是空值或空串，如果是轉成Null，主要讓AWS可以儲存
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string ParseString1(string str)
        {
            if (string.IsNullOrEmpty(str))
                return "-";
            else
                return str;
        }

        /// <summary>
        /// 轉換日期格式為 24小時制
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public string ParseDateTime1(string str)
        {
            string retVal = "-";
            if (!string.IsNullOrEmpty(str))
            {
                DateTime dt;
                if (DateTime.TryParse(str, out dt))
                {
                    retVal=dt.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else 
                    return retVal;
            }
            return retVal;
        }

        public System.Drawing.Image Image
        {
            get
            {
                try
                {
                    //byte[] bs = Convert.FromBase64String(_AWSErrMsg.ImageString);                    
                    MemoryStream ms = _AWSErrMsg.ImageStream;
                    Bitmap bm = new Bitmap(ms);
                    return bm;
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                try
                {
                    MemoryStream ms = new MemoryStream();
                    value.Save(ms, ImageFormat.Png);

                    byte[] bytes = new byte[ms.Length];
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Read(bytes, 0, (int)ms.Length);
//                    string s = Convert.ToBase64String(bytes);                    
                    //ms.Close();
                    //_AWSErrMsg.ImageString = s;
                    _AWSErrMsg.ImageStream = ms;
                }
                catch
                {
                    _AWSErrMsg.ImageStream = null;
                }
            }
        }

        public void SetContent(Exception ex)
        {
            _AWSErrMsg.ExceptionContentHead = ex.Message;
            _AWSErrMsg.ExceptionContent = Transform(ex);
            _AWSErrMsg.StackTraceMethods = "";
            _AWSErrMsg.StackTraceAssemblys = "";
            FillInStackTrace(ex);
        }

        public void GetApplicationSnapShot()
        {
            try
            {
                int xOffset = int.MaxValue, yOffset = int.MaxValue, xOffset2 = int.MinValue, yOffset2 = int.MinValue;
                foreach (Form form in Application.OpenForms)
                {
                    try
                    {
                        if (form.DesktopBounds.Left < xOffset)
                            xOffset = form.DesktopBounds.Left;
                        if (form.DesktopBounds.Right > xOffset2)
                            xOffset2 = form.DesktopBounds.Right;
                        if (form.DesktopBounds.Top < yOffset)
                            yOffset = form.DesktopBounds.Top;
                        if (form.DesktopBounds.Bottom > yOffset2)
                            yOffset2 = form.DesktopBounds.Bottom;
                    }
                    catch (Exception exform1)
                    { 
                    
                    }
                }

                Bitmap BigImg = new Bitmap(xOffset2 - xOffset, yOffset2 - yOffset);
                foreach (Form form in Application.OpenForms)
                {
                    try
                    {
                        Size size = new Size(form.DesktopBounds.Right - form.DesktopBounds.Left, form.DesktopBounds.Bottom - form.DesktopBounds.Top);
                        form.DrawToBitmap(BigImg, new Rectangle(new Point(form.DesktopBounds.Left - xOffset, form.DesktopBounds.Top - yOffset), size));
                    }
                    catch (Exception exform2)
                    { 
                    
                    }
                }
                this.Image = BigImg;
                
            }
            catch
            {
                this.Image = null;
            }
        }

        public void GetDeploySources()
        {
            try
            {
                _AWSErrMsg.DeploySources = "";
                FillInDeploySource(Application.StartupPath);
            }
            catch { }
        }
        private void FillInDeploySource(string path)
        {
            try
            {
                if (File.Exists(Path.Combine(path, "deploy.source")))
                {
                    //# 號前面的是 Url，後面的是其他參數。
                    //2010/2/6 黃耀明。
                    string rawSource = (_AWSErrMsg.DeploySources == "" ? "" : "\n") + new StreamReader(Path.Combine(path, "deploy.source")).ReadLine();
                    _AWSErrMsg.DeploySources += rawSource.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries)[0];
                }
            }
            catch { }
            foreach (var item in Directory.GetDirectories(path))
            {
                FillInDeploySource(item);
            }
        }

        private string Transform(Exception ex)
        {
            Stream s;
            try
            {
                string fileName = DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Month + "_" + DateTime.Now.Second + ".xml";
                DirectoryInfo dic = new DirectoryInfo(Application.StartupPath + "Exception");
                if (!dic.Exists)
                {
                    (new DirectoryInfo(Application.StartupPath)).CreateSubdirectory("Exception");
                }
                s = new FileStream(Application.StartupPath + "\\Exception\\" + fileName, FileMode.CreateNew);
            }
            catch
            {
                s = new MemoryStream();
            }
            XmlTextWriter writer = new XmlTextWriter(s, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;

            //writer.WriteStartElement(ex.GetType().Name);
            writer.WriteStartElement("Exception");
            {
                Transform(writer, new System.Collections.Stack(new object[] { ex }), ex);
            }
            writer.WriteEndElement();

            writer.Flush();
            s.Flush();
            writer.BaseStream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(writer.BaseStream, Encoding.UTF8);


            string result = reader.ReadToEnd();
            reader.Close();
            s.Close();
            return result;
        }

        private void Transform(XmlWriter writer, System.Collections.Stack stack, object ex)
        {
            Type ext = ex.GetType();

            try
            {
                writer.WriteAttributeString("Type", ext.Name);
                if (ex.ToString() != ext.FullName && !(ex is Exception))
                {
                    writer.WriteAttributeString("ToString", ex.ToString());
                }
            }
            catch { }
            foreach (PropertyInfo each in ext.GetProperties())
            {
                if (!each.CanRead || ((ex is Exception) && each.Name == "TargetSite"))
                    continue;
                object obj = null;
                try
                {
                    obj = each.GetValue(ex, null);
                }
                catch
                {
                    continue;
                }
                writer.WriteStartElement(each.Name);
                //writer.WriteAttributeString("PropertyType", each.PropertyType.Name);
                try
                {
                    if (obj != null)
                    {
                        if (obj.GetType().IsSubclassOf(typeof(ValueType)) || obj.GetType() == typeof(string))
                        {
                            writer.WriteAttributeString("Type", obj.GetType().Name);
                            writer.WriteString(obj.ToString());
                        }
                        else
                        {
                            if (stack.Contains(obj))
                            {
                                writer.WriteComment("循環參考。");
                            }
                            else
                            {
                                stack.Push(obj);
                                Transform(writer, stack, obj);
                                stack.Pop();
                            }
                        }
                    }
                    else
                        writer.WriteComment("此屬性值為「Null」。");
                }
                catch (Exception e)
                {
                    writer.WriteComment(string.Format("讀取屬性值錯誤，訊息：\n{0}", e.Message));
                }
                writer.WriteEndElement();
            }
            if (ex is IEnumerable && !(ex is string))
            {
                writer.WriteStartElement("EnumerableItems");
                var enumerable = ex as IEnumerable;
                foreach (var item in enumerable)
                {
                    writer.WriteStartElement("Item");
                    //writer.WriteAttributeString("Type", item.GetType().Name);
                    stack.Push(item);
                    Transform(writer, stack, item);
                    stack.Pop();
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }
    }
}
