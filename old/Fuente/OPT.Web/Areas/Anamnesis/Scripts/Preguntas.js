const objReceta = {};

$(document).ready(function () {

    $(document).on("submit", "#formPreguntas", function (e) {
        e.preventDefault();

        const Url = $(this).attr("action");
        const formData = new FormData(this);

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