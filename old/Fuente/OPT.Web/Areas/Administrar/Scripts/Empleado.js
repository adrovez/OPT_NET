$(document).ready(function () {

    $("#Nuevo").click(function (e) {
        e.preventDefault();
        var Url = $(this).attr('href');
        $('#my-modal-cont').load(Url, function (result) {
            $('#myModal').modal({ backdrop: 'static', keyboard: false, show: true });
            $("#idSucursal").chosen();
        });
    })

    $(document).on("click", "a[name=Ver]", function (e) {
        e.preventDefault();
        var Url = $(this).attr('href');
        $('#my-modal-cont').load(Url, function (result) {
            $('#myModal').modal({ backdrop: 'static', keyboard: false, show: true });
        });
    })

    $(document).on("click", "a[name=Editar]", function (e) {
        e.preventDefault();
        var Url = $(this).attr('href');
        $('#my-modal-cont').load(Url, function (result) {
            $('#myModal').modal({ backdrop: 'static', keyboard: false, show: true });
            $("#idSucursal").chosen();
            $("#idSucursal").trigger("chosen:updated");
        });

    })

    $(document).on('click', 'a[name=paginar]', function (e) {
        e.preventDefault();
        var Url = $(this).attr('href');

        if (Url != undefined) {
            $('#TablaDatos').empty();

            $.ajax({
                url: Url,
                cache: false,
                type: "POST",
                dataType: "html",
                data: $("#formIndex").serialize()
            })
                .done(function (data) {
                    $("#TablaDatos").empty().append(data);
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

    $(document).on("click", "a[name=Eliminar]", function (e) {
        e.preventDefault();
        var Url = $(this).attr('href');

        var Res = confirm("¿Desea Eliminar Producto Seleccionado?.")

        if (Res) {
            $.ajax({
                url: Url,
                cache: false,
                type: "POST",
                dataType: "json"
            })
                .done(function (data) {
                    if (data.ok) {

                        swal("OK", "Datos Eliminados", "success")
                            .then((value) => {
                                // funciona como una redirección HTTP                          
                                setTimeout(function () { location.reload(); }, 2000);
                            });
                    }
                    else {
                        swal("Error", data.respuesta, "error");
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

    })

    $(document).on("submit", "#formIndex", function (e) {
        e.preventDefault();

        $.ajax({
            url: $(this).attr("action"),
            cache: false,
            type: "POST",
            dataType: "html",
            data: $("#formIndex").serialize()
        })
            .done(function (data) {

                $("#TablaDatos").empty();
                $("#TablaDatos").append(data);

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

    $(document).on("submit", "#formCrear", function (e) {
        e.preventDefault();

        $.ajax({
            url: $(this).attr("action"),
            cache: false,
            type: "POST",
            dataType: "json",
            data: $("#formCrear").serialize()
        })
            .done(function (data) {
                if (data.ok) {
                    $("#myModal").modal("hide");
                    $("#ModalCont").empty();

                    swal("OK", "Datos Actualizados", "success")
                        .then((value) => {
                            // funciona como una redirección HTTP                          
                            setTimeout(function () { location.reload(); }, 2000);
                        });
                   
                }
                else {
                    swal("Error", data.respuesta, "error");
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
    })

    $(document).on("submit", "#formEditar", function (e) {
        e.preventDefault();

        $.ajax({
            url: $(this).attr("action"),
            cache: false,
            type: "POST",
            dataType: "json",
            data: $("#formEditar").serialize()
        })
            .done(function (data) {
                if (data.ok) {

                    $("#myModal").modal("hide");
                    $("#ModalCont").empty();

                    swal("OK", "Datos Actualizados", "success")
                        .then((value) => {
                            // funciona como una redirección HTTP                          
                            setTimeout(function () { location.reload(); }, 2000);
                        });                   
                }
                else {
                   
                    swal("Error", data.respuesta, "error");
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
    })


})