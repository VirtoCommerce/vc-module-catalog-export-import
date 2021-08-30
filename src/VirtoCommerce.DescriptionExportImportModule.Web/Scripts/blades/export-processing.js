angular.module('virtoCommerce.descriptionExportImportModule')
.controller('virtoCommerce.descriptionExportImportModule.exportProcessingController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.listEntries', 'platformWebApp.settings', '$q', 'platformWebApp.dialogService', 'virtoCommerce.descriptionExportImportModule.export',
    function ($scope, bladeNavigationService, catalogListEntriesApi, settings, $q, dialogService, exportResources) {
        var blade = $scope.blade;
        blade.title = 'descriptionExportImport.blades.export-processing.title';
        blade.headIcon = "fa fa-download";

        function initialize() {
            const catalogSearchRequest = catalogListEntriesApi.listitemssearch({ categoryId: blade.parentBlade.categoryId, catalogId: blade.parentBlade.catalogId }).$promise;
            const getExportLimits = settings.getValues({ id: 'DescriptionExportImport.Export.LimitOfLines' }).$promise;

            $q.all([catalogSearchRequest, getExportLimits]).then(([catalogSearchResponse, exportLimitResponse]) => {
                const productsTotalNumber = catalogSearchResponse.totalCount;
                const exportLimit = exportLimitResponse[0];
                if (productsTotalNumber > exportLimit) {
                    $scope.bladeClose();
                    showWarningDialog(productsTotalNumber, exportLimit);
                } else {
                    const exportDataRequest = {
                        categoryId: blade.parentBlade.categoryId,
                        catalogId: blade.parentBlade.catalogId
                    }
                    exportResources.run(exportDataRequest, (data) => {
                        blade.notification = data;
                        blade.isLoading = false;
                    });
                }
            });
        }

        $scope.$on("new-notification-event", function (event, notification) {
            if (blade.notification && notification.id === blade.notification.id) {
                angular.copy(notification, blade.notification);
            }
        });

        blade.toolbarCommands = [{
            name: 'platform.commands.cancel',
            icon: 'fa fa-times',
            canExecuteMethod: function() {
                return blade.notification && !blade.notification.finished;
            },
            executeMethod: function() {
                exportResources.cancel({ jobId: blade.notification.jobId });
            }
        }];

        $scope.bladeClose = () => {
            bladeNavigationService.closeBlade(blade);
        }

        $scope.extractFileName = (fileUrl) => {
            return fileUrl.split(/[\\\/]/).pop();
        }

        function showWarningDialog(itemsQty, limitQty) {
            const dialog = {
                id: 'exportLimitWarningDialog',
                itemsQty,
                limitQty
            };
            dialogService.showDialog(dialog, 'Modules/$(VirtoCommerce.DescriptionExportImport)/Scripts/dialogs/exportLimitWarning-dialog.tpl.html', 'platformWebApp.confirmDialogController');
        }

        initialize();
    }]);
