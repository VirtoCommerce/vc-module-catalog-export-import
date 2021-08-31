using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Models
{
    public sealed class ImportProgressInfo : ValueObject
    {
        public ImportProgressInfo()
        {
            Errors = new List<string>();
        }

        public string Description { get; set; }

        public int ProcessedCount { get; set; }

        public int TotalCount { get; set; }

        public int ErrorCount { get; set; }

        public int CreatedCount { get; set; }

        public int UpdatedCount { get; set; }

        public string ReportUrl { get; set; }

        public ICollection<string> Errors { get; set; }
    }
}
