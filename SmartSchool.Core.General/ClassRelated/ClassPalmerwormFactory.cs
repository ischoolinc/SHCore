//using System;
//using System.Collections.Generic;
//using System.Text;
//using SmartSchool.Common;
//using SmartSchool.ClassRelated.Palmerworm;
//using SmartSchool.AccessControl;
//using SmartSchool.Customization.PlugIn.ExtendedContent;
//using Framework;
//using SmartSchool.Customization.PlugIn;

//namespace SmartSchool.ClassRelated
//{
//    internal class ClassPalmerwormFactory : IManager<IContentItem>
//    {
//        private static ClassPalmerwormFactory _Instence;

//        public static ClassPalmerwormFactory Instence
//        {
//            get
//            {
//                if ( _Instence == null ) _Instence = new ClassPalmerwormFactory();
//                return _Instence;
//            }
//        }

//        private ClassPalmerwormFactory()
//        {
//        }

//        private List<IContentItem> plugInItems = new List<IContentItem>();

//        public Customization.PlugIn.ExtendedContent.IContentItem[] Load()
//        {
//            List<Customization.PlugIn.ExtendedContent.IContentItem> _items = new List<Customization.PlugIn.ExtendedContent.IContentItem>();

//            List<Type> _type_list = new List<Type>(new Type[]{
//                typeof(ClassBaseInfoItem),
//                typeof(ClassStudentItem),
//                typeof(ElectronicPaperPalmerworm)
//            });

//            foreach ( Type type in _type_list )
//            {
//                if ( !Attribute.IsDefined(type, typeof(FeatureCodeAttribute)) || CurrentUser.Acl[type].Viewable )
//                {
//                    try
//                    {
//                        IContentItem item = type.GetConstructor(Type.EmptyTypes).Invoke(null) as IContentItem;
//                        _items.Add(item);
//                    }
//                    catch ( Exception ex ) { Framework.BugReporter.ReportException(ex, false); }
//                }
//            }

//            foreach ( IContentItem var in plugInItems )
//            {
//                try
//                {
//                    if ( !Attribute.IsDefined(var.GetType(), typeof(FeatureCodeAttribute)) )
//                        _items.Add((IContentItem)var.Clone());
//                    else
//                        if ( CurrentUser.Acl[var.GetType()].Viewable )
//                            _items.Add((IContentItem)var.Clone());
//                }
//                catch ( Exception ex ) { BugReporter.ReportException(ex, false); }
//            }

//            return _items.ToArray();
//        }

//        #region IManager<IContentItem> жин√

//        public void Add(IContentItem instance)
//        {
//            plugInItems.Add(instance);
//            if ( ItemsChanged != null ) ItemsChanged(this, new EventArgs());
//        }

//        public void Remove(IContentItem instance)
//        {
//            plugInItems.Remove(instance);
//            if ( ItemsChanged != null ) ItemsChanged(this, new EventArgs());
//        }

//        public event EventHandler ItemsChanged;
//        #endregion
//    }
//}
