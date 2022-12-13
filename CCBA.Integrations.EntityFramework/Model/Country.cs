using System.Collections.Generic;

namespace CCBA.Integrations.LegalEntity.Model
{
    public class Country
    {
        public Country()
        {
            LegalEntities = new HashSet<LegalEntity>();
        }

        public int Id { get; set; }
        public virtual ICollection<LegalEntity> LegalEntities { get; set; }
        public string Name { get; set; }
    }
}