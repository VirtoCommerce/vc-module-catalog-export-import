angular.module('virtoCommerce.catalogExportImportModule')
.controller('virtoCommerce.catalogExportImportModule.fileUploadController',
    ['FileUploader', '$document', '$scope', '$timeout', 'platformWebApp.bladeNavigationService', 'platformWebApp.assets.api', '$translate', 'platformWebApp.settings', '$q', 'platformWebApp.dialogService', 'virtoCommerce.catalogExportImportModule.import', 'availableDataTypes', 'editorialReview',
        function (FileUploader, $document, $scope, $timeout, bladeNavigationService, assetsApi,  $translate, settings, $q, dialogService, importResources, availableDataTypes, editorialReview) {
        const blade = $scope.blade;
        const oneKb = 1024;
        const oneMb = 1024 * oneKb;
        $scope.editorialReview = editorialReview;
        $scope.maxCsvSize = oneMb;
        blade.headIcon = 'fas fa-file-alt';
        blade.title = 'catalogExportImport.blades.file-upload.title';
        blade.subtitle = 'catalogExportImport.blades.file-upload.subtitle';
        blade.isLoading = false;
        $scope.newUploadedFile = {};
        $scope.previousUploadedFile = {};

        blade.toolbarCommands = [{
            name: "platform.commands.cancel",
            icon: 'fas fa-ban',
            executeMethod: () => {
                $scope.bladeClose();
            },
            canExecuteMethod: () => true
        }];

        blade.dataType = blade.predefinedDataType;
        blade.availableDataTypes = availableDataTypes;

        function initialize () {
            resetState();

            settings.getValues({ id: 'CatalogExportImport.Import.FileMaxSize' }, (value) => {
                if (!!value) {
                    $scope.maxCsvSize = value[0] * oneMb;
                }
                $scope.formattedSizeLimit = formatFileSize($scope.maxCsvSize);
            });

            let uploader = $scope.uploader = new FileUploader({
                scope: $scope,
                headers: {Accept: 'application/json'},
                url: 'api/assets?folderUrl=tmp',
                method: 'POST',
                autoUpload: false, //We need to set this to false in order to prevent our async operations (delete asset/ validate file) from running in the wrong order
                removeAfterUpload: true,
                filters: [
                    {
                        name: 'onlyCsv',
                        fn: (item) => {
                            $scope.newUploadedFile.name = item.name;
                            if (!uploader.isHTML5) {
                                return true;
                            } else {
                                let result = /^.*\.(csv)$/.test(item.name);
                                $scope.fileTypeError = !result;
                                if ($scope.fileTypeError) {
                                    if (blade.childrenBlades && blade.childrenBlades.length) {
                                        bladeNavigationService.closeChildrenBlades(blade, () => {});
                                    }
                                }
                                return result;
                            }
                        }
                    }, {
                        name: 'csvMaxSize',
                        fn: (item) => {
                            $scope.newUploadedFile.name = item.name;
                            let result = item.size <= $scope.maxCsvSize;
                            $scope.csvMaxSizeError = !result;
                            if (result) {
                                $scope.newUploadedFile.size = formatFileSize(item.size);
                            }
                            if ($scope.csvMaxSizeError) {
                                if (blade.childrenBlades && blade.childrenBlades.length) {
                                    bladeNavigationService.closeChildrenBlades(blade, () => {});
                                }
                            }
                            return result;
                        }
                    }]
            });

            uploader.onWhenAddingFileFailed = () => {
                if ($scope.internalCsvError) {
                    $scope.internalCsvError = false;
                }

                if (blade.csvFilePath) {
                    assetsApi.remove({urls: [blade.csvFilePath]},
                        () => { },
                        (error) => bladeNavigationService.setError('Error ' + error.status, blade)
                    );
                    blade.csvFilePath = null;
                }

                $scope.showUploadResult = true;
            };

            uploader.onAfterAddingFile = (file) => {
                bladeNavigationService.setError(null, blade);

                if (blade.csvFilePath) {
                    $scope.tmpCsvInfo = {};
                    $scope.tmpCsvInfo.name = $scope.newUploadedFile.name;
                    $scope.tmpCsvInfo.size = $scope.newUploadedFile.size;
                    if (blade.childrenBlades && blade.childrenBlades.length) {
                        $scope.showUploadResult = false;
                        $scope.deleteAndCloseBlades(file);
                    } else {
                        removeCsv().then(() => {
                            resetState();
                            file.upload();
                        });
                    }
                } else {
                    file.upload();
                }
            };

            uploader.onSuccessItem = (__, asset) => {
                uploadNewCsv(asset);
                $scope.previousUploadedFile.name = $scope.newUploadedFile.name
                $scope.previousUploadedFile.size = $scope.newUploadedFile.size
            };

            uploader.onErrorItem = (element, response, status) => {
                bladeNavigationService.setError(`${element._file.name} failed: ${response.message ? response.message : status}`, blade);
            };

        }

        $scope.bladeClose = () => {
            if (blade.csvFilePath) {
                bladeNavigationService.showConfirmationIfNeeded(true, true, blade, () => { bladeNavigationService.closeBlade(blade, () => {
                    removeCsv();
                    resetState();
                }); }, () => {}, "catalogExportImport.dialogs.csv-file-delete.title", "catalogExportImport.dialogs.csv-file-delete.subtitle");
            } else {
                bladeNavigationService.closeBlade(blade);
            }
        }

        $scope.browse = () => {
            $timeout(() => $document[0].querySelector('#selectDescriptionCsv').click());
        }

        $scope.deleteUploadedItem = () => {
            bladeNavigationService.showConfirmationIfNeeded(true, true, blade, () => { bladeNavigationService.closeChildrenBlades(blade, () => {
                removeCsv();
                resetState();
            }); }, () => {}, "catalogExportImport.dialogs.csv-file-delete.title", "catalogExportImport.dialogs.csv-file-delete.subtitle");
        }

        $scope.deleteAndCloseBlades = (file) => {
            const dialog = {
                id: "deleteAndCloseConfirmationDialog",
                deletedFileName: $scope.previousUploadedFile.name,
                callback: function (confirm) {
                    if (confirm) {
                        bladeNavigationService.closeChildrenBlades(blade, () => {
                            removeCsv().then(() => {
                                resetState();
                                file.upload();
                                $scope.showUploadResult = true;
                            });
                        });
                    } else {
                        $scope.newUploadedFile.name = $scope.previousUploadedFile.name;
                        $scope.newUploadedFile.size = $scope.previousUploadedFile.size;
                        $scope.showUploadResult = true;
                    }
                }
            };
            dialogService.showDialog(
                dialog,
                "Modules/$(VirtoCommerce.CatalogExportImport)/Scripts/dialogs/descriptionDeleteAndClose-dialog.tpl.html",
                "platformWebApp.confirmDialogController",
                null,
                false,
                false
            );
        };

        $scope.translateErrorCode = (error) => {
            var translateKey = 'catalogExportImport.validation-errors.' + error.errorCode;
            var result = $translate.instant(translateKey, error.properties);
            return result === translateKey ? errorCode : result;
        }

        $scope.showPreview = () => {
            var newBlade = {
                id: "descriptionsImportPreview",
                catalogId: blade.catalog.id,
                csvFilePath: blade.csvFilePath,
                dataType: blade.dataType.value,
                headIcon: "fas fa-file-csv",
                title: "catalogExportImport.blades.import-preview.title",
                subtitle: 'catalogExportImport.blades.import-preview.subtitle',
                controller: "virtoCommerce.catalogExportImportModule.importPreviewController",
                template: "Modules/$(VirtoCommerce.CatalogExportImport)/Scripts/blades/import-preview.tpl.html",
            };

            bladeNavigationService.showBlade(newBlade, blade);
        }

        function uploadNewCsv(asset) {
            blade.csvFilePath = asset[0].relativeUrl;

            if (!_.isEmpty($scope.tmpCsvInfo)) {
                $scope.newUploadedFile.name = $scope.tmpCsvInfo.name;
                $scope.newUploadedFile.size = $scope.tmpCsvInfo.size;
                $scope.tmpCsvInfo = {};
            }

            importResources.validate({ dataType: blade.dataType.value, filePath: blade.csvFilePath }, (data) => {
                $scope.csvValidationErrors = data.errors;
                $scope.internalCsvError = !!$scope.csvValidationErrors.length;
                $scope.showUploadResult = true;
            }, (error) => { bladeNavigationService.setError('Error ' + error.status, blade); });
        }

        function removeCsv() {
            const deferred = $q.defer();
            assetsApi.remove({urls: [blade.csvFilePath]},
                () => {
                    return deferred.resolve();
                },
                (error) => {
                    bladeNavigationService.setError('Error ' + error.status, blade);
                    return deferred.reject();
                }
            );
            return deferred.promise;
        }

        function resetState() {
            $scope.newUploadedFile = {};
            $scope.previousUploadedFile = {};
            blade.csvFilePath = null;

            $scope.showUploadResult = false;
            $scope.fileTypeError = false;
            $scope.csvMaxSizeError = false;
            $scope.internalCsvError = false;
        }


        function formatFileSize(bytes, decimals = 2) {
            if (bytes === 0) return '0 Bytes';

            const kilobyte = 1024;
            const dm = decimals < 0 ? 0 : decimals;
            const sizes = ['Bytes', 'KB', 'MB'];

            const i = Math.floor(Math.log(bytes) / Math.log(kilobyte));

            return parseFloat((bytes / Math.pow(kilobyte, i)).toFixed(dm)) + ' ' + sizes[i];
        }

        initialize();
    }]);
