angular.module('elitetracker')
.controller('commanderEdit', ['$scope', '$http', '$q', '$timeout', function ($scope, $http, $q, $timeout) {
    $scope.ships = [];
    $scope.showMsg = false;
    $scope.isSaving = false;

    $scope.init = function (id) {
        $scope.commanderId = id;
        $http({ method: 'GET', url: '/commander/getships/' + $scope.commanderId })
        .success(function (data) {
            $scope.ships = data;
        });
    };

    $scope.addShip = function () {
        $scope.ships.push({ Guid: '', Name: '', Model: 1, Fitting: 1 });
    };

    $scope.removeShip = function (idx) {
        if (confirm('Remove ship?')) {
            var guid = $scope.ships[idx].Guid;
            $scope.ships.splice(idx, 1);
            $http.post('/commander/removeship/' + $scope.commanderId, { guid: guid }).success(function () {
                showMessage("Ship removed", 1);
            });
        }
    };

    $scope.saveShips = function () {
        $scope.isSaving = true;
        doAsyncSeries($scope.ships).then(function () {
            showMessage("Ships saved", 1);
            $scope.isSaving = false;
        });
    };

    function doAsyncSeries(arr) {
        return arr.reduce(function (promise, item) {
            return promise.then(function () {
                return postShip(item);
            });
        }, $q.when());
    }

    function postShip(ship)
    {
        return $http({
            url: '/commander/saveship/' + $scope.commanderId,
            method: 'POST',
            data: ship});
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