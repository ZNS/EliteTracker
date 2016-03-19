module.exports = function (grunt) {
    // Project configuration.
    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),
        copy: {
            lib: {
                src: [
                    "bower_components/jquery/dist/jquery.min.js",
                    "bower_components/angular/angular.min.js",
                    "bower_components/moment/min/moment.min.js",
                    "bower_components/chartjs/chart.min.js",
                    "bower_components/angular-chart.js/dist/angular-chart.js",
                    "bower_components/angular-google-chart/ng-google-chart.js",
                    "bower_components/checklist-model/checklist-model.js",
                    "bower_components/easyautocomplete/dist/jquery.easy-autocomplete.min.js"
                    ],
                dest: "../Content/Js/lib/",
                expand: true,
                flatten: true
            },
            css: {
                src: [
                    "bower_components/easyautocomplete/dist/easy-autocomplete.css"
                ],
                dest: "../Content/Css/Plugins",
                expand: true,
                flatten: true
            },
            fontawesome: {
                cwd: "bower_components/fontawesome",
                src: [
                    "css/font-awesome.css",
                    "fonts/*"
                ],
                dest: "../Content/Css/Fontawesome",
                expand: true,
                flatten: false
            },
            boostrap: {
                cwd: "bower_components/bootstrap/less",
                src: [
                    "*.less",
                    "mixins/*"],
                dest: "../Content/Css/Bootstrap/src/",
                expand: true,
                flatten: false
            }
        }
    });

    // Load plugins
    grunt.loadNpmTasks('grunt-contrib-copy');
    // Default task(s).
    grunt.registerTask('default', ['copy']);
};