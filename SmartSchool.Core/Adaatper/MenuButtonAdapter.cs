using System;
using System.Collections.Generic;

using System.Text;
using SmartSchool.Customization.PlugIn;
using FISCA.Presentation;
namespace SmartSchool.Adaatper
{
    public class MenuButtonAdapter : IManager<ButtonAdapter> 
    {
        public NLDPanel Presentation { get; private set; }
        public MenuButtonAdapter(NLDPanel presentation)
        {
            Presentation = presentation;
        }

        #region IManager<ButtonAdapter> 成員
        Dictionary<ButtonAdapter, MenuButton> _Mapping = new Dictionary<ButtonAdapter, MenuButton>();

        public void Add(ButtonAdapter instance)
        {
        }

        public void Remove(ButtonAdapter instance)
        {
            _Mapping[instance].Visible = false;
        }

        #endregion
    }
}
