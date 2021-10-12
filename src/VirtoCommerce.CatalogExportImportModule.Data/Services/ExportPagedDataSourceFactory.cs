using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public class ExportPagedDataSourceFactory : IExportPagedDataSourceFactory
    {
        private readonly IEnumerable<Func<ExportDataRequest, int, IExportPagedDataSource>> _dataSourceFactories;
        public ExportPagedDataSourceFactory(IEnumerable<Func<ExportDataRequest, int, IExportPagedDataSource>> dataSourceFactories)
        {
            _dataSourceFactories = dataSourceFactories;
        }

        public IExportPagedDataSource Create(int pageSize, ExportDataRequest request)
        {
            var resultFactory = _dataSourceFactories.FirstOrDefault(x => x(request, pageSize).DataType.EqualsInvariant(request.DataType));

            if (resultFactory is null)
            {
                throw new ArgumentException(nameof(request.DataType));
            }

            var result = resultFactory(request, pageSize);

            return result;
        }
    }
}
