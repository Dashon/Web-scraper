'use strict';

var app = angular.module('videoLinks', ['ngRoute', 'infinite-scroll']).directive('modalDialog', function () {
    return {
        restrict: 'E',
        scope: {
            show: '='
        },
        replace: true, // Replace with the template below
        transclude: true, // we want to insert custom content inside the directive
        link: function (scope, element, attrs) {
            scope.dialogStyle = {};
            if (attrs.width)
                scope.dialogStyle.width = attrs.width;
            if (attrs.height)
                scope.dialogStyle.height = attrs.height;
            scope.hideModal = function () {
                scope.show = false;
            };
        },
        template: "<div class='ng-modal' ng-show='show'>" +
                  "<div class='ng-modal-overlay''></div>" +
                  "<div class='ng-modal-dialog' ng-style='dialogStyle'>" +
                  "<div class='ng-modal-dialog-content' ng-transclude></div>" +
                  "</div></div>"
    };
});;


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


app.service('AppService', ['$http', '$q', function ($http, $q) {
    var orderByParam = '';
    var filterParam = '';
    var nextPageNumber = 0;

    var library = function () {
        this.videos = [];
        this.busy = false;
        this.orderBy = 'ReleaseDate desc';
        this.filter = '';
    };

    library.prototype.nextPage = function () {
        if (this.busy) return;
        this.busy = true;

        if (this.orderBy != '') {
            orderByParam = '&$orderby='.concat(this.orderBy);
        }
        if (this.filter != '') {
            filterParam = '&$filter='.concat(this.filter);
        }

        getData('../odata/Videos?$expand=Genres&$skip='.concat(this.videos.length, orderByParam, filterParam)).then(function (data) {
            var vids = data.value;
            for (var i = 0; i < vids.length; i++) {
                this.videos.push(vids[i]);
            }
            this.busy = false;
        }.bind(this));
    };

    library.prototype.getVideo = function (id) {
        getData('../odata/Video/'.concat('(', id, ')', filterParam)).then(function (video) {
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
        $scope.layout = 'grid';
        $scope.Library.nextPage();
    }]);


app.controller('DetailsCtrl', ['$scope', 'AppService',
    function ($scope, AppService) {
        //hack to get the id form the url
        var id = window.location.pathname.match(/\d+/);

        $scope.Library.getVideo(id).then(function (data) {
            $scope.data = data;
        });
    }]);

