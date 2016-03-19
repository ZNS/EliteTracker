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

    $scope.eddb = function () {
        var name = $("input[name='Name']").val();
        $("form")[0].reset();
        $("input[name='Name']").val(name);

        $http.get('/db/getsystem', { params: { name: name }}).success(function (json) {
            if (json && json != null)
            {
                if (json.Population && json.Population != null)
                    $("input[name='Population']").val(json.Population);
                if (json.Security && json.Security != null && json.Security != "") {
                    setSelected('Security', json.Security);
                }
                if (json.Power && json.Power != null && json.Power != "") {
                    setSelected('PowerPlayLeader', json.Power);
                }
                if (json.Power_State && json.Power_State != null && json.Power_State != "") {
                    setSelected('PowerPlayState', json.Power_State);
                }
                if (json.X && json.X != null)
                    $("input[name='Coordinates.X']").val(json.X);
                if (json.Y && json.Y != null)
                    $("input[name='Coordinates.Y']").val(json.Y);
                if (json.Z && json.Z != null)
                    $("input[name='Coordinates.Z']").val(json.Z);
            }
            else {
                showMessage("Couldn't find any system with that name.", 3);
            }
        });
    };

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
        //Validate
        var valid = true;
        var hasMain = false;
        angular.forEach($scope.stations, function (station) {
            if (station.Main == true) {
                if (hasMain) {
                    showMessage("There can only be one main station", 2);
                    valid = false;
                    return false;
                }
                if (station.Faction.Id == 0) {
                    showMessage("Main station can not have undefined faction", 2);
                    valid = false;
                    return false;
                }
                hasMain = true;
            }
        });
        if (valid) {
            doAsyncSeries($scope.stations).then(function () {
                showMessage("Stations saved", 1);
                $scope.isSaving = false;
            });
        }
        else {
            $scope.isSaving = false;
        }
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
        if (station.Name.length > 0) {
            //Convert economies to int array
            station.Economy = [];
            angular.forEach(station.EconomyTags, function (e) {
                station.Economy.push(e.value);
            });
            return $http({
                url: '/solarsystem/savestation/' + $scope.systemId,
                method: 'POST',
                data: station
            });
        }
    }

    function showMessage(msg, status) {
        $scope.msg = msg;
        $scope.msgStatus = status;
        $scope.showMsg = true;
        $timeout(function () {
            $scope.showMsg = false;
        }, 3000);
    }

    function setSelected(selectName, text)
    {
        var val = $("select[name='" + selectName + "']").find("option")
            .filter(function () { return $(this).html().toLowerCase() == text.toLowerCase(); })
            .attr("value");
        $("select[name='" + selectName + "']").val(val);
    }
}]);