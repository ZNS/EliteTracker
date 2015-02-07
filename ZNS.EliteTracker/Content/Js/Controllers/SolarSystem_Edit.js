angular.module('elitetracker')
.controller('solarSystemEdit', ['$scope', '$http', '$q', function ($scope, $http, $q) {
    $scope.stations = [];
    $scope.economies = null;

    //Load stations
    $scope.init = function (id) {
        $scope.systemId = id;
        $http({ method: 'GET', url: '/solarsystem/getstations/' + $scope.systemId })
        .success(function (data) {
            $scope.stations = data;
            //Fix economy
            $scope.loadEconomies().then(function (result) {
                angular.forEach($scope.stations, function (s) {
                    s.EconomyTags = [];
                    angular.forEach(s.Economy, function (i) {
                        angular.forEach(result.data, function (tag) {
                            if (i == tag.value) {
                                s.EconomyTags.push(tag);
                                return false;
                            }
                        });
                    });
                });
            });
        });
    }

    $scope.loadEconomies = function () {
        var deferred = $q.defer();
        if ($scope.economies == null) {
            return $http({ method: 'GET', url: '/solarsystem/getstationeconomies' }).success(function (result) {
                $scope.economies = result;
            });
        }
        else {
            deferred.resolve($scope.economies);
        }
        return deferred.promise;
    };

    $scope.addStation = function () {
        $scope.stations.push({ Guid: '', Name: '', Economy: [], EconomyTags: [], Type: 1, Main: false, Faction: { Id: 0, Name: '', Ally: false}});
    };

    $scope.removeStation = function (idx) {
        $scope.stations.splice(idx, 1);
    };

    $scope.saveStations = function () {
        //Sequential saving
        var previous = $q.when(null)
        for (var i = 0; i < $scope.stations.length; i++) {
            (function (i) {
                previous = previous.then(function () {
                    return postStation($scope.stations[i]);
                });
            }(i))
        }
    };

    function postStation(station)
    {
        //Convert economies to int array
        station.Economy = [];
        angular.forEach(station.EconomyTags, function (e) {
            station.Economy.push(e.value);
        });
        return $http({
            url: '/solarsystem/savestation/' + $scope.systemId,
            method: 'POST',
            data: station});
    }
}]);