using System;
using System.Collections.Generic;

using System.Text;
using SmartSchool.Customization.PlugIn;
using FISCA.Presentation;
using SmartSchool.AccessControl;

namespace SmartSchool.Adaatper
{
    public class ButtonAdapterPlugInToMenuButton : IManager<ButtonAdapter>
    {
        MenuButton _Target;
        public event EventHandler Changed;
        public ButtonAdapterPlugInToMenuButton(MenuButton target)
        {
            _Target = target;
        }
        private void button_OnSetBarMessage(object sender, SmartSchool.Customization.PlugIn.ButtonAdapter.SetBarMessageEventArgs e)
        {
            if ( e.HasProgress )
                MotherForm.SetStatusBarMessage(e.Message, e.Progress);
            else
                MotherForm.SetStatusBarMessage(e.Message);
        }
        #region IManager<ButtonAdapter> 成員
        Dictionary<MenuButton, ButtonAdapter> items = new Dictionary<MenuButton, ButtonAdapter>();
        public void Add(ButtonAdapter button)
        {
            List<string> paths = new List<string>();
            if ( button.Path != "" )
                paths.AddRange(button.Path.Split('/'));
            paths.Add(button.Text);
            MenuButton adp = _Target.Items[paths.ToArray()];
            button.OnSetBarMessage += new EventHandler<SmartSchool.Customization.PlugIn.ButtonAdapter.SetBarMessageEventArgs>(button_OnSetBarMessage);
            if ( button is IFeature )
                adp.Enable = CurrentUser.Acl[button as IFeature].Executable;
            items.Add(adp, button);
            adp.Click += new EventHandler(adp_Click);
            adp.Image = button.Image;

            if ( Changed != null )
                Changed(this, new EventArgs());
        }

        void adp_Click(object sender, EventArgs e)
        {
            items[(MenuButton)sender].Click();
        }

        public void Remove(ButtonAdapter instance)
        {
            throw new Exception("此方法已不再支援");
        }

        #endregion
    }
}
