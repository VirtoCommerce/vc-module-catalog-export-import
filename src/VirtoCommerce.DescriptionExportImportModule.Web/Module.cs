using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VirtoCommerce.DescriptionExportImportModule.Core;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;
using VirtoCommerce.DescriptionExportImportModule.Data.Repositories;
using VirtoCommerce.DescriptionExportImportModule.Data.Services;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.DescriptionExportImportModule.Web
{
    public class Module : IModule, IHasConfiguration
    {
        public ManifestModuleInfo ModuleInfo { get; set; }

        public IConfiguration Configuration { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            // initialize DB
            serviceCollection.AddDbContext<VirtoCommerceDescriptionExportImportDbContext>((provider, options) =>
           {
               var configuration = provider.GetRequiredService<IConfiguration>();
               options.UseSqlServer(configuration.GetConnectionString(ModuleInfo.Id) ?? configuration.GetConnectionString("VirtoCommerce"));
           });
            serviceCollection.AddTransient<ICsvDataValidator, CsvDataValidator>();
            serviceCollection.AddOptions<ImportOptions>().Bind(Configuration.GetSection("DescriptionExportImport:Import")).ValidateDataAnnotations();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            // register settings
            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.General.AllSettings, ModuleInfo.Id);

            var settingsManager = appBuilder.ApplicationServices.GetService<ISettingsManager>();
            var descriptionImportOptions = appBuilder.ApplicationServices.GetService<IOptions<ImportOptions>>().Value;

            settingsManager.SetValue(ModuleConstants.Settings.General.ImportLimitOfLines.Name,
                descriptionImportOptions.LimitOfLines ?? ModuleConstants.Settings.General.ImportLimitOfLines.DefaultValue);

            settingsManager.SetValue(ModuleConstants.Settings.General.ImportFileMaxSize.Name,
                descriptionImportOptions.FileMaxSize ?? ModuleConstants.Settings.General.ImportFileMaxSize.DefaultValue);

            // register permissions
            var permissionsProvider = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsProvider.RegisterPermissions(ModuleConstants.Security.Permissions.AllPermissions.Select(x =>
                new Permission()
                {
                    GroupName = "DescriptionExportImport",
                    ModuleId = ModuleInfo.Id,
                    Name = x
                }).ToArray());

            // ensure that all pending migrations are applied
            using var serviceScope = appBuilder.ApplicationServices.CreateScope();
            using var dbContext = serviceScope.ServiceProvider.GetRequiredService<VirtoCommerceDescriptionExportImportDbContext>();
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();
        }

        public void Uninstall()
        {
            // do nothing in here
        }
    }
}
