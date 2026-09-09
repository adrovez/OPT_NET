const ObjCliente = {};

$(document).ready(function () {

    $("#idEmpresa").chosen({ allow_single_deselect: true });

    $('#FechaEntrega').datepicker({
        format: 'dd/mm/yyyy',
        startDate: '+0d',
        language: 'es'
    });

    $('#FechaNacimiento').datepicker({
        format: 'dd/mm/yyyy',
        //startDate: '0d',
        endDate: '0d',
        language: 'es'
    });

    $(document).on("change", "#idOT", function () {

        var IdOT = $("#idOT").val();

        $.ajax({
            url: ObjCliente.UrlExiste,
            cache: false,
            type: "POST",
            dataType: "json",
            data: { id: IdOT }
        })
            .done(function (data) {

                if (data.ok == false) {
                    //UI.alert("Error", data.respuesta);
                    swal("¡Alerta!", data.respuesta, "warning");
                    $("#idOT").val('');
                }
            })
            .fail(function (jqXHR, textStatus, errorThrown) {

                if (jqXHR.status == 401) {
                    swal("Sesion Terminada", "La sesion a terminado, debe Logearse nuevamente a la pagina.", "error");
                }
                else {
                    //UI.alert("Error", jqXHR.responseText + '/n' + textStatus + '/n' + errorThrown);
                    swal("Error", jqXHR.responseText, "error");
                }
               
            });

    })

    $(document).on("change", "#RutCliente", function () {

        let valida = true;
        const pRut = $.Rut.formatear($(this).val(), true);

        if ($.Rut.validar(pRut)) {
            $(this).val(pRut);
        }
        else {
            $(this).val('');
            $(this).focus();
            swal("¡Alerta!", "Rut ingresado no es valido.", "warning");
            valida = false;
        }

        if (valida) {

            $.ajax({
                url: ObjCliente.UrlDatoCliente,
                cache: false,
                async: false,
                type: "POST",
                dataType: "json",
                data: { id: pRut }
            })
                .done(function (data) {
                    if (data.ok) {
                        let FechaNacimiento = "";

                        if (data.respuesta.FechaNacimiento != null) {
                            FechaNacimiento = $.FormatoFechaJson(data.respuesta.FechaNacimiento);
                        }

                        $("#RutCliente").val(data.respuesta.RutCliente);
                        $("#Nombre").val(data.respuesta.Nombre);
                        $("#idEmpresa").val(data.respuesta.idEmpresa);
                        $("#idRegion").val(data.respuesta.idRegion);
                        $("#Direccion").val(data.respuesta.Direccion);
                        $("#Celular").val(data.respuesta.Celular);
                        $("#Mail").val(data.respuesta.Mail);
                        $("#FechaNacimiento").val(FechaNacimiento);
                        $("#RecetaRutCliente").val(data.respuesta.RutCliente);
                        $("#TipoPrevision").val(data.respuesta.TipoPrevision);

                        $("#idComuna").empty();
                        $("#idComuna").append('<option value="">--- SELECCIONAR ---</option>');

                        $.each(data.respuesta.ListComuna, function (index, item) {
                            $('#idComuna').append('<option value="' + item.Value + '">' + item.Text + '</option>');
                        });

                        $("#idComuna").val(data.respuesta.idComuna);

                        $('#idEmpresa').trigger('chosen:updated');
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

    $(document).on("change", "#idRegion", function () {

        const IdRegion = $("#idRegion option:selected").val();

        $('#idComuna').empty();
        $('#idComuna').append('<option>--- SELECCIONAR ---</option>');

        $.ajax({
            url: ObjCliente.UrlComuna,
            cache: false,
            type: "POST",
            dataType: "json",
            data: { id: IdRegion }
        })
            .done(function (data) {
                if (data.ok) {

                    $.each(data.respuesta, function (index, item) {
                        $('#idComuna').append('<option value="' + item.idComuna + '">' + item.Comuna + '</option>');
                    });
                }
                else {
                    //UI.alert("Error", data.respuesta);
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

    $(document).on("submit", "#formCliente", function (e) {
        e.preventDefault();

        const Url = $(this).attr("action");
        const idOT = $("#idOT").val();
        const idEmpresa = $("#idEmpresa option:selected").val();
        const FechaAtencion = $("#FechaAtencion").val();
        const FechaEntrega = $("#FechaEntrega").val();
        const Beneficiario = $("#Beneficiario").val();

        if (idOT == 0) {
            swal("Alerta", "Debe ingresar Numero de OT.", "warning");
            return false;
        }

        if (idEmpresa == 0 || idEmpresa == "") {
            swal("Alerta", "Debe seleccionar una empresa para la OT.", "warning");
            return false;
        }

        if (FechaEntrega == "" || idEmpresa == "") {
            swal("Alerta", "Debe debe ingresar fecha de entrega", "warning");
            return false;
        }


        const formData = new FormData(this);
        formData.append("idOT", idOT);
        formData.append("idEmpresa", idEmpresa);
        formData.append("FechaAtencion", FechaAtencion);
        formData.append("FechaEntrega", FechaEntrega);
        formData.append("Beneficiario", Beneficiario);

        $.ajax({
            url: Url,
            cache: false,
            contentType: false,
            processData: false,
            async: false,
            type: "POST",
            dataType: "json",
            data: formData
        })
            .done(function (data) {
                if (data.ok) {

                    $.CargaReceta(data.respuesta);

                    $("#Cliente-tab").removeClass("active");
                    $("#Cliente-tab-content").removeClass("show active");

                    $("#Receta-tab").addClass("active");
                    $("#Receta-tab-content").addClass("show active");

                    $("#idOT").prop('readOnly', true);
                    $("#FechaAtencion").prop('readOnly', true);
                    $("#FechaEntrega").prop('readOnly', true);
                    $("#HoraEntrega").prop('readOnly', true);
                }
                else {
                    //UI.alert("Error", data.respuesta);
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

    $.CargaReceta = function (pRut) {

        $("#RecetaRutCliente").val(pRut);
        const idReceta = $("#idRecetaCristales").val();
        const formData = new FormData();
        formData.append("Rut", pRut);
        formData.append("id", idReceta);

        $.ajax({
            url: ObjCliente.UrlBuscaReceta,
            cache: false,
            contentType: false,
            processData: false,
            type: "POST",
            dataType: "json",
            data: formData
        })
            .done(function (data) {
                if (data.ok) {
                    console.log(data.Respuesta);
                    if (data.Respuesta.idRecetaCristales > 0) {
                        const FechaIngreso = $.FormatoFechaJson(data.Respuesta.FechaIngreso);

                        $("#idRecetaCristales").val(data.Respuesta.idRecetaCristales);
                        $("#RecetaFechaIngreso").val(FechaIngreso);

                        $("#LejosODEsferico").val(data.Respuesta.LejosODEsferico);
                        $("#LejosODCilindro").val(data.Respuesta.LejosODCilindro);
                        $("#LejosODEje").val(data.Respuesta.LejosODEje);
                        $("#LejosODObservacion").val(data.Respuesta.LejosODObservacion);
                        $("#LejosOIEsferico").val(data.Respuesta.LejosOIEsferico);
                        $("#LejosOICilindro").val(data.Respuesta.LejosOICilindro);
                        $("#LejosOIEje").val(data.Respuesta.LejosOIEje);
                        $("#LejosOIObservacion").val(data.Respuesta.LejosOIObservacion);
                        $("#LejosDPEsferico").val(data.Respuesta.LejosDPEsferico);
                        $("#LejosDPObservacion").val(data.Respuesta.LejosDPObservacion);
                        $("#CercaODEsferico").val(data.Respuesta.CercaODEsferico);
                        $("#CercaODCilindro").val(data.Respuesta.CercaODCilindro);
                        $("#CercaODEje").val(data.Respuesta.CercaODEje);
                        $("#CercaODObservacion").val(data.Respuesta.CercaODObservacion);
                        $("#CercaOIEsferico").val(data.Respuesta.CercaOIEsferico);
                        $("#CercaOICilindro").val(data.Respuesta.CercaOICilindro);
                        $("#CercaOIEje").val(data.Respuesta.CercaOIEje);
                        $("#CercaOIObservacion").val(data.Respuesta.CercaOIObservacion);
                        $("#LejosADDEsfera").val(data.Respuesta.LejosADDEsfera);
                        $("#CercaDPEsferico").val(data.Respuesta.CercaDPEsferico);
                        $("#CercaDPObservacion").val(data.Respuesta.CercaDPObservacion);

                        if (data.Respuesta.CheckLejos == true) {
                            $("#checkLejos").prop("checked", true);
                            $("#CheckLejos").val("1");
                        }

                        if (data.Respuesta.CheckCerca == true) {
                            $("#checkCerca").prop("checked", true);
                            $("#CheckCerca").val("1");
                        }

                        if (data.Respuesta.CheckCristalesLaboratorio == true) {
                            $("#checkCristalesLaboratorio").prop("checked", true);
                            $("#CheckCristalesLaboratorio").val("1");
                        }

                        if (data.Respuesta.CheckUrgente == true) {
                            $("#checkUrgente").prop("checked", true);
                            $("#CheckUrgente").val("1");
                        }
                    }

                }
                else {
                    //UI.alert("Error", data.Mensaje);
                    swal("Error", date.Mensaje, "error");
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