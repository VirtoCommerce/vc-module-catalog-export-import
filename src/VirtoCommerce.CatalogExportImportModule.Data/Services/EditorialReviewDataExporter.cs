using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public class EditorialReviewDataExporter : DataExporter<CsvEditorialReview>
    {
        public EditorialReviewDataExporter(IExportPagedDataSourceFactory exportPagedDataSourceFactory, IExportWriterFactory exportWriterFactory, IOptions<PlatformOptions> platformOptions, IBlobStorageProvider blobStorageProvider, IBlobUrlResolver blobUrlResolver) : base(exportPagedDataSourceFactory, exportWriterFactory, platformOptions, blobStorageProvider, blobUrlResolver)
        {
        }

        public override string DataType => ModuleConstants.DataTypes.EditorialReview;
    }
}
