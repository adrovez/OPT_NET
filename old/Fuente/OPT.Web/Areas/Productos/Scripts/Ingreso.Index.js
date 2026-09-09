var ObjOTIndex = {};
$(document).ready(function () {

    $.InicializaDataTableOrderFecha("TablaDatos");

    $("#formIndex").submit(function (e) {
        e.preventDefault();
        const Url = $(this).attr("action");

        $.ajax({
            url: Url,
            cache: false,
            type: "POST",
            dataType: "html",
            data: $("#formIndex").serialize()
        })
        .done(function (data) {

            $('#TablaDatos').DataTable().destroy()
            $("#TableListaIngreso").empty().append(data);
            $.InicializaDataTableOrderFecha("TablaDatos")

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

    $(document).on('click', 'a[name=VerDetalleDocumento]', function (e) {
        e.preventDefault();
        const url = $(this).attr('href');
        $('#my-modal-cont').load(url, function (result) {
            $('#myModal').modal({ backdrop: 'static', keyboard: false, show: true });
        });

    });

})