angular.module('elitetracker')
.controller('comment', ['$scope', '$http', function ($scope, $http) {
    $scope.isSaving = false;
    var editCommentId = 0;

    $scope.postComment = function () {
        $scope.isSaving = true;
        var data = {
            DocumentId: $scope.documentId,
            Body: $.trim($('#commentBody').bbcode())
        };
        if (data.Body.length > 0) {
            var url = '/comment/post';
            if (editCommentId != 0)
                url = '/comment/post/' + editCommentId;
            $http.post(url, data).success(function () {
                $('#commentBody').bbcode('');
                $scope.isSaving = false;
                editCommentId = 0;
                window.location.reload(true);
            });
        }
    };

    $scope.edit = function (id) {
        editCommentId = id;
        var $comment = $("#comment_" + id);
        $http.get('/comment/get/' + id).success(function (json) {
            $('#commentBody').bbcode(json.Body);
        });
    };
}]);