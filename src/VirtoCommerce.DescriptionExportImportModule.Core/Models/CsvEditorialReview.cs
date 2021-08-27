using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Models
{
    public sealed class CsvEditorialReview : IImportable, IExportable
    {
        [Optional]
        [Name("Product Name")]
        public string ProductName { get; set; }

        [Required]
        [Name("Product SKU")]
        public string ProductSku { get; set; }

        private string _id;

        [Optional]
        [Name("Description Id")]
        public string Id
        {
            get => _id;
            set => _id = value?.Trim();
        }

        [Required]
        [Name("Description Content")]
        public string Content { get; set; }

        [Required]
        [Name("Type")]
        public string ReviewType { get; set; }

        [Required]
        [Name("Language")]
        public string LanguageCode { get; set; }
    }
}
