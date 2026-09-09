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

    $(document).on('click', 'a[name=Ver]', function (e) {
        e.preventDefault();
        var url = $(this).attr('href');
        $('#my-modal-cont').load(url, function (result) {
            $('#myModal').modal({ backdrop: 'static', keyboard: false, show: true });
        });
     
    });

    $(document).on('click', 'a[name=Historial]', function (e) {
        e.preventDefault();
        var url = $(this).attr('href');
        $('#my-modal-cont').load(url, function (result) {
            $('#myModal').modal({ backdrop: 'static', keyboard: false, show: true });
            $(".number").number(true, 0, ",", ".");
        });

    });

    $(document).on('click', 'a[name=Deuda]', function (e) {
        e.preventDefault();
        var url = $(this).attr('href');
        $('#my-modal-cont').load(url, function (result) {
            $('#myModal').modal({ backdrop: 'static', keyboard: false, show: true });
            $(".number").number(true, 0, ",", ".");
        });

    });

    $.BuscarPaginar = function (pUrl) {

        $('#TableListaCliente').empty();

        $.ajax({
            url: pUrl,
            cache: false,
            type: "POST",
            dataType: "html",
            data: $("#formIndex").serialize()
        })
       .done(function (data) {
           $("#TableListaCliente").empty().append(data);
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