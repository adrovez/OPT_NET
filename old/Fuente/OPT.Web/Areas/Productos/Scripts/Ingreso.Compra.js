var ObjProducto = {};
$(document).ready(function () {

    $("#Producto").chosen({ allow_single_deselect: true });

    $('#FechaDocumento').datepicker({
        format: 'dd/mm/yyyy',
        endDate: '0d',
        language: 'es'
    });

    $("#formDocumento").submit(function (e) {
        e.preventDefault();
        const Url = $(this).attr("action");
        const Filas = $("#tableProductos tbody tr").length;

        if (Filas == 0) {
            swal("Alerta", "Debe Ingresar productos", "warning");
            return false;
        }
        const formData = new FormData(this);
        let items = [];
        $('#tableProductos tbody tr').each(function () {
            const item = {};
            item.idProducto = $(this).find("input[name=idProducto]").val();
            item.Cantidad = $(this).find("input[name=Cantidad]").val();
            item.ValorUnitario = $(this).find("input[name=ValorUnitario]").val();
            items.push(item);

        });

        formData.append("ListProductos", JSON.stringify(items));

        $.ajax({
            url: Url,
            cache: false,
            processData: false,
            contentType: false,
            type: "POST",
            dataType: "json",
            data: formData
        })
            .done(function (data) {
                if (data.ok) {

                    swal("OK", "Datos Actualizados", "success")

                    $("#formDocumento")[0].reset();
                    $("#Producto-tab").removeClass("active");
                    $("#Producto-tab-content").removeClass("show active");

                    $("#Documento-tab").addClass("active");
                    $("#Documento-tab-content").addClass("show active");
                    $("#tableProductos").empty();
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
            })
    })

    $("#AgregaProducto").click(function () {

        const Producto = $("#Producto option:selected").val();
        const NomProducto = $("#Producto option:selected").text();
        const Cantidad = $("#Cantidad").val();
        const Valor = $("#Valor").val();

        if (Producto == "" || Producto == 0) {
            swal("Alerta", "Debe seleccionar producto.", "warning");
            return false;
        }

        if (Cantidad == "" || Cantidad == 0) {
            swal("Alerta", "Debe ingresar cantidad de producto.", "warning");
            return false;
        }

        if (Valor == "" || Valor == 0) {
            swal("Alerta", "Debe ingresar valor del producto.", "warning");
            return false;
        }

        let fila = "";
        fila += '<tr> ';
        fila += '<td><input type="hidden" name="idProducto" value="' + Producto + '"/>' + NomProducto + '</td> ';
        fila += '<td class="text-right"><input type="hidden" name="Cantidad" value="' + Cantidad + '"/>' + $.number(Cantidad, 0, ",", ".") + '</td> ';
        fila += '<td class="text-right"><input type="hidden" name="ValorUnitario" value="' + Valor + '"/>' + $.number(Valor, 0, ",", "."); + '</td> ';
        fila += '<td class="text-center"><a href="#" class="btn btn-sm btn-outline-danger" name="EliminarFila" title="Eliminar..."><i class="fa fa-trash"></i></a></td>'
        fila += '</tr> ';

        $("#tableProductos").append(fila);

        $("#Producto").val();
        $("#Cantidad").val("");
        $("#Valor").val("");
    })

    $("#btnSiguiente").click(function (e) {

        const NumDocumento = $("#NumDocumento").val();
        const idTipoDocumento = $("#idTipoDocumento option:selected").val();
        const FechaDocumento = $("#FechaDocumento").val();
        const Proveedor = $("#Proveedor").val();
        let Mensaje = "";

        if (NumDocumento == "" || NumDocumento == 0) {
            swal("Alerta", "Debe ingresar numero de documento.", "warning");
            return false;
        }
        if (idTipoDocumento == "") {
            swal("Alerta", "Debe seleccionar tipo de documento.", "warning");
            return false;
        }
        if (FechaDocumento == "") {
            swal("Alerta", "Debe ingresar fecha de documento.", "warning");
            return false;
        }
        if (Proveedor == "") {
            swal("Alerta", "Debe ingresar proveedor.", "warning");
            return false;
        }

        $("#Documento-tab").removeClass("active");
        $("#Documento-tab-content").removeClass("show active");
        $("#Producto-tab").addClass("active");
        $("#Producto-tab-content").addClass("show active");
    })

    $(document).on("click", "#btnVolverDocumento", function () {

        $("#Producto-tab").removeClass("active");
        $("#Producto-tab-content").removeClass("show active");

        $("#Documento-tab").addClass("active");
        $("#Documento-tab-content").addClass("show active");
    })
})