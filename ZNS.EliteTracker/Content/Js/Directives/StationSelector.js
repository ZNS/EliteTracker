angular.module('elitetracker')
.directive('stationSelector', ['$http', '$timeout', function ($http, $timeout) {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, $element, attrs, ctrl) {
            var $systemSelect = $element.siblings("select").eq(0);

            $element.bind('change', function () {
                scope.$eval(function () {
                    ctrl.$setViewValue($element.val());
                });
            });

            $systemSelect.bind('change', function () {
                updateStations();
            });

            function updateStations(val) {
                var id = $systemSelect.val();
                $http({ method: 'GET', url: '/solarsystem/getstations/' + id })
                .success(function (data) {
                    var options = [];
                    angular.forEach(data, function (station) {
                        options.push('<option value="' + station.Guid + '">' + station.Name + '</option>')
                    });
                    $element.html(options);

                    if (!val) {
                        ctrl.$setViewValue($element.val());
                    }
                    else {
                        $element.val(val);
                    }
                });
            }

            $timeout(function () { updateStations(ctrl.$viewValue); }, 250);
        }
    };
}]);