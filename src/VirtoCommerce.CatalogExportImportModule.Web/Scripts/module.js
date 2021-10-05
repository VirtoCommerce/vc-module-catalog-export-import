import 'ui-grid-auto-fit-columns';

// Call this to register your module to main application
var moduleName = "virtoCommerce.catalogExportImportModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, ['ui.grid.autoFitColumns']).run(['virtoCommerce.catalogModule.catalogImportService', 'virtoCommerce.catalogModule.catalogExportService', function (catalogImportService, catalogExportService) {
    catalogImportService.register({
        name: 'Description import',
        description: 'Descriptions data import from CSV',
        icon: 'fa fa-list-alt',
        controller: 'virtoCommerce.catalogExportImportModule.fileUploadController',
        template: 'Modules/$(VirtoCommerce.CatalogExportImport)/Scripts/blades/file-upload.tpl.html',
        predefinedDataType: { key: 'Descriptions', value: 'EditorialReview' }
    });

    catalogExportService.register({
        name: 'Description export',
        description: 'Export only product descriptions',
        icon: 'fa fa-list-alt',
        controller: 'virtoCommerce.catalogExportImportModule.exportProcessingController',
        template: 'Modules/$(VirtoCommerce.CatalogExportImport)/Scripts/blades/export-processing.tpl.html'
    });

    catalogImportService.register({
        name: 'Physical products import',
        description: 'Physical products data import from CSV',
        icon: 'fas fa-box',
        controller: 'virtoCommerce.catalogExportImportModule.fileUploadController',
        template: 'Modules/$(VirtoCommerce.CatalogExportImport)/Scripts/blades/file-upload.tpl.html',
        predefinedDataType: { key: 'Physical Products', value: 'CatalogProduct' }
    });
}]);
