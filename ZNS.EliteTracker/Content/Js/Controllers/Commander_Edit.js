angular.module('elitetracker')
.controller('commanderEdit', ['$scope', '$http', '$q', function ($scope, $http, $q) {
    $scope.ships = [];

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
        $scope.ships.splice(idx, 1);
    };

    $scope.saveShips = function () {
        //Sequential saving
        var previous = $q.when(null)
        for (var i = 0; i < $scope.ships.length; i++) {
            (function (i) {
                previous = previous.then(function () {
                    return postShip($scope.ships[i]);
                });
            }(i));
        }
    };

    function postShip(ship)
    {
        return $http({
            url: '/commander/saveship/' + $scope.commanderId,
            method: 'POST',
            data: ship});
    }
}]);