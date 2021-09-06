angular.module('virtoCommerce.descriptionExportImportModule')
.controller('virtoCommerce.descriptionExportImportModule.exportProcessingController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.settings', '$q', 'platformWebApp.dialogService', 'virtoCommerce.descriptionExportImportModule.export',
    function ($scope, bladeNavigationService, settings, $q, dialogService, exportResources) {
        var blade = $scope.blade;
        blade.title = 'descriptionExportImport.blades.export-processing.title';
        blade.headIcon = "fa fa-download";

        function initialize() {
            const exportDataRequest = getExportRequest();
            const getTotalCountPromise = exportResources.count(exportDataRequest).$promise;
            const getExportLimitsPromise = settings.getValues({ id: 'DescriptionExportImport.Export.LimitOfLines' }).$promise;

            $q.all([getTotalCountPromise, getExportLimitsPromise]).then(([totalCountResponse, exportLimitResponse]) => {
                const descriptionsTotalCount = totalCountResponse.totalCount;
                const exportLimit = exportLimitResponse[0];
                if (descriptionsTotalCount > exportLimit) {
                    $scope.bladeClose();
                    showWarningDialog(descriptionsTotalCount, exportLimit);
                } else {
                    exportResources.run(exportDataRequest, (data) => {
                        blade.notification = data;
                        blade.isLoading = false;
                    });
                }
            });
        }

        function getExportRequest() {
            return {
                catalogId:
                    blade.catalog.id || "",
                categoryIds:
                    blade.selectedCategories.length > 0
                        ? blade.selectedCategories.map((category) => category.id)
                        : getParentCategoryId(blade),
                itemIds:
                    blade.selectedProducts.length > 0 ? blade.selectedProducts.map((product) => product.id) : [],
                keyword:
                    blade.parentBlade.filter.keyword || "",
            };
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

        function getParentCategoryId(currentBlade) {
            const parentBlade = currentBlade.parentBlade;
            if (parentBlade) {
                return parentBlade.categoryId ? [parentBlade.categoryId] : getParentCategoryId(parentBlade);
            } else {
                return [];
            }
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
