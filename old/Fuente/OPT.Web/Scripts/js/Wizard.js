$(document).ready(function () {

    var current_fs, next_fs, previous_fs; //fieldsets
    var opacity;

    $(document).on("click", "#next1", function () {

        current_fs = $(this).parent();
        next_fs = $(this).parent().next();


        var Mensaje = "";       

        if (Mensaje == "") {
            $.next(current_fs, next_fs)
        }
        else {
            swal("Alerta", Mensaje, "warning");
           // UI.alert("Alertar", Mensaje);
        }

    });

    $(document).on("click", "#next2", function () {

        current_fs = $(this).parent();
        next_fs = $(this).parent().next();


        var Mensaje = "";
       

        if (Mensaje == "") {
            $.next(current_fs, next_fs)
        }
        else {
            //UI.alert("Alertar", Mensaje);
            swal("Alerta", Mensaje, "warning");
        }
    });

    $.next = function (current_fs, next_fs) {

        //Add Class Active
        $("#progressbar li").eq($("fieldset").index(next_fs)).addClass("active");

        //show the next fieldset
        next_fs.show();
        //hide the current fieldset with style
        current_fs.animate({ opacity: 0 }, {
            step: function (now) {
                // for making fielset appear animation
                opacity = 1 - now;

                current_fs.css({
                    'display': 'none',
                    'position': 'relative'
                });
                next_fs.css({ 'opacity': opacity });
            },
            duration: 600
        });
    }

    $(document).on("click", ".previous", function () {

        current_fs = $(this).parent();
        previous_fs = $(this).parent().prev();

        //Remove class active
        $("#progressbar li").eq($("fieldset").index(current_fs)).removeClass("active");

        //show the previous fieldset
        previous_fs.show();

        //hide the current fieldset with style
        current_fs.animate({ opacity: 0 }, {
            step: function (now) {
                // for making fielset appear animation
                opacity = 1 - now;

                current_fs.css({
                    'display': 'none',
                    'position': 'relative'
                });
                previous_fs.css({ 'opacity': opacity });
            },
            duration: 600
        });
    });

    $(document).on("submit", "#msform", function (e) {
        e.preventDefault();

        var Mensaje = "";
        var Password = $("#Password").val();
        var cPassword = $("#cPassword").val();

        if (Password === '') {
            Mensaje += " Debe Ingresar Password <br/> ";
        }
        if (cPassword === '') {
            Mensaje += " Debe Confirmar Password <br/> ";
        }

        if (Mensaje != "") {

           // UI.alert("Alertar", Mensaje);
            swal("Alerta", Mensaje, "warning");
            return false;
        }

        $.ajax({
            url: $("#msform").attr("action"),
            cache: false,
            type: "POST",
            dataType: "json",
            data: $("#msform").serialize()
        })
            .done(function (data) {
                if (data.ok) {
                   // UI.alert("OK", "¡Datos Actualizado!");
                    swal("OK", "Datos Actualizado", "success");
                }
                else {
                    //UI.alert("ERROR", data.respuesta);
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

});