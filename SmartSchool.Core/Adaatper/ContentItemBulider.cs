using System;
using System.Collections.Generic;

using System.Text;
using SmartSchool.Customization.PlugIn.ExtendedContent;
using System.Windows.Forms;
using SmartSchool.AccessControl;
using System.Drawing;
using FISCA.Presentation;

namespace SmartSchool.Adaatper
{
    public class ContentItemBulider<T> : IDetailBulider where T : SmartSchool.Customization.PlugIn.ExtendedContent.IContentItem, new()
    {
        #region IDetailBulider 成員

        public DetailContent GetContent()
        {
            var content = new DetailContent();
            var Tcontent = new T();
            content.Padding = new Padding(0);
            content.PrimaryKeyChanged += delegate { Tcontent.LoadContent(content.PrimaryKey); };
            Tcontent.SaveButtonVisibleChanged += delegate { content.SaveButtonVisible = Tcontent.SaveButtonVisible; };
            Tcontent.CancelButtonVisibleChanged += delegate { content.CancelButtonVisible = Tcontent.CancelButtonVisible; };
            content.SaveButtonClick += delegate { Tcontent.Save(); };
            content.CancelButtonClick += delegate { Tcontent.Undo(); };
            content.Group = Tcontent.Title;
            content.Size = Tcontent.DisplayControl.Size;
            content.Controls.Add(Tcontent.DisplayControl);
            Tcontent.DisplayControl.BackColor = Color.Transparent;
            Tcontent.DisplayControl.Dock = DockStyle.Fill;

            if ( Attribute.IsDefined(Tcontent.GetType(), typeof(FeatureCodeAttribute)) )
            {
                FeatureCodeAttribute fca = Attribute.GetCustomAttribute(Tcontent.GetType(), typeof(FeatureCodeAttribute)) as FeatureCodeAttribute;
                if ( fca != null )
                {
                    if ( FISCA.Permission.FeatureAce.Parse(fca.FeatureCode) != FISCA.Permission.AccessOptions.View )
                        return null;
                }
            }

            return content;
        }

        #endregion
    }
    public class ContentItemBulider : IDetailBulider
    {
        private SmartSchool.Customization.PlugIn.ExtendedContent.IContentItem Seed { get; set; }
        public ContentItemBulider(SmartSchool.Customization.PlugIn.ExtendedContent.IContentItem seed)
        {
            Seed = seed;
        }
        #region IDetailBulider 成員

        public DetailContent GetContent()
        {
            var content = new DetailContent();
            var Tcontent = (SmartSchool.Customization.PlugIn.ExtendedContent.IContentItem)Seed.Clone();
            content.Padding = new Padding(0);
            content.PrimaryKeyChanged += delegate { Tcontent.LoadContent(content.PrimaryKey); };
            Tcontent.SaveButtonVisibleChanged += delegate { content.SaveButtonVisible = Tcontent.SaveButtonVisible; };
            Tcontent.CancelButtonVisibleChanged += delegate { content.CancelButtonVisible = Tcontent.CancelButtonVisible; };
            content.SaveButtonClick += delegate { Tcontent.Save(); };
            content.CancelButtonClick += delegate { Tcontent.Undo(); };
            content.Group = Tcontent.Title;
            content.Size = Tcontent.DisplayControl.Size;
            content.Controls.Add(Tcontent.DisplayControl);
            Tcontent.DisplayControl.BackColor = Color.Transparent;
            Tcontent.DisplayControl.Dock = DockStyle.Fill;
            return content;
        }

        #endregion
    }
}
