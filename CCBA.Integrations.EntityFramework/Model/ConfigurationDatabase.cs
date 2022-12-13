using Microsoft.EntityFrameworkCore;
using System;

namespace CCBA.Integrations.LegalEntity.Model
{
    public class ConfigurationDatabase : DbContext
    {
        private readonly string connectionstring;

        [Obsolete]
        public ConfigurationDatabase()
        {
        }

        [Obsolete]
        public ConfigurationDatabase(string connectionstring)
        {
            Database.GetDbConnection().ConnectionString = connectionstring;
        }

        public ConfigurationDatabase(DbContextOptions<ConfigurationDatabase> options) : base(options)
        {
        }

        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<IntegrationLegalentity> IntegrationLegalentities { get; set; }
        public virtual DbSet<Integration> Integrations { get; set; }
        public virtual DbSet<LegalEntity> LegalEntities { get; set; }
        public virtual DbSet<System> Systems { get; set; }
        public virtual DbSet<VwIntegrationLegalentity> VwIntegrationLegalentities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(connectionstring);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("Country_pk")
                    .IsClustered(false);

                entity.ToTable("COUNTRY");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("NAME");
            });

            modelBuilder.Entity<Integration>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("INTEGRATION_pk")
                    .IsClustered(false);

                entity.ToTable("INTEGRATION");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.IntegrationName)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("INTEGRATION_NAME");
            });

            modelBuilder.Entity<IntegrationLegalentity>(entity =>
            {
                entity.ToTable("INTEGRATION_LEGALENTITIES");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.IntegrationId).HasColumnName("INTEGRATION_ID");

                entity.Property(e => e.IsActive).HasColumnName("IS_ACTIVE");

                entity.Property(e => e.LegalEntityId).HasColumnName("LEGAL_ENTITY_ID");

                entity.HasOne(d => d.Integration)
                    .WithMany(p => p.IntegrationLegalentities)
                    .HasForeignKey(d => d.IntegrationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_INTEGRATION_LEGALENTITIES_FUNCTION");

                entity.HasOne(d => d.LegalEntity)
                    .WithMany(p => p.IntegrationLegalentities)
                    .HasForeignKey(d => d.LegalEntityId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_INTEGRATION_LEGALENTITIES_LEGAL_ENTITY");
            });

            modelBuilder.Entity<LegalEntity>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("LegalEntity_pk")
                    .IsClustered(false);

                entity.ToTable("LEGAL_ENTITY");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CountryId).HasColumnName("COUNTRY_ID");

                entity.Property(e => e.LegalEntity1)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("LEGAL_ENTITY");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.LegalEntities)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LEGAL_ENTITY_COUNTRY");
            });

            modelBuilder.Entity<System>(entity =>
            {
                entity.ToTable("SYSTEM");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.SystemName)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("SYSTEM_NAME");
            });

            modelBuilder.Entity<VwIntegrationLegalentity>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("VW_INTEGRATION_LEGALENTITIES");

                entity.Property(e => e.CountryId).HasColumnName("COUNTRY_ID");

                entity.Property(e => e.CountryName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("COUNTRY_NAME");

                entity.Property(e => e.IntegrationId).HasColumnName("INTEGRATION_ID");

                entity.Property(e => e.IntegrationName)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("INTEGRATION_NAME");

                entity.Property(e => e.LegalEntity)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("LEGAL_ENTITY");

                entity.Property(e => e.LegalEntityId).HasColumnName("LEGAL_ENTITY_ID");
            });
        }
    }
}