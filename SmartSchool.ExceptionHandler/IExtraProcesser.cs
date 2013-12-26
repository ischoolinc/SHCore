using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.ExceptionHandler
{
    public interface IExtraProcesser
    {
        ExtraInformation[] Process(object instance);
    }
}
