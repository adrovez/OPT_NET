$(document).ready(function () {

    $("a[name=btnReporte]").click(function (e) {
        e.preventDefault();

        var Url = $("#formIndex").attr('href');
        //var formData = new FormData();
        //formData.append("pIdEmpresa", $("#pIdEmpresa option:selected").val());
        //formData.append("pFechaDesde", $("#pFechaDesde").val());
        //formData.append("pFechaHasta", $("#pFechaHasta").val());

        $.ajax({
            url: Url,
            data: $("#formIndex").serialize(), //formData,
            type: 'POST',
            contentType: false, // NEEDED, DON'T OMIT THIS (requires jQuery 1.6+)
            processData: false, // NEEDED, DON'T OMIT THIS
            cache: false,
            xhrFields: {
                responseType: 'blob'
            }
        })
            .done(function (response) {

                if (response === 0) {

                    swal("Error al Imprimir", "No cuentas con privilegios para realizar esta acción.", "error");

                } else if (response === 1) {

                    swal("Error al Imprimir", "Ocurrió un problema al intentar descargar el archivo. Por favor intenta nuevamente.", "error");

                }
                else {
                    var a = document.createElement('a');
                    var url = window.URL.createObjectURL(response);
                    a.href = url;
                    a.download = 'PagoDiario.xls';
                    a.click();
                    window.URL.revokeObjectURL(url);
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