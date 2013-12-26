using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.API.PlugIn.View;
using System.Windows.Forms;
using FISCA.Presentation;

namespace SmartSchool.Adaatper
{
    public class NavigationPlannerAdatper : INavView
    {
        public NavigationPlannerAdatper(NavigationPlanner planner)
        {
            Planner = planner;
            Planner.SelectedSource.ItemsChanged += new EventHandler(SelectedSource_ItemsChanged);
            Source = new PrimaryKeysCollection();
            Source.ItemsChanged += delegate
            {
                Layout(new List<string>(Source));
            };
        }

        void SelectedSource_ItemsChanged(object sender, EventArgs e)
        {
            if ( ListPaneSourceChanged != null )
                ListPaneSourceChanged(this, new ListPaneSourceChangedEventArgs(Planner.SelectedSource)
                {
                    SelectedAll = ( ( Control.ModifierKeys & Keys.Control ) == Keys.Control ),
                    AddToTemp = ( ( Control.ModifierKeys & Keys.Shift ) == Keys.Shift )
                }
                );
        }

        public NavigationPlanner Planner { get; private set; }

        #region NavView 成員

        public bool Active
        {
            get;
            set;
        }

        public string NavText
        {
            get { return Planner.Text; }
        }

        public string Description
        {
            get { return Planner.Description; }
        }

        public System.Windows.Forms.Control DisplayPane
        {
            get { return Planner.DisplayControl; }
        }

        public void Layout(List<string> PrimaryKeys)
        {
            Planner.Perform(PrimaryKeys);
        }

        public event EventHandler<ListPaneSourceChangedEventArgs> ListPaneSourceChanged;

        #endregion

        #region INavView 成員

        public PrimaryKeysCollection Source
        {
            get;
            private set;
        }

        #endregion
    }
}
