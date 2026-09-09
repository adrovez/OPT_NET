var ObjCliente = {};
$(document).ready(function () {

    $("#formIndex").submit(function (e) {
        e.preventDefault();
        $.BuscarPaginar(ObjCliente.UrlPaginar)
    })

    $(document).on('click', 'a[name=paginar]', function (e) {
        e.preventDefault();
        var Url = $(this).attr('href');

        if (Url != undefined) {
            $.BuscarPaginar($(this).attr('href'));
        }
    });

    $.BuscarPaginar = function (pUrl) {

        $('#TableListaEmpresa').empty();

        $.ajax({
            url: pUrl,
            cache: false,
            type: "POST",
            dataType: "html",
            data: $("#formIndex").serialize()
        })
       .done(function (data) {
           $("#TableListaEmpresa").empty().append(data);
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