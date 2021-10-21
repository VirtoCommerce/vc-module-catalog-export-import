angular.module('virtoCommerce.catalogExportImportModule')
    .controller('virtoCommerce.catalogExportImportModule.importPreviewController', ['$scope', 'virtoCommerce.catalogExportImportModule.import', 'platformWebApp.bladeNavigationService', 'uiGridConstants', 'platformWebApp.bladeUtils', 'platformWebApp.dialogService', 'editorialReview', function ($scope, importResources, bladeNavigationService, uiGridConstants, bladeUtils, dialogService, editorialReview) {
        $scope.uiGridConstants = uiGridConstants;
        $scope.editorialReview = editorialReview;

        var blade = $scope.blade;

        blade.importPermission = "product:catalog:import";

        function initialize() {
            blade.isLoading = true;
            $scope.showUnparsedRowsWarning = false;

            importResources.preview({ filePath: blade.csvFilePath, dataType: blade.dataType }, (response) => {
                const records = response.results;

                $scope.originalRecords = _.map(records, record => ({...record}));

                _.each(records, record => {
                    if (blade.dataType !== $scope.editorialReview) {
                        if (record.productId) {
                            record.productId = truncateId(record.productId);
                        }
                        if (record.productOuterId) {
                            record.productOuterId = truncateId(record.productOuterId);
                        }
                        if (record.categoryId) {
                            record.categoryId = truncateId(record.categoryId);
                        }
                        if (record.categoryOuterId) {
                            record.categoryOuterId = truncateId(record.categoryOuterId);
                        }
                    } else {
                        if (record.descriptionId) {
                            record.descriptionId = truncateId(record.descriptionId);
                        }
                        record.descriptionContent = truncateContent(record.descriptionContent);
                    }
                });

                blade.currentEntities = records;
                blade.totalCount = response.totalCount;
                $scope.pageSettings.totalItems = 10;
                getInvalidRowsCount();
                blade.isLoading = false;
            }, (error) => { bladeNavigationService.setError('Error ' + error.status, blade); });
        }

        blade.toolbarCommands = [
            {
                name: "platform.commands.import",
                icon: "fa fa-download",
                canExecuteMethod: () => true,
                executeMethod: () => {
                    const importDataRequest = {
                        filePath: blade.csvFilePath,
                        dataType: blade.dataType,
                    };
                    importResources.run(importDataRequest, (data) => {
                        var newBlade = {
                            id: "importProcessing",
                            notification: data,
                            headIcon: "fa fa-download",
                            title: "catalogExportImport.blades.import-processing.title",
                            controller: "virtoCommerce.catalogExportImportModule.importProcessingController",
                            template: "Modules/$(VirtoCommerce.CatalogExportImport)/Scripts/blades/import-processing.tpl.html",
                        };

                        bladeNavigationService.showBlade(newBlade, blade);
                    });
                },
                permission: blade.importPermission
            },
            {
                name: "catalogExportImport.blades.import-preview.upload-new",
                icon: 'fas fa-plus',
                canExecuteMethod: () => true,
                executeMethod: () => {
                    bladeNavigationService.closeBlade(blade);
                }
            }
        ];

        //ui-grid
        $scope.setGridOptions = (gridOptions) => {
            $scope.gridOptions = gridOptions;
            bladeUtils.initializePagination($scope);

            gridOptions.onRegisterApi = function (gridApi) {
                gridApi.grid.registerDataChangeCallback((grid) => {
                    grid.buildColumns();
                    _.each(gridApi.grid.options.columnDefs, column => {
                        column.cellTooltip = getCellTooltip;
                        column.headerCellClass = 'br-0 font-weight-500 fs-13';
                    });

                    if (blade.dataType !== $scope.editorialReview) {
                        const productNameColumn = _.findWhere(gridApi.grid.options.columnDefs, {name: 'productName'});
                        const productSkuColumn = _.findWhere(gridApi.grid.options.columnDefs, {name: 'productSku'});
                        productNameColumn.pinnedLeft = true;
                        productNameColumn.cellClass = 'bl-0 font-weight-500'
                        productSkuColumn.enablePinning = true;
                        productSkuColumn.hidePinLeft = false;
                    }

                    grid.api.core.notifyDataChange(uiGridConstants.dataChange.COLUMN);
                },[uiGridConstants.dataChange.ROW])
            };
        };

        function getInvalidRowsCount() {
            $scope.previewCount = _.min([blade.totalCount, $scope.pageSettings.totalItems]);

            if (blade.currentEntities.length < $scope.previewCount) {
                $scope.unparsedRowsCount = $scope.previewCount - blade.currentEntities.length;
                $scope.showUnparsedRowsWarning = true;
            }
        }

        function truncateContent(content) {
            if (content === null) return "";

            if (content.length > 100) {
                return content.substr(0, 99) + '...';
            }

            return content;
        }

        function truncateId(content) {
            if (content === null) return "";

            if (content.length > 9) {
                return content.substr(0, 3) + '...' + content.substr(content.length - 3, content.length);
            }

            return content;
        }

        function getCellTooltip(row, col) {
            const index = blade.currentEntities.indexOf(row.entity);
            return $scope.originalRecords[index][col.name];
        }

        initialize();

    }]);
