const ObjAtencion = {};

$(document).ready(function () {
    
    $('#FechaNacimiento').datepicker({
        format: 'dd/mm/yyyy',
        //startDate: '0d',
        endDate: '0d',
        language: 'es'
    });

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
                url: ObjAtencion.UrlDatoCliente,
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
                        $("#idComuna").val(data.respuesta.idComuna);
                        $("#TipoPrevision").val(data.respuesta.TipoPrevision);
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
      
    $(document).on("submit", "#formAtencion", function (e) {
        e.preventDefault();

        const Url = $(this).attr("action");
        
        $.ajax({
            url: Url,
            cache: false,
            type: "POST",
            dataType: "json",
            data: $("#formAtencion").serialize()
        })
            .done(function (data) {
                if (data.ok) {
                    swal("OK", "Datos Actualizados", "success")
                        .then((value) => {
                            // funciona como una redirección HTTP                          
                            setTimeout(function () { window.location.replace(ObjAtencion.UrlIndex);},3000);
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
    
})