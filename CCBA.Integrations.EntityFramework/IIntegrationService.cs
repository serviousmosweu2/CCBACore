using CCBA.Integrations.LegalEntity.Model;
using System;
using System.Collections.Generic;

namespace CCBA.Integrations.LegalEntity
{
    public interface IIntegrationService : IDisposable
    {
        IEnumerable<VwIntegrationLegalentity> IntegrationLegalEntities(string integrationName);
    }
}