// Call this to register your module to main application
var moduleName = "virtoCommerce.descriptionExportImport";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .config(['$stateProvider',
        function ($stateProvider) {
            $stateProvider
                .state('workspace.virtoCommerceDescriptionExportImportState', {
                    url: '/virtoCommerce.descriptionExportImport',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        'platformWebApp.bladeNavigationService', function (bladeNavigationService) {
                            var newBlade = {
                                id: 'blade1',
                                controller: 'virtoCommerce.descriptionExportImport.helloWorldController',
                                template: 'Modules/$(virtoCommerce.descriptionExportImport)/Scripts/blades/hello-world.html',
                                isClosingDisabled: true
                            };
                            bladeNavigationService.showBlade(newBlade);
                        }
                    ]
                });
        }
    ])

    .run(['platformWebApp.mainMenuService', 'platformWebApp.widgetService', '$state',
        function (mainMenuService, widgetService, $state) {
            //Register module in main menu
            var menuItem = {
                path: 'browse/virtoCommerce.descriptionExportImport',
                icon: 'fa fa-cube',
                title: 'VirtoCommerce.DescriptionExportImportModule',
                priority: 100,
                action: function () { $state.go('workspace.virtoCommerceDescriptionExportImportState'); },
                permission: 'virtoCommerceDescriptionExportImport:access'
            };
            mainMenuService.addMenuItem(menuItem);
        }
    ]);
