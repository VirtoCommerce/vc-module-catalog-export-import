angular.module('virtoCommerce.catalogExportImportModule')
    .factory('virtoCommerce.catalogExportImportModule.export', ['$resource', function ($resource) {
        return $resource('api/catalog/export', null,
            {
                run: { method: 'POST', url: 'api/catalog/export/run'},
                cancel: { method: 'POST', url: 'api/catalog/export/cancel'},
                count: {
                    method: 'POST', url: 'api/catalog/export/count'
                }
            });

    }])
