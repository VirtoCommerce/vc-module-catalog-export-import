using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;

namespace VirtoCommerce.CatalogExportImportModule.Data.Repositories
{
    public class VirtoCommerceCatalogExportImportDbContext : DbContextWithTriggers
    {
        public VirtoCommerceCatalogExportImportDbContext(DbContextOptions<VirtoCommerceCatalogExportImportDbContext> options)
          : base(options)
        {
        }

        protected VirtoCommerceCatalogExportImportDbContext(DbContextOptions options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}

