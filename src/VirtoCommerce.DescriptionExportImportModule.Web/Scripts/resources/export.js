angular.module('virtoCommerce.descriptionExportImportModule')
    .factory('virtoCommerce.descriptionExportImportModule.export', ['$resource', function ($resource) {
        return $resource('api/description/export', null,
            {
                run: { method: 'POST', url: 'api/description/export/run'},
                cancel: { method: 'POST', url: 'api/description/export/cancel'}
            });

    }])
