using VirtoCommerce.DescriptionExportImportModule.Core.Models;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Services
{
    public interface IDescriptionExportPagedDataSourceFactory
    {
        IDescriptionExportPagedDataSource Create(int pageSize, ExportDataRequest request);
    }
}
