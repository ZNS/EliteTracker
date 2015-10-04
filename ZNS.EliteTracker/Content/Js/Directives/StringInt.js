angular.module('elitetracker')
.directive('stringInt', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, element, attr, ngModel) {

            ngModel.$formatters.push(function (value) {
                return value.toString();
            });

            ngModel.$parsers.push(function (value) {
                if (value == null)
                    return null;
                return parseInt(value);
            });

        }
    };
})
.directive('stringIntArray', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, element, attr, ngModel) {

            ngModel.$formatters.push(function (value) {
                angular.forEach(value, function (val) {
                    val = val.toString();
                });
            });

            ngModel.$parsers.push(function (value) {
                angular.forEach(value, function (val) {
                    val = parseInt(val);
                });
            });

        }
    };
});