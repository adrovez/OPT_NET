const ObjAbono = {};
$(document).ready(function () {

    $('#InicioPago').datepicker({
        format: 'dd/mm/yyyy',
        startDate: '+0d',
        language: 'es'
    });

    $.SumarMontoAbono = function () {

        let Suma = 0;
        const Precio = parseInt($("#Precio").val().replace(/\./g, ''));

        $('#TableAbono tbody tr').each(function () {
            var Monto = $(this).find("td")[1].innerHTML;
            Suma += parseFloat(Monto.replace(/\./g, ''));
        });

        $('#TableAbono tfoot tr').find("td").eq(1).html($.number(Suma, 0, ",", "."));

        const Saldo = Precio - Suma;
        $("#Abonos").val(Suma);
        $("#Saldo").val(Saldo);
    }

    $(document).on("submit", "#formAbono", function (e) {
        e.preventDefault();

        const Url = $(this).attr("action");
        const idOT = $("#idOT").val();
        const Abono = parseInt($("#Abono").val().replace(/\./g, ''));
        const TextFormaPago = $("#idFormaPago option:selected").text();
        const Saldo = parseInt($("#Saldo").val().replace(/\./g, ''));

        if (Abono > Saldo) {
            const mensaje = "Monto Ingresado no puede ser mayor al Saldo (" + $.number(Saldo, 0, ",", ".") + ")";
            //UI.alert("Error", "Monto Ingresado no puede ser mayor al Saldo (" + $.number(Saldo, 0, ",", ".") + ")");
            swal("Error", mensaje, "error");
            return false;
        }

        $("#Monto").val(Abono);

        const formData = new FormData(this);
        formData.append("idOT", idOT);


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


                    var fila = "";

                    fila += '<tr>';
                    fila += '<td>' + TextFormaPago + '</td>';
                    fila += '<td class="text-right">' + $.number(Abono, 0, ",", ".") + '</td>';
                    fila += '<td class="text-center"><a href="' + ObjAbono.UrlElimina + "?id=" + data.respuesta + '" type="button" name="EliminaFilaAbono" class="btn btn-sm btn-outline-danger" title="Eliminar..."><i class="fa fa-trash"></i></a></td>';
                    fila += '</tr>';

                    $("#TableAbono tbody").append(fila);
                    $("#idFormaPago").val('');
                    $("#Abono").val('')
                    $("#Monto").val('');
                    $.SumarMontoAbono();
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

    $(document).on("submit", "#formFinaliza", function (e) {
        e.preventDefault();
      
        const Url = $(this).attr("action");
        const idOT = $("#idOT").val();
        const Abono = parseInt($("#Abonos").val().replace(/\./g, ''));
        const Precio = parseInt($("#Precio").val().replace(/\./g, ''));
        const idEmpresa = $("#idEmpresa option:selected").val();
        const UrlFinaliza = ObjAbono.UrlFinaliza + "?id=" + idOT;

        const pPrecio = $("#Precio").val() == "" ? 0 : parseFloat($("#Precio").val().replace(/\./g, ''));
        const pAbono = $('#Abonos').val() == "" ? 0 : parseFloat($('#Abonos').val().replace(/\./g, ''));

        const pSaldo = parseFloat(pPrecio) - parseFloat(pAbono)
        const pCuota = $("#NumeroCuota").val() == "" ? 0 : parseInt($("#NumeroCuota").val());

        if (pPrecio == 0) {
            swal("Alerta", "OT no tiene monto. Debe ingresar Producto", "warning");
            return false;
        }

        if (pSaldo > 0 && pCuota == 0) {
            swal("Alerta", "Debe ingresar numero de cuotas para el saldo pendiente.", "warning");
            return false;
        }

        if (idEmpresa == 0 || idEmpresa == "") {
            swal("Alerta", "Debe seleccionar una empresa para la OT.", "warning");
            return false;
        }

        const formData = new FormData(this);
        formData.append("idOT", idOT);
        formData.append("idEmpresa", idEmpresa);
        formData.append("Abono", Abono);
        formData.append("Precio", Precio);

        $.ajax({
            url: Url,
            cache: false,
            contentType: false,
            processData: false,
            async: false,
            type: "POST",
            dataType: "json",
            data: formData
        })
            .done(function (data) {
                if (data.ok) {
                   
                     swal("OK", "Datos Actualizados", "success")
                        .then((value) => {
                            // funciona como una redirección HTTP                          
                            setTimeout(function () { window.location.replace(UrlFinaliza); });
                        });
                }
                else {
                   // UI.alert("Error", data.respuesta);
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

    $(document).on("change", "#NumeroCuota", function () {

        const pPrecio = $("#Precio").val() == "" ? 0 : parseFloat($("#Precio").val().replace(/\./g, ''));
        const pAbono = $('#Abonos').val() == "" ? 0 : parseFloat($('#Abonos').val().replace(/\./g, ''));

        const pSaldo = parseFloat(pPrecio) - parseFloat(pAbono)
        const pCuota = $("#NumeroCuota").val() == "" ? 0 : parseInt($("#NumeroCuota").val());
        const ValorCuota = pCuota == 0 ? "" : parseInt(parseFloat(pSaldo) / parseFloat(pCuota));

        $("#ValorCuota").val(ValorCuota);

        if (pSaldo == 0 && pCuota == 0) {
            $('#InicioPago').removeAttr('required');
            $('#InicioPago').val('');
        }
        else {
            $('#InicioPago').attr('required', 'required');
        }
    })

    $(document).on('click', 'a[name=EliminaFilaAbono]', function (e) {
        e.preventDefault();

        const Fila = $(this).closest('tr');
        const Url = $(this).attr('href');
        const Res = confirm("¿Desea Eliminar Producto Seleccionado?.")

        if (Res) {

            $.ajax({
                url: Url,
                cache: false,
                type: "GET",
                dataType: "json"
            })
                .done(function (data) {
                    if (data.ok) {

                        Fila.remove();
                        $.SumarMontoAbono();
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
        }

    });

    $(document).on("click", "#btnVolverLente", function () {

        $("#Pago-tab").removeClass("active");
        $("#Pago-tab-content").removeClass("show active");

        $("#Lentes-tab").addClass("active");
        $("#Lentes-tab-content").addClass("show active");
    })

})