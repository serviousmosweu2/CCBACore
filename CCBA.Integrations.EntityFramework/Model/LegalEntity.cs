using System.Collections.Generic;

namespace CCBA.Integrations.LegalEntity.Model
{
    public class LegalEntity
    {
        public LegalEntity()
        {
            IntegrationLegalentities = new HashSet<IntegrationLegalentity>();
        }

        public virtual Country Country { get; set; }
        public int CountryId { get; set; }
        public int Id { get; set; }
        public virtual ICollection<IntegrationLegalentity> IntegrationLegalentities { get; set; }
        public string LegalEntity1 { get; set; }
    }
}