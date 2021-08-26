using VirtoCommerce.DescriptionExportImportModule.Core.Models;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Services
{
    public interface IExportPagedDataSourceFactory
    {
        IExportPagedDataSource Create(int pageSize, ExportDataRequest request);
    }
}
