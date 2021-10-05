angular.module('virtoCommerce.catalogExportImportModule')
.controller('virtoCommerce.catalogExportImportModule.exportProcessingController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.settings', '$q', 'platformWebApp.dialogService', 'virtoCommerce.catalogExportImportModule.export',
    function ($scope, bladeNavigationService, settings, $q, dialogService, exportResources) {
        var blade = $scope.blade;
        blade.title = 'catalogExportImport.blades.export-processing.title';
        blade.headIcon = "fa fa-download";
        blade.isLoading = true;

        const isSelectedAll = getIsSelectedAll();

        function initialize() {
            const exportDataRequest = getExportRequest();
            const getTotalCountPromise = exportResources.count(exportDataRequest).$promise;
            const getExportLimitsPromise = settings.getValues({ id: 'CatalogExportImport.Export.LimitOfLines' }).$promise;

            $q.all([getTotalCountPromise, getExportLimitsPromise]).then(([totalCountResponse, exportLimitResponse]) => {
                const descriptionsTotalCount = totalCountResponse.totalCount;
                const exportLimit = exportLimitResponse[0];
                if (descriptionsTotalCount > exportLimit) {
                    $scope.bladeClose();
                    showWarningDialog(descriptionsTotalCount, exportLimit);
                } else {
                    showConfirmDialog(exportDataRequest, descriptionsTotalCount);
                }
            });
        }

        function getExportRequest() {
            return {
                catalogId:
                    blade.catalog.id,
                categoryId: getParentCategoryId(blade),
                categoryIds:
                    !isSelectedAll
                        ? blade.selectedCategories.map((category) => category.id)
                        : [],
                itemIds:
                    !isSelectedAll ? blade.selectedProducts.map((product) => product.id) : [],
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
                return parentBlade.categoryId ? parentBlade.categoryId : getParentCategoryId(parentBlade);
            } else {
                return null;
            }
        }

        function showWarningDialog(itemsQty, limitQty) {
            const dialog = {
                id: 'exportLimitWarningDialog',
                itemsQty,
                limitQty
            };
            dialogService.showDialog(dialog, 'Modules/$(VirtoCommerce.CatalogExportImport)/Scripts/dialogs/exportLimitWarning-dialog.tpl.html', 'platformWebApp.confirmDialogController');
        }

        function showConfirmDialog(exportDataRequest, itemsQty) {

            const dialog = {
                id: 'exportDescriptionConfirmDialog',
                itemsQty,
                exportAll: isSelectedAll,
                callback: function(confirm) {
                    if (confirm) {
                        exportResources.run(exportDataRequest,
                            (data) => {
                                blade.notification = data;
                                blade.isLoading = false;
                            });
                    } else {
                        bladeNavigationService.closeBlade(blade);
                    }

                }
            };
            dialogService.showDialog(dialog, 'Modules/$(VirtoCommerce.CatalogExportImport)/Scripts/dialogs/export-descriptions-confirm-dialog.tpl.html', 'platformWebApp.confirmDialogController');
        }

        function getIsSelectedAll() {
            const itemsBlade = blade.parentBlade;

            console.log(itemsBlade);

            const selectAllState = itemsBlade.$scope.gridApi.selection.getSelectAllState();
            const result = selectAllState || (blade.selectedCategories.length === 0 && blade.selectedProducts.length === 0);

            console.log(selectAllState);
            console.log(result);

            return result;
        }

        initialize();
    }]);
