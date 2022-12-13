namespace CCBA.Integrations.Base.Models
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public class JobData
    {
        public JobData(string entityName, string entityId)
        {
            EntityName = entityName;
            EntityId = entityId;
        }

        public string EntityId { get; }
        public string EntityName { get; }
    }
}