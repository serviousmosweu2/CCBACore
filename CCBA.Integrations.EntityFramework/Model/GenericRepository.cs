using Microsoft.EntityFrameworkCore;

namespace CCBA.Integrations.LegalEntity.Model
{
    public abstract class GenericRepository
    {
        protected readonly ConfigurationDatabase DbContext;

        protected GenericRepository(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ConfigurationDatabase>();
            optionsBuilder.UseSqlServer(connectionString);
            DbContext = new ConfigurationDatabase(optionsBuilder.Options);
        }
    }
}