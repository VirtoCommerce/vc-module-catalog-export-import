using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.PushNotifications;

namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public class ExportPushNotification : PushNotification
    {
        public ExportPushNotification(string creator)
            : base(creator)
        {
            Errors = new List<string>();
        }

        public string JobId { get; set; }
        public int ProcessedCount { get; set; }
        public int TotalCount { get; set; }
        public ICollection<string> Errors { get; set; }
        public DateTime? Finished { get; set; }
        public string FileUrl { get; set; }
    }
}
