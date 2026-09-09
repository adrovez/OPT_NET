var ObjOTIndex = {};
$(document).ready(function () {

    $.InicializaDataTableOrderFecha("TablaDatos");
    $(".number").number(true, 0, ",", ".");

    $("#formIndex").submit(function (e) {
        e.preventDefault();
        const Url = $(this).attr("action");
        $('#TablaDatos').DataTable().destroy()
        $("#TableListaOT").empty();

        $.ajax({
            url: Url,
            cache: false,           
            type: "POST",
            dataType: "html",
            data: $("#formIndex").serialize()
        })
            .done(function (data) {
                $("#TableListaOT").append(data);
                $.InicializaDataTableOrderFecha("TablaDatos")
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                if (jqXHR.status == 401) {
                    swal("Sesion Terminada", "La sesion a terminado, debe Logearse nuevamente a la pagina.", "error");
                }
                else {
                    swal("Error", jqXHR.responseText, "error");
                }
            });
    })

    $(document).on('click', 'a[name=VerDetalleOT]', function (e) {
        e.preventDefault();
        const url = $(this).attr('href');
        $('#my-modal-cont').load(url, function (result) {
            $('#myModal').modal({ backdrop: 'static', keyboard: false, show: true });
            $(".number").number(true, 0, ",", ".");
        });

    });

    $(document).on('click', 'a[name=EnviarOT]', function (e) {
        e.preventDefault();
        const url = $(this).attr('href');
        $('#my-modal-cont').load(url, function (result) {
            $('#myModal').modal({ backdrop: 'static', keyboard: false, show: true });           
        });

    });

    $(document).on('click', 'a[name=BitacoraOT]', function (e) {
        e.preventDefault();
        const url = $(this).attr('href');
        $('#my-modal-cont').load(url, function (result) {
            $('#myModal').modal({ backdrop: 'static', keyboard: false, show: true });
        });

    });

    $(document).on('click', 'a[name=EliminarOT]', function (e) {
        e.preventDefault();
        const url = $(this).attr('href');
        const Fila = $(this).closest('tr');
        const OT = Fila.find("td").eq(1).html();

        swal({
            title: "Eliminar OT",
            text: "¿Esta seguro de eliminar la OT N°" + OT + "?",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        })
        .then((willDelete) => {
            if (willDelete) {

                    $.ajax({
                        url: url,
                        cache: false,
                        type: "GET",
                        dataType: "json"
                    })
                        .done(function (data) {

                            if (data.ok) {
                                swal("OK", "¡Datos Actualizados!", "success")
                                    .then((value) => {
                                        // funciona como una redirección HTTP                          
                                        setTimeout(function () { location.reload(); }, 2000);
                                    });
                            }
                            else {
                                swal("Error al Actualizar", data.Mensaje, "error");
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
        });


    });

})