using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Repositories
{
    public class VirtoCommerceDescriptionExportImportDbContext : DbContextWithTriggers
    {
        public VirtoCommerceDescriptionExportImportDbContext(DbContextOptions<VirtoCommerceDescriptionExportImportDbContext> options)
          : base(options)
        {
        }

        protected VirtoCommerceDescriptionExportImportDbContext(DbContextOptions options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.Entity<MyModuleEntity>().ToTable("MyModule").HasKey(x => x.Id);
            // modelBuilder.Entity<MyModuleEntity>().Property(x => x.Id).HasMaxLength(128);
            // base.OnModelCreating(modelBuilder);
        }
    }
}

