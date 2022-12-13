namespace CCBA.Integrations.DMF.Shared.Models
{
    public abstract class Configuration
    {
        public string definitionGroupId { get; set; }
        public string executionId { get; set; }
        public string legalEntityId { get; set; }
    }
}