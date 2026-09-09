$(document).ready(function () {

    $("#idEmpresa").chosen({ allow_single_deselect: true });

    $('#FechaPago').datepicker({
        format: "dd/mm/yyyy",
        //startDate: '0d',
        endDate: '0d',
        language: "es",
        autoclose: true,
        todayHighlight: true
    });

    $("#formIndex").submit(function (e) {
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

               swal("OK", data.Mensaje, "success");

               $("#TableError tbody").empty();
               $.each(data.Respuesta, function (key, Item) {

                   $("#TableError tbody").append('<tr><td>' + Item.Fila + '</td>' +
                                                     '<td>' + Item.OT + '</td>' +                                                     
                                                     '<td>' + Item.Error + '</td></tr>');
               });
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

})
