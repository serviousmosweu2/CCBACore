using System.Collections.Generic;
using System.Linq;

namespace CCBA.Integrations.LegalEntity.Model
{
    public class IntegrationLegalEntityRepository : GenericRepository
    {
        public IntegrationLegalEntityRepository(string connectionString) : base(connectionString)
        {
        }

        public IEnumerable<VwIntegrationLegalentity> IntegrationLegalEntities(string integrationName)
        {
            return DbContext.VwIntegrationLegalentities.Where(x => x.IntegrationName == integrationName).ToList();
        }
    }
}