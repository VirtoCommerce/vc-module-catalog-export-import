using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IExportPagedDataSource
    {
        int CurrentPageNumber { get; }
        int PageSize { get; set; }
        ExportDataRequest Request { get; set; }
        string DataType { get; }

        Task<int> GetTotalCountAsync();

        Task<bool> FetchAsync();

        IExportable[] Items { get; }
    }
}
