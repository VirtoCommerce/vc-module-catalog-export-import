angular.module('virtoCommerce.descriptionExportImportModule')
    .controller('virtoCommerce.descriptionExportImportModule.importPreviewController', ['$scope', 'virtoCommerce.descriptionExportImportModule.import', 'platformWebApp.bladeNavigationService', 'uiGridConstants', 'platformWebApp.bladeUtils', 'platformWebApp.dialogService', function ($scope, importResources, bladeNavigationService, uiGridConstants, bladeUtils, dialogService) {
        $scope.uiGridConstants = uiGridConstants;

        var blade = $scope.blade;

        blade.importPermission = "product:description:import";

        function initialize() {
            blade.isLoading = true;
            $scope.showUnparsedRowsWarning = false;

            importResources.preview({ filePath: blade.csvFilePath, dataType: blade.dataType }, (response) => {
                const records = response.results;

                $scope.originalRecords = _.map(records, record => ({...record}));

                _.each(records, record => {
                    if (record.descriptionId) {
                        record.descriptionId = truncateId(record.descriptionId);
                    }
                    record.descriptionContent = truncateContent(record.descriptionContent);
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
                            id: "descriptionsImportProcessing",
                            notification: data,
                            headIcon: "fa fa-download",
                            title: "descriptionExportImport.blades.import-processing.title",
                            controller: "virtoCommerce.descriptionExportImportModule.importProcessingController",
                            template: "Modules/$(VirtoCommerce.DescriptionExportImport)/Scripts/blades/import-processing.tpl.html",
                        };

                        bladeNavigationService.showBlade(newBlade, blade);
                    });
                },
                permission: blade.importPermission
            },
            {
                name: "priceExportImport.blades.import-preview.upload-new",
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
