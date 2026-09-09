$(document).ready(function () {

    $(".number").number(true, 0, ",", ".");

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
        const Url = $("#formIndex").attr("action");
        const Return = $("#btnVolverOT").attr("href");
        const MontoPago = parseInt($("#Monto").val().replace(/\./g, ''));
        const SaldoPendiente = parseInt($("#SaldoPendiente").val().replace(/\./g, ''));

        if (MontoPago > SaldoPendiente) {
            swal("Alerta", "¡Monto ingresado no puede ser mayor al Saldo Pendiente!", "warning")
            return false;
        }

        $.ajax({
            url: Url,
            cache: false,
            type: "POST",
            dataType: "json",
            data: $("#formIndex").serialize()
        })
       .done(function (data) {
           if (data.ok) {
               swal("OK", "¡Datos Actualizados!", "success")
                   .then((value) => {
                       // funciona como una redirección HTTP                          
                       setTimeout(function () { location.replace(Return); }, 2000);
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

    $("#idTipoPago").change(function () {
        const TipoPago = $("#idTipoPago option:selected").text();
        $("#TipoPago").val(TipoPago)
    })
       
})
