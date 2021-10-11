angular.module('virtoCommerce.catalogExportImportModule')
.controller('virtoCommerce.catalogExportImportModule.exportProcessingController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.settings', '$q', 'platformWebApp.dialogService', 'virtoCommerce.catalogExportImportModule.export', 'editorialReview',
    function ($scope, bladeNavigationService, settings, $q, dialogService, exportResources, editorialReview) {
        var blade = $scope.blade;
        const dataType = blade.dataType;
        blade.title = 'catalogExportImport.blades.export-processing.title';
        blade.headIcon = "fa fa-download";
        blade.isLoading = true;
        $scope.editorialReview = editorialReview;

        const isSelectedAll = getIsSelectedAll();

        function initialize() {
            const exportDataRequest = getExportRequest();
            const getTotalCountPromise = exportResources.count(exportDataRequest).$promise;
            const getExportLimitsPromise = settings.getValues({ id: 'CatalogExportImport.Export.LimitOfLines' }).$promise;

            $q.all([getTotalCountPromise, getExportLimitsPromise]).then(([totalCountResponse, exportLimitResponse]) => {
                const totalCount = totalCountResponse.totalCount;
                const exportLimit = exportLimitResponse[0];
                if (totalCount > exportLimit) {
                    $scope.bladeClose();
                    showWarningDialog(totalCount, exportLimit);
                } else {
                    showConfirmDialog(exportDataRequest, totalCount);
                }
            });
        }

        function getExportRequest() {
            return {
                dataType,
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
                id: 'exportCatalogConfirmDialog',
                itemsQty,
                dataType,
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
            dialogService.showDialog(dialog, 'Modules/$(VirtoCommerce.CatalogExportImport)/Scripts/dialogs/exportConfirm-dialog.tpl.html', 'platformWebApp.confirmDialogController');
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
