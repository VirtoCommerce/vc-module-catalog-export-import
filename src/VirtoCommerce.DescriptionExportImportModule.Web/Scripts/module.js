import 'ui-grid-auto-fit-columns';

// Call this to register your module to main application
var moduleName = "virtoCommerce.descriptionExportImportModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, ['ui.grid.autoFitColumns']).run(['virtoCommerce.catalogModule.catalogImportService', 'virtoCommerce.catalogModule.catalogExportService', function (catalogImportService, catalogExportService) {
    catalogImportService.register({
        name: 'Description import',
        description: 'Descriptions data import from CSV',
        icon: 'fa fa-list-alt',
        controller: 'virtoCommerce.descriptionExportImportModule.fileUploadController',
        template: 'Modules/$(VirtoCommerce.DescriptionExportImport)/Scripts/blades/file-upload.tpl.html'
    });

    catalogExportService.register({
        name: 'Description export',
        description: 'Export only product descriptions',
        icon: 'fa fa-list-alt',
        controller: 'virtoCommerce.descriptionExportImportModule.exportProcessingController',
        template: 'Modules/$(VirtoCommerce.DescriptionExportImport)/Scripts/blades/export-processing.tpl.html'
    });
}]);
