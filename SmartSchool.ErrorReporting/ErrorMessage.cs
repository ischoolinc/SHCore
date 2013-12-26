using System;
using System.Collections.Generic;
using System.Text;
using FISCA.UDT;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Windows.Forms;
using System.Diagnostics;

namespace SmartSchool.ErrorReporting
{
    public class ErrorMessgae : FISCA.UDT.ActiveRecord
    {
        public ErrorMessgae()
        {
            EnvironmentUserName = Environment.UserName;
            EnvironmentMachineName = Environment.MachineName;
            EnvironmentOSVersion = Environment.OSVersion.VersionString;
            EnvironmentServicePack = Environment.OSVersion.ServicePack;
            EnvironmentPlatform = Environment.OSVersion.Platform.ToString();
            ImageString = "";
            ExceptionContent = "";
            AuthServer = FISCA.Authentication.DSAServices.IsLogined ? FISCA.Authentication.DSAServices.AccessPoint : "NotLogin";
            AuthLoginAccount = FISCA.Authentication.DSAServices.IsLogined ? FISCA.Authentication.DSAServices.UserAccount : "NotLogin";
            ComputerTime = DateTime.Now;
            StackTraceMethods = "";
            StackTraceAssemblys = "";
            DeploySources = "";
        }
        public ErrorMessgae(Exception ex)
            : this()
        {
            SetContent(ex);
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
                    StackTraceMethods += (StackTraceMethods == "" ? "" : "\n") + frame.GetMethod().ToString() + " in " + frame.GetMethod().DeclaringType.FullName;
                    StackTraceAssemblys += (StackTraceMethods == "" ? "" : "\n") + frame.GetMethod().Module.Assembly.FullName;
                }
            }
        }
        [Field]
        public string EnvironmentUserName { get; set; }
        [Field]
        public string EnvironmentMachineName { get; set; }
        [Field]
        public string EnvironmentOSVersion { get; set; }
        [Field]
        public string EnvironmentServicePack { get; set; }
        [Field]
        public string EnvironmentPlatform { get; set; }
        [Field]
        public string ExceptionContent { get; set; }
        [Field]
        private string ImageString { get; set; }
        [Field]
        public string AuthServer { get; set; }
        [Field]
        public string AuthLoginAccount { get; set; }
        [Field]
        public DateTime ComputerTime { get; set; }
        [Field]
        public string StackTraceMethods { get; set; }
        [Field]
        public string StackTraceAssemblys { get; set; }
        [Field]
        public string DeploySources { get; set; }
        [Field]
        public string OperatorMessage { get; set; }

        public System.Drawing.Image Image
        {
            get
            {
                try
                {
                    byte[] bs = Convert.FromBase64String(ImageString);
                    MemoryStream ms = new MemoryStream(bs);
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
                    string s = Convert.ToBase64String(bytes);
                    ms.Close();
                    ImageString = s;
                }
                catch
                {
                    ImageString = "";
                }
            }
        }

        public void SetContent(Exception ex)
        {
            ExceptionContent = Transform(ex);
            StackTraceMethods = "";
            StackTraceAssemblys = "";
            FillInStackTrace(ex);
        }

        public void GetApplicationSnapShot()
        {
            try
            {
                int xOffset = int.MaxValue, yOffset = int.MaxValue, xOffset2 = int.MinValue, yOffset2 = int.MinValue;
                foreach (Form form in Application.OpenForms)
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
                Bitmap BigImg = new Bitmap(xOffset2 - xOffset, yOffset2 - yOffset);
                foreach (Form form in Application.OpenForms)
                {
                    Size size = new Size(form.DesktopBounds.Right - form.DesktopBounds.Left, form.DesktopBounds.Bottom - form.DesktopBounds.Top);
                    form.DrawToBitmap(BigImg, new Rectangle(new Point(form.DesktopBounds.Left - xOffset, form.DesktopBounds.Top - yOffset), size));
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
                DeploySources = "";
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
                    string rawSource = (DeploySources == "" ? "" : "\n") + new StreamReader(Path.Combine(path, "deploy.source")).ReadLine();
                    DeploySources += rawSource.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries)[0];
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
