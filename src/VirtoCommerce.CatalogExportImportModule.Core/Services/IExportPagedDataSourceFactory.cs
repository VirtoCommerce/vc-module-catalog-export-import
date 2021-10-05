using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IExportPagedDataSourceFactory
    {
        IExportPagedDataSource Create(int pageSize, ExportDataRequest request);
    }
}
