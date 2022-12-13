using Microsoft.Azure.Cosmos.Table;

namespace CCBA.Integration.Core.DMF.Extensions.Models
{
    public class DMFConfigurationTableModel : TableEntity
    {
        public DMFConfigurationTableModel(string name, string integration)
        {
            PartitionKey = name;
            RowKey = integration;
        }

        public string definitionGroupId { get; set; }
        public string packageName { get; set; }
        public string executionId { get; set; }
        public bool reExecute { get; set; }
        public string legalEntityId { get; set; }
        public bool execute { get; set; }
        public bool overwrite { get; set; }
        public string packageUrl { get; set; }
        public int operationType { get; set; }
    }
}