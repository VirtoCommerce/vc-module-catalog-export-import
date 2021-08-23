angular.module('virtoCommerce.descriptionExportImport')
    .controller('virtoCommerce.descriptionExportImport.helloWorldController', ['$scope', 'virtoCommerce.descriptionExportImport.webApi', function ($scope, api) {
        var blade = $scope.blade;
        blade.title = 'VirtoCommerce.DescriptionExportImportModule';

        blade.refresh = function () {
            api.get(function (data) {
                blade.title = 'virtoCommerce.descriptionExportImport.blades.hello-world.title';
                blade.data = data.result;
                blade.isLoading = false;
            });
        };

        blade.refresh();
    }]);
