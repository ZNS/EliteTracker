angular.module('elitetracker')
.directive('timeAgo', [function ($interval) {
    return {
        restrict: 'A',
        link: function (scope, $element) {
            $element.text(moment.utc($element.text()).fromNow());
        }
    };
}]);