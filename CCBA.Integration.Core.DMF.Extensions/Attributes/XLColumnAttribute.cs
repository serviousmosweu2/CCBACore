using System;

namespace CCBA.Integration.Core.DMF.Extensions.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class XLColumnAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
