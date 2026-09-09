var ObjProducto = {};
$(document).ready(function () {

    $("#formCreate").submit(function (e) {
        e.preventDefault();
        const Url = $(this).attr("action");
        const myArray = $("#Producto").val().split("-");

        if (myArray.length < 3) {
            swal("Alerta", "El nombre del producto debe tener el formato MARCA-PRODUCTO-COLOR.", "warning");
            return false;
        }

        $.ajax({
            url: Url,
            cache: false,
            type: "POST",
            dataType: "json",
            data: $("#formCreate").serialize()
        })
            .done(function (data) {
                if (data.ok) {
                    swal("OK", "Datos Actualizados", "success")
                    $("#formCreate")[0].reset();
                }
                else {
                    swal("Error", data.Mensaje, "error");
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

    $(document).on('change', '#Codigo', function () {

        const codigo = $(this).val();

        $.ajax({
            url: ObjProducto.UrlExiste,
            cache: false,
            type: "POST",
            dataType: "json",
            data: { id: codigo}
        })
            .done(function (data) {
                if (data.ok==false) {
                    swal("Alerta", data.Mensaje, "warning");
                    $("#Codigo").val("");
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
    });



})