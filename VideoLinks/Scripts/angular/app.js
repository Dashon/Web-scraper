'use strict';

var app = angular.module('videoLinks', []);


//app.config(['$routeProvider', '$locationProvider', '$httpProvider', function ($routeProvider, $locationProvider, $httpProvider) {

//    //================================================
//    // Add an interceptor for AJAX errors
//    //================================================
//    $httpProvider.responseInterceptors.push(['$q', '$location', function ($q, $location) {
//        return function (promise) {
//            return promise.then(
//              // Success: just return the response
//              function (response) {
//                  return response;
//              },
//              // Error: check the error status to get only the 401
//              function (response) {
//                  if (response.status === 401)
//                      $location.url('/signin');
//                  return $q.reject(response);
//              }
//            );
//        }
//    }]);

//}]);

app.factory('AppService', ['$http', '$q', '$timeout', function ($http, $q, $timeout) {
    function getData(endpoint) {
        var d = $q.defer();
        $http.get(endpoint).success(function (data) {
            d.resolve(data);
        }).error(function (data) {
            d.reject(data);
        });

        return d.promise;
    }

    function postData(endpoint, data) {
        var d = $q.defer();
        $http.post(endpoint, data).success(function (data) {
            d.resolve(data);
        }).error(function (data) {
            d.reject(data);
        });
        return d.promise;
    }
    return {
        Videos: function () {
            return getData('../api/video/').then(function (videos) {

                return videos;
            });
        },
        Video: function (id) {
            return getData('../api/video/' + id).then(function (video) {

                return video;
            });
        }
    }
}]);


app.controller('HomeCtrl', ['$scope', 'AppService',
    function ($scope, AppService) {
        AppService.Videos().then(function (data) {
            $scope.videos = data;
            $scope.layout = 'grid';
        });
    }]);

app.controller('DetailsCtrl', ['$scope', 'AppService', '$routeParams',
    function ($scope, AppService, $routeParams) {
        var id = $routeParams.video;
        AppService.Videos(id).then(function (data) {
            $scope.video = data;
        });

    }]);