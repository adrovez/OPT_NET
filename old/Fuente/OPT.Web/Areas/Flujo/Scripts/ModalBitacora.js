$(document).ready(function () {

    $(document).on("submit", "#formEnviarOT", function (e) {
        e.preventDefault();

        const Url = $(this).attr("action");


        $.ajax({
            url: Url,
            type: "POST",
            dataType: "json",
            data: $("#formEnviarOT").serialize()
        })
            .done(function (data) {
                if (data.ok) {

                    swal("OK", "¡Datos Actualizados!", "success")
                        .then((value) => {
                            // funciona como una redirección HTTP                          
                            setTimeout(function () { location.reload(); }, 2000);
                        });

                    $("#myModal").modal("hide");
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

