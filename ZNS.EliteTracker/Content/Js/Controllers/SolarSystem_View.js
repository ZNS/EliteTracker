angular.module('elitetracker')
.controller('solarSystemView', ['$scope', '$http', '$timeout', function ($scope, $http, $timeout) {
    var now = moment();
    $scope.showMsg = false;
    $scope.statusTo = moment.utc([now.year(), now.month(), now.date()]);
    $scope.statusFrom = moment($scope.statusTo).subtract(1, 'month');
    $scope.statuses = [];
    $scope.isSaving = false;

    $scope.init = function (id) {
        $scope.systemId = id;

        $scope.currentStatus = {
            Id: 0,
            SolarSystem: id,
            Date: moment.utc([now.year(), now.month(), now.date()]).toISOString(),
            FactionStatus: []
        };

        $http({ method: 'GET', url: '/solarsystem/getstatus/' + id, params: { from: $scope.statusFrom.toISOString(), to: $scope.statusTo.toISOString() } })
        .success(function (data) {
            if (data.length > 0) {
                $scope.statuses = data;
                $scope.currentStatus = data[data.length - 1];

                //If the last status is not for the current day use it as a new status template
                if (!moment($scope.currentStatus.Date).isSame(moment.utc(), 'day')) {
                    $scope.currentStatus.Id = 0;
                    $scope.currentStatus.Date = moment.utc([now.year(), now.month(), now.date()]).toISOString();
                    angular.forEach($scope.currentStatus.FactionStatus, function (fs) {
                        fs.Influence = 0;
                    });
                }
            }

            //Track current date
            $scope.currentStatusDate = moment($scope.currentStatus.Date);

            //Create chart data
            $scope.chart = {
                labels: [],
                series: [],
                data: []
            };
            var labelIdx = -1;
            angular.forEach(data, function (status) {
                $scope.chart.labels.push(moment(status.Date).format("l"));
                labelIdx += 1;
                angular.forEach(status.FactionStatus, function (faction) {
                    //Find correct series index
                    var index = -1;
                    angular.forEach($scope.chart.series, function (ds, idx) {
                        if (ds == faction.Faction.Name) {
                            index = idx;
                            return false;
                        }
                    });
                    //Add data
                    if (index == -1) {
                        $scope.chart.series.push(faction.Faction.Name);
                        if (labelIdx > 0) {
                            $scope.chart.data.push([]);
                            //Pad
                            for (var i = 0; i < labelIdx; i++) {
                                $scope.chart.data[$scope.chart.data.length - 1].push(0);
                            }
                            $scope.chart.data[$scope.chart.data.length - 1].push(faction.Influence);
                        }
                        else {
                            $scope.chart.data.push([faction.Influence]);
                        }
                    }
                    else {
                        $scope.chart.data[index].push(faction.Influence);
                    }
                });
            });
        });
    };

    $scope.prevStatus = function () {
        $scope.currentStatusDate = $scope.currentStatusDate.subtract(1, 'days');
        updateCurrentStatus();
    };

    $scope.nextStatus = function () {
        if (!$scope.currentStatusDate.isSame(moment.utc(), 'day')) {
            $scope.currentStatusDate = $scope.currentStatusDate.add(1, 'days');
            updateCurrentStatus();
        }
    };

    function updateCurrentStatus()
    {
        var exists = false;
        angular.forEach($scope.statuses, function (status) {
            if (moment(status.Date).isSame($scope.currentStatusDate, 'day'))
            {
                $scope.currentStatus = status;
                exists = true;
                return false;
            }
        });

        if (!exists) {
            var clone = $.extend(true, {}, $scope.currentStatus);
            clone.Id = 0;
            clone.Date = $scope.currentStatusDate.toISOString();
            angular.forEach(clone.FactionStatus, function (fs) {
                fs.Influence = 0;
            });
            $scope.statuses.push(clone);
            $scope.statuses.sort(function (a, b) {
                return (a.Date < b.Date) ? -1 : 1;
            });
            $scope.currentStatus = clone;
        }
    }

    $scope.addFaction = function () {
        $scope.currentStatus.FactionStatus.push({
            Faction: { Id: 0, Name: '' },
            Influence: 0,
            State: 0,
            PendingStates: []
        });
    };

    $scope.removeFaction = function (id) {
        var index = -1;
        angular.forEach($scope.currentStatus.FactionStatus, function (faction, idx) {
            if (faction.Faction.Id == id) {
                index = idx;
                return false;
            }
        });
        if (index > -1) {
            $scope.currentStatus.FactionStatus.splice(index, 1);
        }
    };

    $scope.saveCurrentStatus = function () {
        $scope.isSaving = true;
        $http.post('/solarsystem/saveStatus/' + $scope.currentStatus.Id, $scope.currentStatus)
        .success(function (response) {
            angular.forEach($scope.statuses, function (status) {
                if (moment(status.Date).isSame(moment(response.date), 'day'))
                {
                    status.Id = response.id;
                    return false;
                }
            });
            $scope.showMessage('Save successful', 1);
            $scope.isSaving = false;
        });
    };

    $scope.addActiveCommander = function() {
        $http.post('/solarsystem/addactivecommander/' + $scope.systemId).success(function () {
            window.location.reload();
        });
    };

    $scope.removeActiveCommander = function () {
        $http.post('/solarsystem/removeactivecommander/' + $scope.systemId).success(function () {
            window.location.reload();
        });
    };

    $scope.showMessage = function (msg, status) {
        $scope.msg = msg;
        $scope.msgStatus = status;
        $scope.showMsg = true;
        $timeout(function () { $scope.showMsg = false; }, 3000);
    };
}]);