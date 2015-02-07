angular.module('elitetracker')
.directive('clock', ['$interval', function ($interval) {
    return {
        restrict: 'CA',
        link: function (scope, $element) {
            function update() {
                $element.text(moment.utc().add(1286, 'year').format('MMM D, YYYY HH:mm'));
            }
            $interval(update, 1000);
            update();
        }
    };
}]);