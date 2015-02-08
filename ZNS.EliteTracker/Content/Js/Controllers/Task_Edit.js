angular.module('elitetracker')
.controller('taskEdit', ['$scope', '$http', function ($scope, $http) {
    $scope.deleteTask = function (id) {
        if (confirm('Delete task?')) {
            $http.post('/task/delete/' + id).success(function () {
                window.location = "/task";
            });
        }
    };
}]);