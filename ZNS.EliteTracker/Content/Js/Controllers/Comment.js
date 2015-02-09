angular.module('elitetracker')
.controller('comment', ['$scope', '$http', function ($scope, $http) {
    $scope.isSaving = false;

    $scope.postComment = function () {
        $scope.isSaving = true;
        var data = {
            DocumentId: $scope.documentId,
            Body: $.trim($('#commentBody').bbcode())
        };
        if (data.Body.length > 0) {
            $http.post('/comment/post', data).success(function () {
                $('#commentBody').bbcode('');
                $scope.isSaving = false;
                window.location.reload();
            });
        }
    };
}]);