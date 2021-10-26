using System;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IImportPagedDataSource<T> : IDisposable where T : IImportable
    {
        int CurrentPageNumber { get; }

        int PageSize { get; }

        string GetHeaderRaw();

        int GetTotalCount();

        Task<bool> FetchAsync();

        ImportRecord<T>[] Items { get; }

        void RegisterClassMap(ClassMap<T> classMap);
    }
}
