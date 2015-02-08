angular.module('elitetracker')
.controller('solarSystemEdit', ['$scope', '$http', '$q', '$timeout', function ($scope, $http, $q, $timeout) {
    $scope.stations = [];
    $scope.economies = null;
    $scope.showMsg = false;
    $scope.isSaving = false;

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
        if (confirm('Delete station?')) {
            var guid = $scope.stations[idx].Guid;
            $scope.stations.splice(idx, 1);
            $http.post('/solarsystem/removestation/' + $scope.systemId, { guid: guid }).success(function () {
                showMessage("Station removed", 1);
            });
        }
    };

    $scope.saveStations = function () {
        $scope.isSaving = true;
        doAsyncSeries($scope.stations).then(function () {
            showMessage("Stations saved", 1);
            $scope.isSaving = false;
        });
    };

    function doAsyncSeries(arr) {
        return arr.reduce(function (promise, item) {
            return promise.then(function() {
                return postStation(item);
            });
        }, $q.when());
    }

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

    function showMessage(msg, status) {
        $scope.msg = msg;
        $scope.msgStatus = status;
        $scope.showMsg = true;
        $timeout(function () {
            $scope.showMsg = false;
        }, 3000);
    }
}]);