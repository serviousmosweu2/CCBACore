using CCBA.Integrations.LegalEntity.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CCBA.Integrations.LegalEntity
{
    [Obsolete]
    public class IntegrationService : IIntegrationService
    {
        private readonly ConfigurationDatabase db;

        public IntegrationService(string connectionString)
        {
            db = new ConfigurationDatabase(connectionString);
        }

        public void Dispose()
        {
            db.Dispose();
        }

        public IEnumerable<VwIntegrationLegalentity> IntegrationLegalEntities(string integrationName)
        {
            return db.VwIntegrationLegalentities.Where(x => x.IntegrationName == integrationName).ToList();
        }
    }
}