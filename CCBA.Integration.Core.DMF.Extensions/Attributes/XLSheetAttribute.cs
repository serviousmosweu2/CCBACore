namespace CCBA.Integration.Core.DMF.Extensions.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
    public class XLSheetAttribute : System.Attribute
    {
        public string Name { get; set; }
        public string File { get; set; }
    }
}
