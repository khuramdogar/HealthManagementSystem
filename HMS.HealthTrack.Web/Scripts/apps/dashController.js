(function () {
   var dashController = function ($scope, $http) {
      var statsUrl = "/Inventory/Inventory";
      var onError = function (reason) {
         $scope.error = "Could not fetch data: " + reason;
      };
      var onGetDashStats = function (response) {
         $scope.notifications = response.data.Data;
         response.data.Data.forEach(function(element) {
            $scope.getNotification(element.DashboardNotificationId);
         })
      };

      var onStatReceived = function(response) {
         var stat = response.data;
         var indexOfStat = -1;
         $scope.notifications.some(function (el, i) {
            if (el.DashboardNotificationId === stat.DashboardNotificationId) {
               indexOfStat = i;
               return true;
            }
         });
         stat.show = stat.ShowWhenZero || stat.ItemCount > 0;
         $scope.notifications[indexOfStat] = stat;
      }

      //Get user notifications
      $scope.getNotifications = function () {
         console.log("Getting notifications");
         $http.post(statsUrl + "/GetNotifications/", { header: { 'Content-Type': "application/json" } })
            .then(onGetDashStats, onError);
      };

      //Get user notifications
      $scope.getNotification = function (notificationId) {
         console.log("Getting notifications");
         $http.get(statsUrl + "/GetNotification/" + notificationId, { header: { 'Content-Type': "application/json" } })
            .then(onStatReceived, onError);
      };
   };
   angular.module("inventoryApp").controller("dashController", ["$scope", "$http", dashController]);
}());