angular.module('elitetracker')
.controller('commentIndex', ['$scope', '$http', function ($scope, $http) {
    $scope.deleteComment = function(id) {
        if (confirm('Delete comment?'))
        {
            $http.post('/comment/delete/' + id).success(function() {
                window.location.reload();
            });
        }
    };
}]);