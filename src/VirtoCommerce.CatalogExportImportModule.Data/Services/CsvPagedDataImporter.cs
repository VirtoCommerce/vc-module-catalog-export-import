using System;
using System.IO;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public abstract class CsvPagedDataImporter<TImportable> : ICsvPagedDataImporter
         where TImportable : IImportable
    {
        private readonly ICsvImportErrorReporterFactory _importReporterFactory;
        private readonly IImportPagedDataSourceFactory _dataSourceFactory;
        private readonly ICsvParsingErrorHandlerFactory _parsingErrorHandlerFactory;
        private readonly IBlobUrlResolver _blobUrlResolver;
        private readonly IImportConfigurationFactory _importConfigurationFactory;

        public abstract string DataType { get; }

        protected CsvPagedDataImporter(
            IBlobUrlResolver blobUrlResolver,
            IImportConfigurationFactory importConfigurationFactory,
            IImportPagedDataSourceFactory dataSourceFactory,
            ICsvParsingErrorHandlerFactory parsingErrorHandlerFactory,
            ICsvImportErrorReporterFactory importReporterFactory)
        {
            _blobUrlResolver = blobUrlResolver;
            _dataSourceFactory = dataSourceFactory;
            _parsingErrorHandlerFactory = parsingErrorHandlerFactory;
            _importConfigurationFactory = importConfigurationFactory;
            _importReporterFactory = importReporterFactory;
        }

        protected virtual async Task<ClassMap<TImportable>> GetClassMapAsync(ImportDataRequest request)
        {
            return await Task.FromResult<ClassMap<TImportable>>(null);
        }

        public virtual async Task ImportAsync(ImportDataRequest request, Action<ImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            ValidateParameters(request, progressCallback, cancellationToken);

            // Throw error if operation was cancelled by user
            cancellationToken.ThrowIfCancellationRequested();

            // Send message about import start
            var importProgress = new ImportProgressInfo { Description = "Import has started" };
            progressCallback(importProgress);

            // Setup configuration
            // Be careful: configuration is copied inside csv reader,
            // so changes made after passing it to data source will has no effect
            var configuration = _importConfigurationFactory.Create();

            // Setup error reporter
            var reportFilePath = GetReportFilePath(request.FilePath);
            await using var importErrorReporter = await _importReporterFactory.CreateAsync(reportFilePath, configuration.Delimiter);

            // Setup parsing error handling
            var parsingErrorHandler = _parsingErrorHandlerFactory.Create(importErrorReporter);
            parsingErrorHandler.HandleErrors(configuration);

            // Setup data source & class maps
            using var dataSource = _dataSourceFactory.Create<TImportable>(request.FilePath, ModuleConstants.Settings.PageSize, configuration);
            var classMap = await GetClassMapAsync(request);
            if (classMap != null)
            {
                dataSource.RegisterClassMap(classMap);
            }

            // Writer header to error report
            var header = dataSource.GetHeaderRaw();
            importErrorReporter.WriteHeader(header);

            // Send message about total count
            importProgress.TotalCount = dataSource.GetTotalCount();
            progressCallback(importProgress);

            const string importDescription = "{0} out of {1} have been imported.";

            try
            {
                importProgress.Description = "Fetching...";
                progressCallback(importProgress);

                // Get next chunk
                while (await dataSource.FetchAsync())
                {
                    // Validate & process chunk
                    await ProcessChunkAsync(request, progressCallback, dataSource, importErrorReporter, importProgress);

                    // Write error messages
                    await importErrorReporter.FlushAsync();

                    // Send message about processed count
                    if (importProgress.ProcessedCount != importProgress.TotalCount)
                    {
                        importProgress.Description = string.Format(importDescription, importProgress.ProcessedCount, importProgress.TotalCount);
                        progressCallback(importProgress);
                    }
                }
            }
            catch (Exception e)
            {
                // Send message about unhandled exception
                importProgress.Errors.Add(e.Message);
                progressCallback(importProgress);
            }
            finally
            {
                // Send message about processing complete
                var completedMessage = importProgress.ErrorCount > 0 ? "Import completed with errors" : "Import completed";
                importProgress.Description = $"{completedMessage}: {string.Format(importDescription, importProgress.ProcessedCount, importProgress.TotalCount)}";

                // Include url to error report if not empty
                if (importErrorReporter.ReportIsNotEmpty)
                {
                    importProgress.ReportUrl = _blobUrlResolver.GetAbsoluteUrl(reportFilePath);
                }

                progressCallback(importProgress);
            }
        }


        protected abstract Task ProcessChunkAsync(ImportDataRequest request, Action<ImportProgressInfo> progressCallback,
            IImportPagedDataSource<TImportable> dataSource, ICsvImportErrorReporter importErrorReporter, ImportProgressInfo importProgress);
        
        protected static string GetReportFilePath(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var fileExtension = Path.GetExtension(fileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var reportFileName = $"{fileNameWithoutExtension}_report{fileExtension}";
            var result = filePath.Replace(fileName, reportFileName);

            return result;
        }

        private static void ValidateParameters(ImportDataRequest request, Action<ImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (progressCallback == null)
            {
                throw new ArgumentNullException(nameof(progressCallback));
            }

            if (cancellationToken == null)
            {
                throw new ArgumentNullException(nameof(cancellationToken));
            }
        }
    }
}
