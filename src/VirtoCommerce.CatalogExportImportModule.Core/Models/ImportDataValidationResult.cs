using System;
namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public sealed class ImportDataValidationResult
    {
        public ImportDataValidationResult()
        {
            Errors = Array.Empty<ImportDataValidationError>();
        }

        public ImportDataValidationError[] Errors { get; set; }
    }
}
