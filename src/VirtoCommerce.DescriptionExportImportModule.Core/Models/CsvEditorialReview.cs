using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Models
{
    public sealed class CsvEditorialReview : IImportable, IExportable
    {
        [Optional]
        [Name("Product Name")]
        public string ProductName { get; set; }

        [Optional]
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

        public void PatchModel(EditorialReview target)
        {
            target.Content = Content;
            target.ReviewType = ReviewType;
            target.LanguageCode = LanguageCode;
        }

    }
}
