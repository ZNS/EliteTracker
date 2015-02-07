angular.module('elitetracker')
.directive('wysibb', function () {
    return {
        restrict: 'A',
        link: function (scope, $element) {
            $element.wysibb({
                buttons: "bold,italic,underline,strike,|,fontsize,|,img,video,link,|,quote"
            });
        }
    };
});