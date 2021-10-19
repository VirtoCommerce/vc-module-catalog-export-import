using System.Collections.Generic;
using System.IO;
using System.Text;
using Moq;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Data.Services;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.CatalogExportImportModule.Tests
{
    public static class TestHelper
    {
        public static IBlobStorageProvider GetBlobStorageProvider(string csv, MemoryStream errorReporterMemoryStream = null)
        {
            errorReporterMemoryStream ??= new MemoryStream();
            var blobStorageProviderMock = new Mock<IBlobStorageProvider>();
            var stream = GetStream(csv);
            blobStorageProviderMock.Setup(x => x.OpenRead(It.IsAny<string>())).Returns(() => stream);
            blobStorageProviderMock.Setup(x => x.OpenWrite(It.IsAny<string>())).Returns(() => errorReporterMemoryStream);
            blobStorageProviderMock.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .ReturnsAsync(() => new BlobInfo { Size = stream.Length });
            return blobStorageProviderMock.Object;
        }

        public static ImportPagedDataSourceFactory GetCustomerImportPagedDataSourceFactory(IBlobStorageProvider blobStorageProvider)
        {
            return new ImportPagedDataSourceFactory(blobStorageProvider, new ImportConfigurationFactory());
        }

        public static Stream GetStream(string csv)
        {
            var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, leaveOpen: true);
            writer.Write(csv);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string GetCsv(IEnumerable<string> records, string header = null)
        {
            var csv = new StringBuilder();

            if (header != null)
            {
                csv.AppendLine(header);
            }

            foreach (var record in records)
            {
                csv.AppendLine(record);
            }

            return csv.ToString();
        }
    }
}
