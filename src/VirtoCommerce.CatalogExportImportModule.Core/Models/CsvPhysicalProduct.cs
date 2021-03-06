using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Include)]
    public sealed class CsvPhysicalProduct : IImportable, IExportable, IHasProperties
    {
        [Required]
        [Name("Product Name")]
        public string ProductName { get; set; }

        [Optional]
        [Name("Product Id")]
        public string ProductId { get; set; }

        [Optional]
        [Name("Product Outer Id")]
        public string ProductOuterId { get; set; }

        [Required]
        [Name("Product SKU")]
        public string ProductSku { get; set; }

        [Optional]
        [Name("Main Product Id")]
        public string MainProductId { get; set; }

        [Optional]
        [Name("Main Product Outer Id")]
        public string MainProductOuterId { get; set; }

        [Optional]
        [Name("Category Id")]
        public string CategoryId { get; set; }

        [Optional]
        [Name("Category Outer Id")]
        public string CategoryOuterId { get; set; }

        [Optional]
        [Name("Product Type")]
        public string ProductType { get; set; }

        [Optional]
        [Name("Priority")]
        public int? Priority { get; set; }

        [Optional]
        [Name("Gtin")]
        public string Gtin { get; set; }

        [Optional]
        [Name("Can be purchased")]
        [BooleanTrueValues("true", "yes")]
        [BooleanFalseValues("false", "no")]
        public bool? CanBePurchased { get; set; }

        [Optional]
        [Name("Visible")]
        [BooleanTrueValues("true", "yes")]
        [BooleanFalseValues("false", "no")]
        public bool? Visible { get; set; }

        [Optional]
        [Name("Track Inventory")]
        [BooleanTrueValues("true", "yes")]
        [BooleanFalseValues("false", "no")]
        public bool? TrackInventory { get; set; }

        [Optional]
        [Name("Package Type")]
        public string PackageType { get; set; }

        [Optional]
        [Name("Description Id")]
        public string DescriptionId { get; set; }

        [Optional]
        [Name("Description Type")]
        public string DescriptionType { get; set; }

        [Optional]
        [Name("Description Language")]
        public string DescriptionLanguage { get; set; }

        [Optional]
        [Name("Description")]
        public string Description { get; set; }

        [Optional]
        [Name("Max Quantity")]
        public int? MaxQuantity { get; set; }

        [Optional]
        [Name("Min Quantity")]
        public int? MinQuantity { get; set; }

        [Optional]
        [Name("Manufacturer Part Number")]
        public string ManufacturerPartNumber { get; set; }

        [Optional]
        [Name("Measure Unit")]
        public string MeasureUnit { get; set; }

        [Optional]
        [Name("Weight Unit")]
        public string WeightUnit { get; set; }

        [Optional]
        [Name("Weight")]
        public decimal? Weight { get; set; }

        [Optional]
        [Name("Height")]
        public decimal? Height { get; set; }

        [Optional]
        [Name("Length")]
        public decimal? Length { get; set; }

        [Optional]
        [Name("Width")]
        public decimal? Width { get; set; }

        [Optional]
        [Name("Tax Type")]
        public string TaxType { get; set; }

        [Optional]
        [Name("Shipping Type")]
        public string ShippingType { get; set; }

        [Optional]
        [Name("Vendor")]
        public string Vendor { get; set; }

        [Optional]
        [Name("First Listed")]
        public DateTime? FirstListed { get; set; }

        [Optional]
        [Name("Listing Expires On")]
        public DateTime? ListingExpiresOn { get; set; }

        [Ignore]
        public IList<Property> Properties { get; set; } = new List<Property>();

        public CsvPhysicalProduct FromModel(CatalogProduct product)
        {
            ProductName = product.Name;
            ProductId = product.Id;
            MainProductId = product.MainProductId;
            ProductSku = product.Code;
            ProductOuterId = product.OuterId;
            CategoryId = product.CategoryId;
            CategoryOuterId = product.Category?.OuterId;
            ProductType = product.ProductType;
            Priority = product.Priority;
            Gtin = product.Gtin;
            CanBePurchased = product.IsBuyable;
            Visible = product.IsActive;
            TrackInventory = product.TrackInventory;
            PackageType = product.PackageType;
            MaxQuantity = product.MaxQuantity;
            MinQuantity = product.MinQuantity;
            ManufacturerPartNumber = product.ManufacturerPartNumber;
            MeasureUnit = product.MeasureUnit;
            WeightUnit = product.WeightUnit;
            Weight = product.Weight;
            Height = product.Height;
            Length = product.Length;
            Width = product.Width;
            TaxType = product.TaxType;
            ShippingType = product.ShippingType;
            Vendor = product.Vendor;
            FirstListed = product.StartDate;
            ListingExpiresOn = product.EndDate;
            Properties = product.Properties?.Select(x => x.Clone() as Property).ToArray();

            if (!product.Reviews.IsNullOrEmpty())
            {
                var description = product.Reviews[0];
                DescriptionId = description.Id;
                Description = description.Content;
                DescriptionLanguage = description.LanguageCode;
                DescriptionType = description.ReviewType;
            }

            return this;
        }

        public void PatchModel(CatalogProduct target)
        {
            target.Name = ProductName;
            target.Code = ProductSku;
            target.OuterId = ProductOuterId;
            target.MainProductId = string.IsNullOrEmpty(MainProductId) ? null : MainProductId;
            target.CategoryId = CategoryId;
            target.ProductType = ModuleConstants.ProductTypes.Physical;
            target.Priority = Priority ?? 0;
            target.Gtin = Gtin;
            target.IsBuyable = CanBePurchased;
            target.IsActive = Visible;
            target.TrackInventory = TrackInventory;
            target.PackageType = PackageType;
            target.MaxQuantity = MaxQuantity;
            target.MinQuantity = MinQuantity;
            target.ManufacturerPartNumber = ManufacturerPartNumber;
            target.MeasureUnit = MeasureUnit;
            target.WeightUnit = WeightUnit;
            target.Weight = Weight;
            target.Height = Height;
            target.Length = Length;
            target.Width = Width;
            target.TaxType = TaxType;
            target.ShippingType = ShippingType;
            target.Vendor = Vendor;
            target.StartDate = FirstListed ?? DateTime.UtcNow;
            target.EndDate = ListingExpiresOn;

            target.Properties = Properties;
        }

        public void PatchDescription(EditorialReview review)
        {
            review.Content = Description;
            review.LanguageCode = DescriptionLanguage;
            review.ReviewType = DescriptionType;
        }

    }
}
