module.exports = function (grunt) {
    // Project configuration.
    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),
        copy: {
            lib: {
                src: [
                    "bower_components/jquery/dist/jquery.js",
                    "bower_components/angular/angular.js",
                    "bower_components/moment/moment.js",
                    "bower_components/chartjs/chart.js",
                    "bower_components/angular-chart.js/angular-chart.js"
                    ],
                dest: "../Content/Js/lib/",
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