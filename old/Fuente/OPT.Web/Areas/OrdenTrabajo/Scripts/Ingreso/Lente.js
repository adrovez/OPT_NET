const ObjLente = {};
$(document).ready(function () {

    $("#idProducto").chosen({ allow_single_deselect: true });

    $.SumarMontoProducto = function () {

        var Suma = 0;

        $('#TableProducto tbody tr').each(function () {
            var Monto = $(this).find("td").eq(4).html();
            Suma += parseFloat(Monto.replace(/\./g, ''));
        });

        $("#TotalProducto").val($.number(Suma, 0, ",", "."));
        $("#tdTotalProducto").html($.number(Suma, 0, ",", "."));
        $("#Precio").val(Suma);
        $("#precio").val(Suma);
        $("#Saldo").val(Suma);
    }

    $(document).on("submit", "#formLente", function (e) {
        e.preventDefault();

        const Url = $(this).attr("action");
        const idOT = $("#idOT").val();
        const Valor = $("#Valor").val();
        const Cantidad = $("#Cantidad").val();
        const Comentario = $("#Comentario").val();
        const ProductoTexto = $("#idProducto option:selected").text();

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

                    var Total = (Valor * Cantidad);
                    var fila = "";

                    fila += '<tr>';
                    fila += '<td>' + ProductoTexto + '</td>';
                    fila += '<td class="text-right">' + Cantidad + '</td>';
                    fila += '<td class="text-right">' + $.number(Valor, 0, ",", ".") + '</td>';
                    fila += '<td>' + Comentario + '</td>';
                    fila += '<td class="text-right">' + $.number(Total, 0, ",", ".") + '</td>';
                    fila += '<td class="text-center"><a href="' + ObjLente.UrlElimina + "?id=" + data.respuesta + '" type="button" name="EliminaFila" class="btn btn-sm btn-outline-danger" title="Eliminar..."><i class="fa fa-trash"></i></a></td>';
                    fila += '</tr>';

                    $("#TableProducto tbody").append(fila);

                    $("#idProducto").val()
                    $("#Cantidad").val('')
                    $("#Valor").val('');
                    $("#Comentario").val('');

                    $.SumarMontoProducto();
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

    $(document).on('click', 'a[name=EliminaFila]', function (e) {
        e.preventDefault();

        const Fila = $(this).closest('tr');
        const Url = $(this).attr('href');

        swal({
            title: "Eliminar Lente",
            text: "¿Desea Eliminar Producto Seleccionado?",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        })
            .then((willDelete) => {
                if (willDelete) {

                    $.ajax({
                        url: Url,
                        cache: false,
                        type: "GET",
                        dataType: "json"
                    })
                        .done(function (data) {
                            if (data.ok) {

                                Fila.remove();
                                $.SumarMontoProducto();
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
                }
            });
    })

    $(document).on('click', "#btnSiguienteAbono", function (e) {

        const filas = $("#TableProducto tbody tr").length;
        if (filas > 0) {

            $("#Lentes-tab").removeClass("active");
            $("#Lentes-tab-content").removeClass("show active");

            $("#Pago-tab").addClass("active");
            $("#Pago-tab-content").addClass("show active");
        }
        else {
            swal("Alerta", "Debe Ingresar producto.", "warning");
        }

    })

    $(document).on("click", "#btnVolverReceta", function () {

        $("#Lentes-tab").removeClass("active");
        $("#Lentes-tab-content").removeClass("show active");

        $("#Receta-tab").addClass("active");
        $("#Receta-tab-content").addClass("show active");
    })

})