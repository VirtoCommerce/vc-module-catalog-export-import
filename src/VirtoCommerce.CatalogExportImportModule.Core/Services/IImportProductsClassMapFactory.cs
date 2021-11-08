using System.Threading.Tasks;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IImportProductsClassMapFactory
    {
        Task<ClassMap<CsvPhysicalProduct>> CreateClassMapAsync(string catalogId);
    }
}
