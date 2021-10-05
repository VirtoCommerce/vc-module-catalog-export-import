using System;
using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;

namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public sealed class CsvPhysicalProduct : IImportable, IExportable
    {
        [Required]
        [Name("Product Name")]
        public string ProductName { get; set; }

        [Optional]
        [Name("Product Id")]
        public string ProductId { get; set; }

        [Required]
        [Name("Product SKU")]
        public string ProductSku { get; set; }

        [Optional]
        [Name("Product Outer Id")]
        public string ProductOuterId { get; set; }

        [Optional]
        [Name("Category Id")]
        public string CategoryId { get; set; }

        [Optional]
        [Name("Category Outer Id")]
        public string CategoryOuterId { get; set; }

        [Required]
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
        [BooleanTrueValues("yes", "true")]
        [BooleanFalseValues("no", "false")]
        public bool? CanBePurchased { get; set; }

        [Optional]
        [Name("Visible")]
        [BooleanTrueValues("yes", "true")]
        [BooleanFalseValues("no", "false")]
        public bool? Visible { get; set; }

        [Optional]
        [Name("Track Inventory")]
        [BooleanTrueValues("yes", "true")]
        [BooleanFalseValues("no", "false")]
        public bool? TrackInventory { get; set; }

        [Optional]
        [Name("Package Type")]
        public string PackageType { get; set; }

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
        public string Weight { get; set; }

        [Optional]
        [Name("Lenght")]
        public string Lenght { get; set; }

        [Optional]
        [Name("Width")]
        public string Width { get; set; }

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

    }
}