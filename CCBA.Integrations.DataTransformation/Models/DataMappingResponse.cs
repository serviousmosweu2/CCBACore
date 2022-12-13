using CCBA.Integrations.DataTransformation.Common;

namespace CCBA.Integrations.DataTransformation.Models
{
    public class DataMappingResponse
    {
        public string DefaultValue { get; set; }
        public string FieldName { get; set; }
        public string Integration { get; set; }
        public string SourceSystem { get; set; }
        public string SourceValue { get; set; }
        public TransformationStatus Status { get; set; }
        public string TargetSystem { get; set; }
        public string TargetValue { get; set; }
    }
}