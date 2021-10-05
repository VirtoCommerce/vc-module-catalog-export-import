angular.module("virtoCommerce.catalogExportImportModule")
    .factory("virtoCommerce.catalogExportImportModule.import", ["$resource", function ($resource) {
        return $resource("api/catalog/import", null, {
            validate: { method: "POST", url: "api/catalog/import/validate" },
            preview: { method: "POST", url: "api/catalog/import/preview" },
            run: { method: "POST", url: "api/catalog/import/run" },
            cancel: { method: "POST", url: "api/catalog/import/cancel" },
        });
    },
]);
