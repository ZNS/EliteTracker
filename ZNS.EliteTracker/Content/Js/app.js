angular.module('elitetracker', ['ngTagsInput', 'googlechart', 'checklist-model'])
.value('googleChartApiConfig', {
    version: '1',
    optionalSettings: {
        packages: ['corechart', 'timeline']
    }
})
.run(['$rootScope', function ($rootScope) {
    $rootScope.goTo = function (url) {
        window.location = url;
    };
}]);