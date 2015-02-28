angular.module('elitetracker')
.directive('timeAgo', [function ($interval) {
    return {
        restrict: 'A',
        link: function (scope, $element) {
            if ($element.text().length > 0) {
                $element.text(moment.utc($element.text()).fromNow());
            }
        }
    };
}]);