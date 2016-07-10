var app = angular.module("myApp", []);

app.controller("myCtrl", function ($scope, $http) {
    $scope.items = [];

    $http.get('/file/list').
        success(function (data) {
            $scope.items = data;
        });

    function updateItems(items){
        $scope.items = items;
    }

    $scope.range = function (min, max, step) {
        step = step || 1;
        var input = [];
        for (var i = min; i < Math.floor( max ); i += step) {
            input.push(i);
        }
        return input;
    };
});

app.directive('dropTarget', function () {
    return function ($scope, $element) {

        $element.bind('dragleave', function (e) {
            this.className = 'col-sm-2';
            return false;
        });

        $element.bind('drop', function (e) {
            this.className = 'col-sm-2';
            e.preventDefault();
            readfiles(e.dataTransfer.files);
        });

        $element.bind('dragover', function (e) {
            e.preventDefault();
            e.stopPropagation();
            // do something when dragover event is observe
            this.className = 'hover col-sm-2';
            return false;
        });

        
    };
});

