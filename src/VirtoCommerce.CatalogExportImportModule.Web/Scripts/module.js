import 'ui-grid-auto-fit-columns';

// Call this to register your module to main application
var moduleName = "virtoCommerce.catalogExportImportModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, ['ui.grid.autoFitColumns']).run(['virtoCommerce.catalogModule.catalogImportService', 'virtoCommerce.catalogModule.catalogExportService', function (catalogImportService, catalogExportService) {
    catalogImportService.register({
        name: 'Descriptions import',
        description: 'Descriptions data import from CSV',
        icon: 'fa fa-list-alt',
        controller: 'virtoCommerce.catalogExportImportModule.fileUploadController',
        template: 'Modules/$(VirtoCommerce.CatalogExportImport)/Scripts/blades/file-upload.tpl.html'
    });

    catalogExportService.register({
        name: 'Descriptions export',
        description: 'Export only product descriptions',
        icon: 'fa fa-list-alt',
        controller: 'virtoCommerce.catalogExportImportModule.exportProcessingController',
        template: 'Modules/$(VirtoCommerce.CatalogExportImport)/Scripts/blades/export-processing.tpl.html'
    });
}]);
