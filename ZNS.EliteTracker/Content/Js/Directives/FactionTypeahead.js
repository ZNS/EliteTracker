angular.module('elitetracker')
.directive('factionTypeahead', [function () {
    return {
        restrict: 'E',
        template: '<input id="factionName" name="factionName" class="form-control" value="{{value}}" type="text" placeholder="Faction name" />',
        replace: true,
        link: function ($scope, $element, attr) {
            $scope.value = attr.value;

            $element.easyAutocomplete({
                url: function (q) { return "/faction/typeahead/?q=" + q },
                getValue: "name",
                list: {
                    maxNumberOfElements: 10,
                    requestDelay: 500,
                    onChooseEvent: function () {
                        var item = $element.getSelectedItemData();
                        $("#factionId").val(item.id);
                    }
                }
            }).blur(function () {
                var item = $element.getSelectedItemData();
                if (!item || item === -1) {
                    $("#factionId").val("0");
                }
            });
        }
    };
}]);