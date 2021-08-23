angular.module('virtoCommerce.descriptionExportImport')
    .factory('virtoCommerce.descriptionExportImport.webApi', ['$resource', function ($resource) {
        return $resource('api/VirtoCommerceDescriptionExportImport');
}]);
