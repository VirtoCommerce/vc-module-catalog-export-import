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
using VirtoCommerce.FeatureManagementModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using featureManagementCore = VirtoCommerce.FeatureManagementModule.Core;

namespace VirtoCommerce.CatalogExportImportModule.Web
{
    public class Module : IModule, IHasConfiguration
    {
        public ManifestModuleInfo ModuleInfo { get; set; }

        public IConfiguration Configuration { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            // initialize DB
            serviceCollection.AddDbContext<VirtoCommerceCatalogExportImportDbContext>((provider, options) =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                options.UseSqlServer(configuration.GetConnectionString(ModuleInfo.Id) ?? configuration.GetConnectionString("VirtoCommerce"));
            });
            serviceCollection.AddOptions<ImportOptions>().Bind(Configuration.GetSection("CatalogExportImport:Import")).ValidateDataAnnotations();
            serviceCollection.AddOptions<ExportOptions>().Bind(Configuration.GetSection("CatalogExportImport:Export")).ValidateDataAnnotations();

            serviceCollection.AddTransient<ICsvDataValidator, CsvDataValidator>();

            serviceCollection.AddTransient<IProductEditorialReviewSearchService, ProductEditorialReviewSearchService>();
            serviceCollection.AddSingleton<IExportPagedDataSourceFactory, ExportPagedDataSourceFactory>();
            serviceCollection.AddTransient<IDataExporter, EditorialReviewDataExporter>();
            serviceCollection.AddTransient<IDataExporter, PhysicalProductDataExporter>();
            serviceCollection.AddTransient<IExportDataRequestPreprocessor, ExportDataRequestPreprocessor>();
            serviceCollection.AddTransient<IProductEditorialReviewService, ProductEditorialReviewService>();
            serviceCollection.AddTransient<IExportProductSearchService, ExportProductSearchService>();
            serviceCollection.AddSingleton<IExportWriterFactory, ExportWriterFactory>();
            serviceCollection.AddSingleton<IImportPagedDataSourceFactory, ImportPagedDataSourceFactory>();
            serviceCollection.AddTransient<IValidator<ImportRecord<CsvEditorialReview>[]>, ImportReviewsValidator>();
            serviceCollection.AddSingleton<ICsvImportReporterFactory, CsvImportReporterFactory>();

            serviceCollection.AddTransient<Func<ExportDataRequest, int, IExportPagedDataSource>>(serviceProvider =>
                (request, pageSize) => new EditorialReviewExportPagedDataSource(serviceProvider.GetService<IProductEditorialReviewSearchService>(), serviceProvider.GetService<IItemService>(), pageSize, request));
            serviceCollection.AddTransient<Func<ExportDataRequest, int, IExportPagedDataSource>>(serviceProvider =>
                (request, pageSize) => new ProductExportPagedDataSource(serviceProvider.GetService<IExportProductSearchService>(), pageSize, request));

            serviceCollection.AddTransient<ICsvPagedDataImporter, CsvPagedEditorialReviewDataImporter>();

            // Workaround. Should be excluded when the catalog module's bug  will be excepted https://virtocommerce.atlassian.net/browse/PT-4224.
            serviceCollection.AddTransient<IListEntryIndexedSearchService, ListEntryIndexedSearchService>();
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

            var featureStorage = appBuilder.ApplicationServices.GetService<IFeatureStorage>();
            featureStorage.TryAddFeatureDefinition(ModuleConstants.Features.CatalogExportImport, featureManagementCore.ModuleConstants.FeatureFilters.Developers);

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
