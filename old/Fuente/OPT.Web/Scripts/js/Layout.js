$(document).ready(function () {
        
    $.ValidaFoto = function () {
        $('.File_Archivo').fileinput({
            showUpload: false,
            allowedFileExtensions: ["jpg", "png", "gif"]
        });
    }

    $.ValidaPDF = function () {
        $('.File_Archivo').fileinput({
            showUpload: false,
            allowedFileExtensions: ["pdf"]
        });
    }

    $.ajaxSetup({ cache: false });

    $(document).ajaxStart(function () {
        $.blockUI({
            message: '<i class="fa fa-spinner fa-pulse fa-3x fa-fw"></i><h2>Por favor espere...</h2>',
            css: {
                border: 'none',
                padding: '15px',
                backgroundColor: '#000',
                '-webkit-border-radius': '10px',
                '-moz-border-radius': '10px',
                opacity: .5,
                color: '#fff'
            }
        });
    });

    $(document).ajaxSuccess(function () {
        $.unblockUI();
    });

    $(document).ajaxError(function () {
        $.unblockUI();
    });

    $.DatosSession = function (pUrl) {
        $.ajax({
            url: pUrl,
            cache: false,
            type: 'Post',
            dataType: 'json'
        })
        .done(function (result) {
            if (result.ok) {              
                $('#usuarioId').text(result.respuesta.Nombre);
                $('#SucursalSelected').empty();
                $.each(result.respuesta.Sucursal, function (index, item) {
                    if (result.respuesta.idSucursal == item.idSucursal) {
                        $('#SucursalSelected').append('<option value="' + item.idSucursal + '" selected>' + item.Nombre + '</option>');
                    }
                    else {
                        $('#SucursalSelected').append('<option value="' + item.idSucursal + '">' + item.Nombre + '</option>');
                    }                    
                });
                
            }
            else {
                //UI.alert("Error", result.respuesta);
                swal("Error", result.respuesta, "error");
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $("#MensajeError").empty().append(jqXHR.responseText);
            $('#Error').modal({ show: true });
        });
    }

    $.CreaMenu = function (pURL) {
        $.ajax({
            url: pURL,
            cache: false,
            type: "POST",
            dataType: "html"
        })
        .done(function (result) {
            $("#MenuNav").empty().append(result);
        })
        .fail(function (jqXHR, textStatus, error) {
            if (jqXHR.status == 401) {
                swal("Sesion Terminada", "La sesion a terminado, debe Logearse nuevamente a la pagina.", "error");
            }
            else {
                swal("Error", jqXHR.responseText, "error");
            }
        });
    }

    $.Modal = function (pUrl) {

        $('#my-modal-cont').load(pUrl, function () {
            $('#myModal').modal({ backdrop: 'static', keyboard: false, show: true });
        });
    }

    $(document).on("change", "#SucursalSelected", function () {

        var pUrl = $(this).attr("data-url");
        var valor=$("#SucursalSelected option:selected").val();

        $.ajax({
            url: pUrl,
            cache: false,
            type: 'Post',
            dataType: 'json',
            data: { id: valor}
        })
            .done(function (result) {
                if (result.ok) {
                    location.reload(true);
                }
                else {
                    //UI.alert("Error", result.respuesta);
                    swal("Error", data.respuesta, "error");
                }
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                $("#MensajeError").empty().append(jqXHR.responseText);
                $('#Error').modal({ show: true });
            });
    });
})