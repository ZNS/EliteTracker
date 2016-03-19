angular.module('elitetracker')
.controller('mapView', ['$scope', function ($scope) {
    Ed3d.init({
        basePath: '/content/ed3d/',
        container: 'edmap',
        jsonPath: "/solarsystem/getmapdata",
        withHudPanel: true,
        hudMultipleSelect: true,
        effectScaleSystem: [28, 1000],
        startAnim: false,
        playerPos: [-8, -19, 166]
    });

    $scope.toggleFullscreen = function () {
        $("#edmap").toggleClass("fullscreen");
        Ed3d.rebuild();
    };
}]);