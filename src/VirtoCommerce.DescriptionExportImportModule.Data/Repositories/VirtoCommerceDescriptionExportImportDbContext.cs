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
        }
    }
}

