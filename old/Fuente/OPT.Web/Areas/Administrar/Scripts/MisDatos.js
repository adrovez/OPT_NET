$(document).ready(function () {

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

                    swal("¡Datos Usuario Actualizado!", "", "success");
                }
                else {
                    swal("Error al Actualizar", data.respuesta, "error");
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