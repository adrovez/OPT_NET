var ObjCliente = {};

$(document).ready(function () {

    $('#FechaNacimiento').datepicker({
        format: 'dd/mm/yyyy',
        //startDate: '0d',
        endDate: '0d',
        language: 'es'
    });

    $("#idRegion").change(function () {

        var IdRegion = $("#idRegion option:selected").val();

        $('#idComuna').empty();
        $('#idComuna').append('<option value="">--- SELECCIONAR ---</option>');

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
       
    $("#formCreate").submit(function (e) {

        var rutformato = $("#RutCliente").val();
        var existe = $.ExisteRut(rutformato);

        if (existe) {
            e.preventDefault();
            //UI.alert("ALERTA:", "Cliente ya existe.");
            swal("Alerta", "Cliente ya existe", "warning");
            $("#formCreate")[0].reset();
        }
    })

    $.ExisteRut = function (pRut) {
        var Valida = false;

        $.ajax({
            url: ObjCliente.UrlExisteRut,
            cache: false,
            async: false,
            type: "POST",
            dataType: "json",
            data: { id: pRut }
        })
       .done(function (data) {
           if (data.ok == false) {              
               swal("Alerta", data.respuesta, "warning");
               Valida = true;
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

        return Valida;
    }
})