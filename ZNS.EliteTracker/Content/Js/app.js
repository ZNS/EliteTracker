angular.module('elitetracker', ['chart.js', 'ngTagsInput'])
.run(['$rootScope', function ($rootScope) {
    $rootScope.goTo = function (url) {
        window.location = url;
    };
}]);