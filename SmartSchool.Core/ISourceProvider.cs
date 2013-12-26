using System;
using System.Collections.Generic;

namespace SmartSchool
{
    public interface ISourceProvider<T, S>
    {
        List<T> Source { get;set;}
        event EventHandler SourceChanged;
        bool DisplaySource { get;}
        bool ImmediatelySearch { get;}
        S SearchProvider { get;}
        string SearchWatermark { get;}
    }
}
