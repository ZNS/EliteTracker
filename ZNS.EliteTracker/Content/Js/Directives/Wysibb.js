angular.module('elitetracker')
.directive('wysibb', function () {
    return {
        restrict: 'A',
        link: function (scope, $element, attrs) {
            var buttons = "bold,italic,underline,strike,|,fontsize,|,img,video,link,|,quote";
            if (attrs.wysibbEditor && attrs.wysibbEditor == "1") {
                buttons = "bold,italic,underline,strike,|,fontsize,|,bullist,numlist,|,img,video,link,|,quote";
            }
            $element.wysibb({
                buttons: buttons
            });
        }
    };
});