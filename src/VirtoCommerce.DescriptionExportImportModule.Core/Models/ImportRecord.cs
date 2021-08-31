namespace VirtoCommerce.DescriptionExportImportModule.Core.Models
{
    public class ImportRecord<TRecord>
    {
        public int Row { get; set; }

        public string RawRecord { get; set; }

        public TRecord Record { get; set; }
    }
}
