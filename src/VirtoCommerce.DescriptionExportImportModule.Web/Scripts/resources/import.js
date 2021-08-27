angular.module("virtoCommerce.descriptionExportImportModule")
    .factory("virtoCommerce.descriptionExportImportModule.import", ["$resource", function ($resource) {
        return $resource("api/description/import", null, {
            validate: { method: "POST", url: "api/description/import/validate" },
            run: { method: "POST", url: "api/description/import/run" },
            cancel: { method: "POST", url: "api/description/import/cancel" },
        });
    },
]);
