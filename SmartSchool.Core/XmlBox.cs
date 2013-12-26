using System.Windows.Forms;
using System.Xml;
using FISCA.DSAUtil;

namespace SmartSchool
{
    public partial class XmlBox : FISCA.Presentation.Controls.BaseForm
    {
        public XmlBox()
        {
            InitializeComponent();
        }

        public static void ShowXml(XmlElement xml, IWin32Window owner)
        {
            XmlBox box = new XmlBox();
            box.richTextBox1.Text = DSXmlHelper.Format(xml.OuterXml);
            box.ShowDialog(owner);
        }

        public static void ShowXml(XmlElement xml)
        {
            ShowXml(xml, null);
        }

    }
}