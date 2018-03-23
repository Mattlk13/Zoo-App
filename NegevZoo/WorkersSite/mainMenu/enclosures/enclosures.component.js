﻿app.controller('zooEnclosureCtrl', ['$scope', '$mdDialog', 'enclosureService',
function enclosureController($scope, $mdDialog, enclosureService) {
    $scope.page             = 'list';
    
    initializeComponent();
    
    function initializeComponent() {
        $scope.updateEnclosures     = function () {
            $scope.isLoading        = true;
            
            enclosureService.enclosures.getAllEnclosures().then(
                function (data) {
                    $scope.enclosures   = data.data;
                    $scope.isLoading    = false;
                    
                    for (let e of $scope.enclosures) {
                        e.pictureUrl = app.baseURL + e.pictureUrl;
                    }
                },
                function () {
                    $mdDialog.show(
                        $mdDialog.alert()
                        .clickOutsideToClose(true)
                        .textContent('אירעה שגיאה במהלך טעינת הנתונים')
                        .ok('סגור')
                    );
                    
                    $scope.isLoading    = false;
                });
            };
            
            $scope.switchPage           = function(page, selectedEnclosure) {
                $scope.page                 = page;
                $scope.selectedEnclosure    = selectedEnclosure || { };
            };
            
            $scope.openMap              = function(ev, selectedEnclosure) {
                $mdDialog.show({
                    controller:             MapDialogController,
                    templateUrl:            'mainMenu/enclosures/map.dialog.html',
                    parent:                 angular.element(document.body),
                    targetEvent:            ev,
                    clickOutsideToClose:    true,
                })
                .then(function(clickPosition) {
                    selectedEnclosure.markerLongitude   = clickPosition.height;
                    selectedEnclosure.markerLatitude    = clickPosition.width;

                    console.log(selectedEnclosure);
                });
            };

            $scope.addEnclosure = function(enclosure) {
                $scope.isLoading            = true;
                    var successContent      = $scope.page === 'create' ? 'המתחם נוסף בהצלחה!' : 'המתחם עודכן בהצלחה!';
                    var failContent         = $scope.page === 'create' ? 'התרחשה שגיאה בעת שמירת המתחם' : 'התרחשה שגיאה בעת עדכון המתחם';
    
                    enclosureService.enclosures.updateEnclosure(enclosure).then(
                        function () {
                            $mdDialog.show(
                                $mdDialog.alert()
                                    .clickOutsideToClose(true)
                                    .textContent(successContent)
                                    .ok('סגור')
                            );
    
                            $scope.isLoading = false;
                        },
                        function () {
                            $mdDialog.show(
                                $mdDialog.alert()
                                    .clickOutsideToClose(true)
                                    .textContent(failContent)
                                    .ok('סגור')
                            );
    
                            $scope.isLoading = false;
                        });
            };
            
            $scope.updateEnclosures();
        };

        $scope.uploadFile = function() {
            console.log($scope.myFile);
        }

        $scope.test = function () {
            console.log('test');
        }

        function MapDialogController($scope, $mdDialog) {
            $scope.img          = new Image();

            $scope.img.onload = function () {
                $scope.originalPicWidth = document.getElementById('pic').width;

                $scope.mapStyle = {
                    width:  $scope.img.width + 'px',
                    height: $scope.img.height + 'px'
                }
            }
            
            $scope.img.src = "http://localhost:5987/assets/zoo_map.png";

            $scope.test = function(event) {
                var widthOffset = $scope.img.width - $scope.originalPicWidth;

                var clickPosition = {
                    width: event.layerX + widthOffset,
                    height: event.layerY
                };

                $mdDialog.hide(clickPosition);
            }
        }
    }])
    .directive('zooEnclosures', function () {
        return {
            templateUrl: 'mainMenu/enclosures/enclosures.html'
        };
    });