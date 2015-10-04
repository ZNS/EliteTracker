angular.module('elitetracker')
.controller('solarSystemView', ['$scope', '$http', '$timeout', function ($scope, $http, $timeout) {
    var now = moment().utc();
    $scope.showMsg = false;
    $scope.statusPageCount = 1;
    $scope.statusPage = 0;
    $scope.statuses = [];
    $scope.isSaving = false;
    $scope.switchTimeline = 1;
    $scope.timelineRendered = false;

    $scope.init = function (id) {
        $scope.systemId = id;

        $scope.currentStatus = {
            Id: 0,
            SolarSystem: id,
            Date: moment.utc([now.year(), now.month(), now.date()]).toISOString(),
            FactionStatus: []
        };

        $http({ method: 'GET', url: '/solarsystem/getstatus/' + id, params: { page: 0 } })
        .success(function (data) {
            if (data.result.length > 0) {
                $scope.statusPageCount = data.pageCount;
                $scope.statuses = data.result;
                $scope.currentStatus = $.extend(true, {}, data.result[data.result.length - 1]);

                //If the last status is not for the current day use it as a new status template
                if (!moment($scope.currentStatus.Date).isSame(moment.utc(), 'day')) {
                    $scope.currentStatus.Id = 0;
                    $scope.currentStatus.Date = moment.utc([now.year(), now.month(), now.date()]).toISOString();
                    angular.forEach($scope.currentStatus.FactionStatus, function (fs) {
                        fs.Influence = 0;
                    });
                }
            }

            //Track current date
            $scope.currentStatusDate = moment($scope.currentStatus.Date);

            $scope.$watch('switchTimeline', function (val) {
                if (!$scope.timelineRendered)
                    return;

                if (val == 1) {
                    $scope.timeline.data = $scope.factionStatesData;
                }
                else if (val == 2) {
                    $scope.timeline.data = $scope.factionPendingStatesData;
                }
            });

            renderGoogleChart(data.result);
        });
    };

    $scope.prevStatus = function () {
        $scope.currentStatusDate = $scope.currentStatusDate.subtract(1, 'days');
        updateCurrentStatus();
    };

    $scope.nextStatus = function () {
        if (!$scope.currentStatusDate.isSame(moment.utc(), 'day')) {
            $scope.currentStatusDate = $scope.currentStatusDate.add(1, 'days');
            updateCurrentStatus();
        }
    };

    $scope.prevChart = function () {
        $scope.statusPage += 1;
        loadChart();
    };

    $scope.nextChart = function () {
        $scope.statusPage -= 1;
        loadChart();
    };

    function loadChart() {
        $http({ method: 'GET', url: '/solarsystem/getstatus/' + $scope.systemId, params: { page: $scope.statusPage } })
        .success(function (data) {
            renderGoogleChart(data.result);
        });
    }

    function renderGoogleChart(data) {
        $scope.chart = {
            type: "LineChart",
            options: {
                curveType: "function",
                chartArea: { top: 10, left: "4%", width: '96%', height: '100%' },
                hAxis: {
                    format: 'yyyy-MM-dd',
                    gridlines: { count: 15 },
                    minorGridlines: { count: 1 },
                    textStyle: { fontSize: 10 },
                },
                vAxis: {
                    textStyle: { fontSize: 11 },
                    minValue: 0,
                    maxValue: 100
                },
                legend: { position: 'in', alignment: 'center', textStyle: { fontSize: 12 } },
                pointsVisible: true,
                pointSize: 3
            }
        };

        $scope.timeline = {
            type: "Timeline",
            options: {
                backgroundColor: '#fff',
                timeline: {
                    colorByRowLabel: true,
                    showRowLabels: false,
                    showBarLabels: true
                }
            }
        };

        //Create cols
        var cols = [{
            id: "date",
            label: "Date",
            type: "date"
        }];
        var colsTimeline = [{ id: 'Faction', type: 'string' }, {id: 'State', type: 'string'}, { id: 'Start', type: 'date' }, { id: 'End', type: 'date' }];
        angular.forEach(data, function (status) {
            angular.forEach(status.FactionStatus, function (fs) {
                var id = fs.Faction.Id;
                var label = fs.Faction.Name;
                //Only add unique
                var colExists = false;
                angular.forEach(cols, function (col) {
                    if (col.id === "faction_" + id) {
                        colExists = true;
                        return false;
                    }
                })
                if (!colExists) {
                    cols.push({
                        id: "faction_" + id,
                        label: label,
                        type: "number"
                    })
                }
            });
        });

        //Create rows
        var rows = [];
        var rowsTimeline = [];
        var rowsTimeline2 = [];
        var prevStatus = null;
        angular.forEach(data, function (status) {
            var row = { c: [] };
            row.c.push({ v: moment(status.Date).toDate() });
            angular.forEach(cols, function (col, idx) {
                if (idx > 0) //Skip x axis label
                {
                    var foundValue = false;
                    angular.forEach(status.FactionStatus, function (fs) {
                        //Influence
                        if ("faction_" + fs.Faction.Id == col.id)
                        {
                            row.c.push({ v: fs.Influence, f: fs.Influence + "%" });
                            foundValue = true;
                            return false;
                        }
                    });
                    if (!foundValue) {
                        row.c.push({ v: 0, f: "No data" });
                    }
                }
            });
            rows.push(row);

            //Timeline
            angular.forEach(status.FactionStatus, function (fs) {
                //Timeline
                var prevStatusFaction = null;
                if (fs.PendingStates == null)
                    fs.PendingStates = [];
                if (prevStatus != null) {
                    angular.forEach(prevStatus.FactionStatus, function (preFs) {
                        if (fs.Faction.Id === preFs.Faction.Id) {
                            prevStatusFaction = preFs;
                            return false;
                        }
                    });
                }

                if (prevStatusFaction == null || prevStatusFaction.PendingStates.toString() != fs.PendingStates.toString()) {
                    if (prevStatusFaction != null) {
                        for (var i = rowsTimeline2.length - 1; i > -1; i--) {
                            var r = rowsTimeline2[i];
                            if (r.c[0].v === fs.Faction.Name) {
                                r.c[3].v = moment(status.Date).toDate();
                                break;
                            }
                        }
                    }
                    var pendingStates = "None";
                    if (fs.PendingStates != null && fs.PendingStates.length > 0) {
                        pendingStates = "";
                        angular.forEach(fs.PendingStates, function (ps) {
                            pendingStates += getStateName(ps) + ",";
                        });
                    }
                    var rowTimeline2 = { c: [] };
                    rowTimeline2.c.push({ v: fs.Faction.Name });
                    rowTimeline2.c.push({ v: pendingStates });
                    rowTimeline2.c.push({ v: moment(status.Date).toDate() });
                    rowTimeline2.c.push({ v: moment(data[data.length - 1].Date).toDate() });
                    rowsTimeline2.push(rowTimeline2);
                }

                if (prevStatusFaction == null || prevStatusFaction.State !== fs.State) {
                    if (prevStatusFaction != null) {
                        for (var i = rowsTimeline.length - 1; i > -1; i--) {
                            var r = rowsTimeline[i];
                            if (r.c[0].v === fs.Faction.Name) {
                                r.c[3].v = moment(status.Date).toDate();
                                break;
                            }
                        }
                    }
                    var rowTimeline = { c: [] };
                    rowTimeline.c.push({ v: fs.Faction.Name });
                    rowTimeline.c.push({ v: getStateName(fs.State) });
                    rowTimeline.c.push({ v: moment(status.Date).toDate() });
                    rowTimeline.c.push({ v: moment(data[data.length - 1].Date).toDate() });
                    rowsTimeline.push(rowTimeline);
                }
            });
            prevStatus = status;
        });

        $scope.factionStatesData = {
            "cols": colsTimeline,
            "rows": rowsTimeline
        };

        $scope.factionPendingStatesData = {
            "cols": colsTimeline,
            "rows": rowsTimeline2
        }

        $scope.chart.data = {
            "cols": cols,
            "rows": rows
        };
        $scope.timeline.data = $scope.factionStatesData;
        $scope.timelineRendered = true;
    }

    function getStateName(stateId)
    {
        switch (stateId)
        {
            case 2:
                return "Boom";
            case 3:
                return "Expansion";
            case 4:
                return "Lockdown";
            case 5:
                return "Civil unrest";
            case 6:
                return "Civil war";
            case 7:
                return "War";
            case 8:
                return "Outbreak";
            case 9:
                return "Election";
        }
        return "No State";
    }

    function updateCurrentStatus()
    {
        var exists = false;
        angular.forEach($scope.statuses, function (status) {
            if (moment(status.Date).isSame($scope.currentStatusDate, 'day'))
            {
                $scope.currentStatus = status;
                exists = true;
                return false;
            }
        });

        if (!exists) {
            var clone = $.extend(true, {}, $scope.currentStatus);
            clone.Id = 0;
            clone.Date = $scope.currentStatusDate.toISOString();
            angular.forEach(clone.FactionStatus, function (fs) {
                fs.Influence = 0;
            });
            $scope.statuses.push(clone);
            $scope.statuses.sort(function (a, b) {
                return (a.Date < b.Date) ? -1 : 1;
            });
            $scope.currentStatus = clone;
        }
    }

    $scope.addFaction = function () {
        $scope.currentStatus.FactionStatus.push({
            Faction: { Id: 0, Name: '' },
            Influence: 0,
            State: 0,
            PendingStates: []
        });
    };

    $scope.removeFaction = function (id) {
        var index = -1;
        angular.forEach($scope.currentStatus.FactionStatus, function (faction, idx) {
            if (faction.Faction.Id == id) {
                index = idx;
                return false;
            }
        });
        if (index > -1) {
            $scope.currentStatus.FactionStatus.splice(index, 1);
        }
    };

    $scope.comparePendingStates = function (obj1, obj2) {
        return parseInt(obj1) === parseInt(obj2);
    };

    $scope.saveCurrentStatus = function () {
        $scope.isSaving = true;
        $http.post('/solarsystem/saveStatus/' + $scope.currentStatus.Id, $scope.currentStatus)
        .success(function (response) {
            $scope.currentStatus.Id = response.id;
            angular.forEach($scope.statuses, function (status) {
                if (moment(status.Date).isSame(moment(response.date), 'day'))
                {
                    status.Id = response.id;
                    return false;
                }
            });
            $scope.showMessage('Save successful', 1);
            $scope.isSaving = false;
        });
    };

    $scope.addActiveCommander = function() {
        $http.post('/solarsystem/addactivecommander/' + $scope.systemId).success(function () {
            window.location.reload();
        });
    };

    $scope.removeActiveCommander = function () {
        $http.post('/solarsystem/removeactivecommander/' + $scope.systemId).success(function () {
            window.location.reload();
        });
    };

    $scope.showMessage = function (msg, status) {
        $scope.msg = msg;
        $scope.msgStatus = status;
        $scope.showMsg = true;
        $timeout(function () { $scope.showMsg = false; }, 3000);
    };
}]);