// Call this to register your module to main application
var moduleName = "virtoCommerce.descriptionExportImportModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, []).run(['virtoCommerce.catalogModule.catalogImportService', function (catalogImportService) {
    catalogImportService.register({
        name: 'Description import',
        description: 'Descriptions data import from CSV',
        icon: 'fa fa-list-alt',
        controller: 'virtoCommerce.descriptionExportImportModule.fileUploadController',
        template: 'Modules/$(VirtoCommerce.DescriptionExportImport)/Scripts/blades/file-upload.tpl.html'
    });
}]);
