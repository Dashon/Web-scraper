'use strict';

var app = angular.module('videoLinks', ['ngRoute', 'infinite-scroll']);


app.config(['$routeProvider', '$locationProvider', '$httpProvider', function ($routeProvider, $locationProvider, $httpProvider) {

    //================================================
    // Add an interceptor for AJAX errors
    //================================================
    $httpProvider.responseInterceptors.push(['$q', '$location', function ($q, $location) {
        return function (promise) {
            return promise.then(
              // Success: just return the response
              function (response) {
                  return response;
              },
              // Error: check the error status to get only the 401
              function (response) {
                  if (response.status === 401)
                      $location.url('/signin');
                  return $q.reject(response);
              }
            );
        }
    }]);

    $locationProvider.html5Mode(true);

}]);


app.factory('AppService', ['$http', '$q', function ($http, $q) {

    var library = function () {
        this.videos = [];
        this.busy = false;
        this.after = 0;
        this.orderBy = 'releaseDate desc';
        this.filter = '';
    };


    library.prototype.nextPage = function () {
        if (this.busy) return;
        this.busy = true;

        var orderByParam = '&$orderby=';
        var filterParam = '&$filter=';

        if (this.orderBy != '') {
            orderByParam = orderByParam.concat(this.orderBy);
        }
        if (this.filter != '') {
            filterParam = filterParam.concat(this.filter);
        }
        getData('../api/video?$skip='.concat(this.after, orderByParam, filterParam)).then(function (videos) {
            for (var i = 0; i < videos.length; i++) {
                this.videos.push(videos[i].data);
            }
            this.after = this.videos[this.videos.length - 1].id;
            this.busy = false;
        });
    };


    library.prototype.getVideo = function (id) {
        getData('../api/video/'.concat(id, filterParam)).then(function (video) {
            return video;
        });
    };

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
            d.esolve(data);
        }).error(function (data) {
            d.reject(data);
        });
        return d.promise;
    }

    return { Library: library };
}]);


app.controller('HomeCtrl', ['$scope', 'AppService',
    function ($scope, AppService) {
        $scope.Library = new AppService.Library();
    }]);


app.controller('DetailsCtrl', ['$scope', 'AppService',
    function ($scope, AppService) {
        //hack to get the id form the url
        var id = window.location.pathname.match(/\d+/);

        //AppService.Video(id).then(function (data) {
        //    $scope.data = $scope.Library.getVideo;
        //});
    }]);

