using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IExportDataRequestPreprocessor
    {
        Task PreprocessRequestAsync(ExportDataRequest dataRequest, bool extendCategoriesWithChildren = false);
    }
}
