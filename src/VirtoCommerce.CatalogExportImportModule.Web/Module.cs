using System;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogExportImportModule.Data.Repositories;
using VirtoCommerce.CatalogExportImportModule.Data.Services;
using VirtoCommerce.CatalogExportImportModule.Data.Validation;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogExportImportModule.Web
{
    public class Module : IModule, IHasConfiguration
    {
        public ManifestModuleInfo ModuleInfo { get; set; }

        public IConfiguration Configuration { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            // Initialize DB
            serviceCollection.AddDbContext<VirtoCommerceCatalogExportImportDbContext>((provider, options) =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                options.UseSqlServer(configuration.GetConnectionString(ModuleInfo.Id) ?? configuration.GetConnectionString("VirtoCommerce"));
            });

            // Initialize settings
            serviceCollection.AddOptions<ImportOptions>().Bind(Configuration.GetSection("CatalogExportImport:Import")).ValidateDataAnnotations();
            serviceCollection.AddOptions<ExportOptions>().Bind(Configuration.GetSection("CatalogExportImport:Export")).ValidateDataAnnotations();

            // Initialize editorial review services
            // Workaround. Should be replaced with regular catalog module services after their improvement
            serviceCollection.AddTransient<IProductEditorialReviewService, ProductEditorialReviewService>();
            serviceCollection.AddTransient<IProductEditorialReviewSearchService, ProductEditorialReviewSearchService>();

            // Initialize catalog module services overrides, extensions & replacements for export
            // Workaround. Should be replaced with regular catalog module services after their improvement
            serviceCollection.AddTransient<IExportProductSearchService, ExportProductSearchService>();
            serviceCollection.AddTransient<IPropertyLoader, PropertyLoader>();

            // Initialize export services
            // Data request
            serviceCollection.AddTransient<IExportDataRequestPreprocessor, ExportDataRequestPreprocessor>();
            // Data source
            serviceCollection.AddSingleton<IExportPagedDataSourceFactory, ExportPagedDataSourceFactory>();
            serviceCollection.AddTransient<Func<ExportDataRequest, int, IExportPagedDataSource>>(serviceProvider =>
                (request, pageSize) => new EditorialReviewExportPagedDataSource(serviceProvider.GetService<IProductEditorialReviewSearchService>(), serviceProvider.GetService<IItemService>(), pageSize, request));
            serviceCollection.AddTransient<Func<ExportDataRequest, int, IExportPagedDataSource>>(serviceProvider =>
                (request, pageSize) => new ProductExportPagedDataSource(serviceProvider.GetService<IExportProductSearchService>(), pageSize, request, serviceProvider.GetService<IItemService>()));
            // Exporters
            serviceCollection.AddTransient<IDataExporter, EditorialReviewDataExporter>();
            serviceCollection.AddTransient<IDataExporter, PhysicalProductDataExporter>();
            // Writer
            serviceCollection.AddSingleton<IExportWriterFactory, ExportWriterFactory>();

            // Initialize catalog module services overrides, extensions & replacements for import
            // Workaround. Should be replaced with regular catalog module services after their improvement
            serviceCollection.AddTransient<IImportProductSearchService, ImportProductSearchService>();
            serviceCollection.AddTransient<IImportCategorySearchService, ImportCategorySearchService>();
            serviceCollection.AddTransient<IListEntryIndexedSearchService, ListEntryIndexedSearchService>();

            // Initialize import services
            // Configuration & class maps
            serviceCollection.AddSingleton<IImportConfigurationFactory, ImportConfigurationFactory>();
            serviceCollection.AddTransient<IImportProductsClassMapFactory, ImportProductsClassMapFactory>();
            // Data source
            serviceCollection.AddSingleton<IImportPagedDataSourceFactory, ImportPagedDataSourceFactory>();
            // Error handling
            serviceCollection.AddTransient<ICsvFileValidator, CsvFileValidator>();
            serviceCollection.AddTransient<IValidator<ImportRecord<CsvEditorialReview>[]>, ImportReviewsValidator>();
            serviceCollection.AddTransient<IValidator<ImportRecord<CsvPhysicalProduct>[]>, ImportPhysicalProductsValidator>();
            serviceCollection.AddSingleton<ICsvParsingErrorHandlerFactory, CsvParsingErrorHandlerFactory>();
            serviceCollection.AddSingleton<ICsvValidatorFactory, CsvValidatorFactory>();
            serviceCollection.AddSingleton<ICsvImportErrorReporterFactory, CsvImportErrorReporterFactory>();
            // Importers
            serviceCollection.AddTransient<ICsvPagedDataImporter, CsvPagedEditorialReviewDataImporter>();
            serviceCollection.AddTransient<ICsvPagedDataImporter, CsvPagedPhysicalProductDataImporter>();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            AbstractTypeFactory<EditorialReview>.OverrideType<EditorialReview, ExtendedEditorialReview>();

            // register settings
            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.General.AllSettings, ModuleInfo.Id);

            var settingsManager = appBuilder.ApplicationServices.GetService<ISettingsManager>();
            var importOptions = appBuilder.ApplicationServices.GetService<IOptions<ImportOptions>>().Value;
            var exportOptions = appBuilder.ApplicationServices.GetService<IOptions<ExportOptions>>().Value;

            settingsManager.SetValue(ModuleConstants.Settings.General.ImportLimitOfLines.Name,
                importOptions.LimitOfLines ?? ModuleConstants.Settings.General.ImportLimitOfLines.DefaultValue);

            settingsManager.SetValue(ModuleConstants.Settings.General.ImportFileMaxSize.Name,
                importOptions.FileMaxSize ?? ModuleConstants.Settings.General.ImportFileMaxSize.DefaultValue);

            settingsManager.SetValue(ModuleConstants.Settings.General.ExportLimitOfLines.Name,
                exportOptions.LimitOfLines ?? ModuleConstants.Settings.General.ExportLimitOfLines.DefaultValue);

            // register permissions
            var permissionsProvider = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsProvider.RegisterPermissions(ModuleConstants.Security.Permissions.AllPermissions.Select(x =>
                new Permission()
                {
                    GroupName = "CatalogExportImport",
                    ModuleId = ModuleInfo.Id,
                    Name = x
                }).ToArray());

            // ensure that all pending migrations are applied
            using var serviceScope = appBuilder.ApplicationServices.CreateScope();
            using var dbContext = serviceScope.ServiceProvider.GetRequiredService<VirtoCommerceCatalogExportImportDbContext>();
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();
        }

        public void Uninstall()
        {
            // do nothing in here
        }
    }
}
