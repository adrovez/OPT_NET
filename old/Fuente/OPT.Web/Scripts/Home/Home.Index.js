var ObjHome = {};

$(function () {
    // Create a function that will handle AJAX requests
    function requestData(chart) {
        $.ajax({
            url: ObjHome.UrlPeriodo, // This is the URL to the API
            cache: false,
            type: "GET"
        })
    .done(function (data) {
        if (data.ok) {
            // When the response to the AJAX request comes back render the chart with new data
            //chart.setData(JSON.parse(data.respuesta));
            chart.setData(data.respuesta);
            $("#TotalChart").html($.number(data.total, 0, ',', '.'));
        }
        else {
            alert(data.respuesta);
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        if (jqXHR.status == 401) {
            swal("Sesion Terminada", "La sesion a terminado, debe Logearse nuevamente a la pagina.", "error");
        }
        else {
            swal("Error", jqXHR.responseText, "error");
        }
    });
    }

    function requestData2(chart2) {
        $.ajax({
            url: ObjHome.UrlPeriodo2, // This is the URL to the API
            cache: false,
            type: "GET"
        })
    .done(function (data) {
        if (data.ok) {
            // When the response to the AJAX request comes back render the chart with new data
            //chart.setData(JSON.parse(data.respuesta));
            
            chart2.setData(data.respuesta);
            $("#TotalChart2").html($.number(data.total, 0, ',', '.'));
            
        }
        else {
            alert(data.respuesta);
        }
    })
            .fail(function (jqXHR, textStatus, errorThrown) {
                if (jqXHR.status == 401) {
                    swal("Sesion Terminada", "La sesion a terminado, debe Logearse nuevamente a la pagina.", "error");
                }
                else {
                    swal("Error", jqXHR.responseText, "error");
                }
            });
    }

    function requestData3(chart3) {
        $.ajax({
            url: ObjHome.UrlPeriodo3, // This is the URL to the API
            cache: false,
            type: "GET"
        })
    .done(function (data) {
        if (data.ok) {
            // When the response to the AJAX request comes back render the chart with new data
            //chart.setData(JSON.parse(data.respuesta));
            
            chart3.setData(data.respuesta);
            $("#TotalChart3").html($.number(data.total, 0, ',', '.'));
        }
        else {
            alert(data.respuesta);
        }
    })
            .fail(function (jqXHR, textStatus, errorThrown) {
                if (jqXHR.status == 401) {
                    swal("Sesion Terminada", "La sesion a terminado, debe Logearse nuevamente a la pagina.", "error");
                }
                else {
                    swal("Error", jqXHR.responseText, "error");
                }
            });
    }

    // Use Morris.Bar
    var chart = Morris.Donut({
        element: 'morris-bar-chart',
        data: [{ "value": "", "label": "" }],
        formatter: function (x) { return $.number(x, 0, ',', '.') }
    }).on('click', function (i, row) {
        console.log(i, row);
    });

    var chart2 = Morris.Donut({
        element: 'morris-bar-chart2',
        data: [{ "value": "", "label": "" }],
        formatter: function (x) { return $.number(x, 0, ',', '.') }
    }).on('click', function (i, row) {
        console.log(i, row);
    });

    var chart3 = Morris.Donut({
        element: 'morris-bar-chart3',
        data: [{ "value": "", "label": "" }],
        formatter: function (x) { return $.number(x, 0, ',', '.') }
    }).on('click', function (i, row) {
        console.log(i, row);
    });


    //var chart = Morris.Bar({
    //    element: 'morris-bar-chart',
    //    data: [],
    //    xkey: 'Periodo',
    //    ykeys: ['Cantidad', 'Precio'],
    //    labels: ['Cantidad OT', 'Total Precio'],
    //    hideHover: 'auto',
    //    resize: true
    //});


    //var chart = Morris.Donut({
    //    element: 'morris-donut-chart',
    //    data: [],
    //    resize: true
    //});


    //var chart = Morris.Area({
    //    element: 'morris-bar-chart',
    //    data: [],
    //    xkey: 'Periodo',
    //    ykeys: ['Cantidad', 'Precio'],
    //    labels: ['Cantidad OT', 'Total Precio'],
    //    pointSize: 12,
    //    hideHover: 'auto',
    //    resize: true
    //});

    // Request initial data for the past 7 days:
    requestData(chart);
    requestData2(chart2);
    requestData3(chart3);

});