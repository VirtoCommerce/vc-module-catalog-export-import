using System.Collections.Generic;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Models
{
    public sealed class ImportErrorsContext
    {
        public IList<int> ErrorsRows { get; set; } = new List<int>();
    }
}
