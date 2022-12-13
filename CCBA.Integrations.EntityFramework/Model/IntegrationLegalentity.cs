namespace CCBA.Integrations.LegalEntity.Model
{
    public class IntegrationLegalentity
    {
        public int Id { get; set; }
        public virtual Integration Integration { get; set; }
        public int IntegrationId { get; set; }
        public bool IsActive { get; set; }
        public virtual LegalEntity LegalEntity { get; set; }
        public int LegalEntityId { get; set; }
    }
}