using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VirtoCommerce.CatalogExportImportModule.Data.Repositories
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<VirtoCommerceCatalogExportImportDbContext>
    {
        public VirtoCommerceCatalogExportImportDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<VirtoCommerceCatalogExportImportDbContext>();

            builder.UseSqlServer("Data Source=(local);Initial Catalog=VirtoCommerce3;Persist Security Info=True;User ID=virto;Password=virto;MultipleActiveResultSets=True;Connect Timeout=30");

            return new VirtoCommerceCatalogExportImportDbContext(builder.Options);
        }
    }
}
