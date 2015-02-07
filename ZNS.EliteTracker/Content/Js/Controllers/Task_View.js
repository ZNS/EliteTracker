angular.module('elitetracker')
.controller('taskView', ['$scope', '$http', function ($scope, $http) {

    $scope.signUp = function (taskId) {
        $http.post('/task/signup/' + taskId).success(function () {
            window.location.reload();
        });
    };

    $scope.withdraw = function (taskId) {
        $http.post('/task/withdraw/' + taskId).success(function () {
            window.location.reload();
        });
    };

}]);