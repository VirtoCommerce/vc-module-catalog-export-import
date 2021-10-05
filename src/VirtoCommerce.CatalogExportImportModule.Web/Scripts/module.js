import 'ui-grid-auto-fit-columns';

// Call this to register your module to main application
var moduleName = "virtoCommerce.catalogExportImportModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

var module = angular.module(moduleName, ['ui.grid.autoFitColumns']).run(['virtoCommerce.featureManagerSubscriber', 'virtoCommerce.catalogModule.catalogImportService', 'virtoCommerce.catalogModule.catalogExportService', function (featureManagerSubscriber, catalogImportService, catalogExportService) {

    catalogImportService.register({
        name: 'Descriptions import',
        description: 'Descriptions data import from CSV',
        icon: 'fa fa-list-alt',
        controller: 'virtoCommerce.catalogExportImportModule.fileUploadController',
        template: 'Modules/$(VirtoCommerce.CatalogExportImport)/Scripts/blades/file-upload.tpl.html',
        predefinedDataType: { key: 'Descriptions', value: 'EditorialReview' }
    });

    catalogExportService.register({
        name: 'Descriptions export',
        description: 'Export only product descriptions',
        icon: 'fa fa-list-alt',
        dataType: 'descriptions',
        controller: 'virtoCommerce.catalogExportImportModule.exportProcessingController',
        template: 'Modules/$(VirtoCommerce.CatalogExportImport)/Scripts/blades/export-processing.tpl.html'
    });

    catalogExportService.register({
        name: 'Physical products export',
        description: 'Export only physical products',
        icon: 'fas fa-box',
        dataType: 'physicalProducts',
        controller: 'virtoCommerce.catalogExportImportModule.exportProcessingController',
        template: 'Modules/$(VirtoCommerce.CatalogExportImport)/Scripts/blades/export-processing.tpl.html'
    });

    featureManagerSubscriber.onLoginStatusChanged('CatalogExportImport', () => {
        catalogImportService.register({
            name: 'Physical products import',
            description: 'Physical products data import from CSV',
            icon: 'fas fa-box',
            controller: 'virtoCommerce.catalogExportImportModule.fileUploadController',
            template: 'Modules/$(VirtoCommerce.CatalogExportImport)/Scripts/blades/file-upload.tpl.html',
            predefinedDataType: { key: 'Physical Products', value: 'CatalogProduct' }
        });
    });
}]);

module.constant('availableDataTypes', [{ key: 'Descriptions', value: 'EditorialReview' }, { key: 'Physical Products', value: 'CatalogProduct' }]);
module.constant('editorialReview', 'EditorialReview');
