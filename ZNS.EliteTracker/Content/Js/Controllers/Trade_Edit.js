angular.module('elitetracker')
.controller('tradeEdit', ['$scope', '$http', '$timeout', '$q', function ($scope, $http, $timeout, $q) {
    $scope.routeId = 0;
    $scope.isSaving = false;
    $scope.showMsg = false;
    $scope.milestones = [];

    $scope.init = function (id) {
        $scope.routeId = id;
        if ($scope.routeId != 0)
        {
            $http({ method: 'GET', url: '/trade/getmilestones/' + $scope.routeId })
            .success(function (data) {
                $scope.milestones = data;
            });
        }
    };

    $scope.deleteRoute = function () {
        if (confirm('Delete entire trade route?'))
        {
            $http({ method: 'POST', url: '/trade/delete/' + $scope.routeId })
            .success(function () {
                window.location.href = "/trade";
            });
        }
    };

    $scope.addMilestone = function () {
        $scope.milestones.push({
            From: { SolarSystem: { Id: 0, Name: '' }, StationGuid: '' },
            To: { SolarSystem: { Id: 0, Name: '' }, StationGuid: '' },
            Commodity: 1
        });
    };

    $scope.saveMilestones = function () {
        $scope.isSaving = true;
        console.log($scope.milestones);
        //Validate
        var valid = true;
        angular.forEach($scope.milestones, function (ms) {
            if (ms.From.StationGuid == '' || ms.To.StationGuid == '' || ms.Commodity <= 0) {
                showMessage("All routes must have stations and commodities", 2);
                valid = false;
                return false;
            }
        });
        if (valid) {
            //Serial saving
            doAsyncSeries($scope.milestones).then(function () {
                showMessage("Routes saved", 1);
                $scope.isSaving = false;
            });
        }
    };

    function doAsyncSeries(arr) {
        return arr.reduce(function (promise, item) {
            return promise.then(function () {
                return postMilestone(item);
            });
        }, $q.when());
    }

    function postMilestone(milestone) {
        return $http({
            url: '/trade/savemilestone/' + $scope.routeId,
            method: 'POST',
            data: milestone
        });
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