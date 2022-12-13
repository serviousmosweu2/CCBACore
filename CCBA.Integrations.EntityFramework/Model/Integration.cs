using System.Collections.Generic;

namespace CCBA.Integrations.LegalEntity.Model
{
    public class Integration
    {
        public Integration()
        {
            IntegrationLegalentities = new HashSet<IntegrationLegalentity>();
        }

        public int Id { get; set; }
        public virtual ICollection<IntegrationLegalentity> IntegrationLegalentities { get; set; }
        public string IntegrationName { get; set; }
    }
}