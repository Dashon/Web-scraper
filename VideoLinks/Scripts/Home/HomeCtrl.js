'use strict';

var app = angular.module('videoLinks');

app.controller('HomeCtrl', ['$scope', 'AppService', '$window', '$q', '$timeout',
    function($scope, AppService, $window, $q, $timeout) {
        $scope.Videos = AppService.Videos();
    }]);
