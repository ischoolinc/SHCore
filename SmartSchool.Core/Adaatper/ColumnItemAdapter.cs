using System;
using System.Collections.Generic;

using System.Text;
using SmartSchool.Customization.PlugIn.ExtendedColumn;
using FISCA.Presentation;

namespace SmartSchool.Adaatper
{
    public class ColumnItemAdapter:ListPaneField
    {
        public IColumnItem ColumnItem { get; private set; }
        public ColumnItemAdapter(IColumnItem columnItem):base(columnItem.ColumnHeader)
        {
            ColumnItem = columnItem;
            ColumnItem.VariableChanged += new EventHandler(ColumnItem_VariableChanged);
            this.PreloadVariable += new EventHandler<PreloadVariableEventArgs>(ColumnItemAdapter_PreloadVariable);
            this.GetVariable += new EventHandler<GetVariableEventArgs>(ColumnItemAdapter_GetVariable);
        }

        void ColumnItemAdapter_GetVariable(object sender, GetVariableEventArgs e)
        {
            e.Value = ColumnItem.ExtendedValues[e.Key];
        }

        void ColumnItemAdapter_PreloadVariable(object sender, PreloadVariableEventArgs e)
        {
            ColumnItem.FillExtendedValues(new List<string>(e.Keys));
        }

        void ColumnItem_VariableChanged(object sender, EventArgs e)
        {
            this.Reload();
        }
        
    }
}
