angular.module('elitetracker')
.controller('solarSystemView', ['$scope', '$http', '$timeout', function ($scope, $http, $timeout) {
    var now = moment();
    $scope.showMsg = false;
    $scope.statusTo = moment.utc([now.year(), now.month(), now.date()]);
    $scope.statusFrom = moment($scope.statusTo).subtract(1, 'month');
    $scope.historicStatus = [];

    $scope.init = function (id) {
        $scope.currentStatus = {
            Id: 0,
            SolarSystem: id,
            Date: moment.utc([now.year(), now.month(), now.date()]).toISOString(),
            FactionStatus: []
        };

        $http({ method: 'GET', url: '/solarsystem/getstatus/' + id, params: { from: $scope.statusFrom.toISOString(), to: $scope.statusTo.toISOString() } })
        .success(function (data) {
            if (data.length > 0) {
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

            //Create chart data
            $scope.chart = {
                labels: [],
                series: [],
                data: []
            };
            angular.forEach(data, function (status) {
                $scope.chart.labels.push(moment(status.Date).format("l"));
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
                        $scope.chart.data.push([faction.Influence]);
                    }
                    else {
                        $scope.chart.data[index].push(faction.Influence);
                    }
                });
            });
        });
    };

    $scope.addFaction = function () {
        $scope.currentStatus.FactionStatus.push({
            Faction: { Id: 0, Name: '' },
            Influence: 0,
            State: 0,
            PendingStates: []
        });
    };

    $scope.saveCurrentStatus = function () {
        $http.post('/solarsystem/saveStatus/' + $scope.currentStatus.Id, $scope.currentStatus)
        .success(function () {
            $scope.showMessage('Save successful', 1);
        });
    };

    $scope.showMessage = function (msg, status) {
        $scope.msg = msg;
        $scope.msgStatus = status;
        $scope.showMsg = true;
        $timeout(function () { $scope.showMsg = false; }, 3000);
    };
}]);