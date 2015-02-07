angular.module('elitetracker')
.controller('comment', ['$scope', '$http', function ($scope, $http) {
    $scope.postComment = function () {
        var data = {
            DocumentId: $scope.documentId,
            Body: $('#commentBody').bbcode()
        };
        $http.post('/comment/post', data).success(function () {
            $('#commentBody').bbcode('');
            window.location.reload();
        });
    };
}]);