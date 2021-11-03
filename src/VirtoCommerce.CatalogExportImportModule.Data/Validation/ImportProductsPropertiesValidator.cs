using System.Collections.Generic;
using FluentValidation;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogExportImportModule.Data.Validation
{
    public class ImportProductsPropertiesValidator : AbstractValidator<ICollection<Property>>
    {
        private readonly ImportRecord<CsvPhysicalProduct> _importRecord;

        public ImportProductsPropertiesValidator(ImportRecord<CsvPhysicalProduct> record)
        {
            _importRecord = record;
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleForEach(x => x).SetValidator(new ImportProductsPropertyValidator(_importRecord));
        }
    }
}
