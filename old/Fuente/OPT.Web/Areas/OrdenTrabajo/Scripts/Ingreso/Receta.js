$(document).ready(function () {

    $(document).on("submit", "#formReceta", function (e) {
        e.preventDefault();

        const Url = $(this).attr("action");
        const idOT = $("#idOT").val();
        const idRecetaCristales = $("#idRecetaCristales").val();

        const formData = new FormData(this);
        formData.append("idOT", idOT);
        formData.append("idRecetaCristales", idRecetaCristales);

        $.ajax({
            url: Url,
            cache: false,
            contentType: false,
            processData: false,
            type: "POST",
            dataType: "json",
            data: formData
        })
            .done(function (data) {
                if (data.ok) {

                    $("#idRecetaCristales").val(data.respuesta);

                    $("#Receta-tab").removeClass("active");
                    $("#Receta-tab-content").removeClass("show active");

                    $("#Lentes-tab").addClass("active");
                    $("#Lentes-tab-content").addClass("show active");
                }
                else {
                    // UI.alert("Error", data.respuesta);
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

    $(document).on("submit", "#formFichaMedicaReceta", function (e) {
        e.preventDefault();

        const Url = $(this).attr("action");
        const idOT = $("#idOT").val();
        const idRecetaCristales = $("#idRecetaCristales").val();

        const formData = new FormData(this);
        formData.append("idOT", idOT);
        formData.append("idRecetaCristales", idRecetaCristales);

        $.ajax({
            url: Url,
            cache: false,
            contentType: false,
            processData: false,
            type: "POST",
            dataType: "json",
            data: formData
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

    $(document).on("change", "#LejosODEsferico", function () {

        const VarloOD = $("#LejosODEsferico").val() == "" ? 0 : parseFloat($("#LejosODEsferico").val().replace(",", "."));
        const AdicionValor = $("#LejosADDEsfera").val() == "" ? 0 : parseFloat($("#LejosADDEsfera").val().replace(",", "."));
        let Suma = 0;

        if (AdicionValor > 0) {
            Suma = parseFloat(VarloOD) + parseFloat(AdicionValor);
            $("#CercaODEsferico").val(Suma);
        }

    })

    $(document).on("change", "#LejosODCilindro", function () {
        const Valor = $("#LejosODCilindro").val();
        const AdicionValor = $("#LejosADDEsfera").val() == "" ? 0 : parseFloat($("#LejosADDEsfera").val().replace(",", "."));

        if (AdicionValor > 0) {
            $("#CercaODCilindro").val(Valor);
        }
    })

    $(document).on("change", "#LejosODEje", function () {
        const Valor = $("#LejosODEje").val();
        const AdicionValor = $("#LejosADDEsfera").val() == "" ? 0 : parseFloat($("#LejosADDEsfera").val().replace(",", "."));

        if (AdicionValor > 0) {
            $("#CercaODEje").val(Valor);
        }
    })

    $(document).on("change", "#LejosOIEsferico", function () {
        const VarloIO = $("#LejosOIEsferico").val() == "" ? 0 : parseFloat($("#LejosOIEsferico").val());
        const AdicionValor = $("#LejosADDEsfera").val() == "" ? 0 : parseFloat($("#LejosADDEsfera").val());
        let Suma = 0;

        if (AdicionValor > 0) {
            Suma = parseFloat(VarloIO) + parseFloat(AdicionValor);
            $("#CercaOIEsferico").val(Suma);
        }

    })

    $(document).on("change", "#LejosOICilindro", function () {
        const Valor = $("#LejosOICilindro").val();
        const AdicionValor = $("#LejosADDEsfera").val() == "" ? 0 : parseFloat($("#LejosADDEsfera").val().replace(",", "."));

        if (AdicionValor > 0) {
            $("#CercaOICilindro").val(Valor);
        }
    })

    $(document).on("change", "#LejosOIEje", function () {
        const Valor = $("#LejosOIEje").val();
        const AdicionValor = $("#LejosADDEsfera").val() == "" ? 0 : parseFloat($("#LejosADDEsfera").val().replace(",", "."));

        if (AdicionValor > 0) {
            $("#CercaOIEje").val(Valor);
        }
    })

    $(document).on("change", "#LejosADDEsfera", function () {
        const VarloOD = $("#LejosODEsferico").val() == "" ? 0 : parseFloat($("#LejosODEsferico").val());
        const VarloIO = $("#LejosOIEsferico").val() == "" ? 0 : parseFloat($("#LejosOIEsferico").val());
        const AdicionValor = $("#LejosADDEsfera").val() == "" ? 0 : parseFloat($("#LejosADDEsfera").val());
        let SumaOD = 0;
        let SumaIO = 0;

        if (AdicionValor > 0) {
            SumaOD = parseFloat(VarloOD) + parseFloat(AdicionValor);
            SumaIO = parseFloat(VarloIO) + parseFloat(AdicionValor);
            var LejosOIEje = $("#LejosOIEje").val();
            var LejosOICilindro = $("#LejosOICilindro").val();
            var LejosODEje = $("#LejosODEje").val();
            var LejosODCilindro = $("#LejosODCilindro").val();
          
            $("#CercaODEsferico").val(SumaOD);
            $("#CercaODEje").val(LejosODEje);
            $("#CercaODCilindro").val(LejosODCilindro);
            $("#CercaOIEsferico").val(SumaIO);
            $("#CercaOIEje").val(LejosOIEje);
            $("#CercaOICilindro").val(LejosOICilindro);

        }
        else {
            $("#checkCerca").prop("checked", false);
            $.LimpiarCerca();
           
        }
    })

    $(document).on("click", "#checkLejos", function () {
        if ($(this).is(':checked')) {                       
            $('#CheckLejos').val('1');
            $("#LejosODObservacion").prop('required', true);
            $("#LejosOIObservacion").prop('required', true);
            $("#LejosDPObservacion").prop('required', true);

        } else {
            $.LimpiarLejos();
        }
    });

    $(document).on("click", "#checkCerca", function () {
        if ($(this).is(':checked')) {            
            $('#CheckCerca').val('1');
            $("#CercaODObservacion").prop('required', true);
            $("#CercaOIObservacion").prop('required', true);
            $("#CercaDPObservacion").prop('required', true);

        } else {
            $.LimpiarCerca();
        }
    });

    $(document).on("click", "#checkCristalesLaboratorio", function () {
        if ($(this).is(':checked')) {
            $('#CheckCristalesLaboratorio').val('1');

        } else {
            $('#CheckCristalesLaboratorio').val('0');
        }
    });

    $(document).on("click", "#checkUrgente", function () {
        if ($(this).is(':checked')) {
            $('#CheckUrgente').val('1');

        } else {
            $('#CheckUrgente').val('0');
        }
    });

    $(document).on("click", "#btnVolverCliente", function () {
        $("#Receta-tab").removeClass("active");
        $("#Receta-tab-content").removeClass("show active");

        $("#Cliente-tab").addClass("active");
        $("#Cliente-tab-content").addClass("show active");
    })

    $.LimpiarLejos = function () {
        
        $('#CheckLejos').val('0');
        $('#LejosODObservacion').removeAttr('required');
        $('#LejosOIObservacion').removeAttr('required');
        $('#LejosDPObservacion').removeAttr('required');
        $("#LejosODEsferico").val('');
        $("#LejosODEje").val('');
        $("#LejosODCilindro").val('');
        $("#LejosOIEsferico").val('');
        $("#LejosOIEje").val('');
        $("#LejosOICilindro").val('');
        $("#LejosODObservacion").val('');
        $("#LejosOIObservacion").val('');
        $("#LejosDPEsferico").val('');
        $("#LejosDPObservacion").val('');
        $("#LejosADDEsfera").val('');
    }

    $.LimpiarCerca = function () {
       
        $('#CheckCerca').val('0');
        $('#CercaODObservacion').removeAttr('required');
        $('#CercaOIObservacion').removeAttr('required');
        $('#CercaDPObservacion').removeAttr('required');
        $("#CercaODEsferico").val('');
        $("#CercaODEje").val('');
        $("#CercaODCilindro").val('');
        $("#CercaOIEsferico").val('');
        $("#CercaOIEje").val('');
        $("#CercaOICilindro").val('');
        $("#CercaODObservacion").val('');
        $("#CercaOIObservacion").val('');
        $("#CercaDPEsferico").val('');
        $("#CercaDPObservacion").val('');
    }
})