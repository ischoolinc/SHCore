using System;

namespace SmartSchool
{
    public  interface IPalmerwormManager
    {
        bool EnableSave { get;set;}
        bool EnableCancel { get;set;}
        event EventHandler Save;
        event EventHandler Cacel;
        event EventHandler Reflash;
    }
}
