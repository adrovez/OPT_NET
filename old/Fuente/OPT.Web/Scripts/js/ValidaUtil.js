$(document).ready(function () {

    /* ================ NUMEROS ENTEROS ================*/

    $("input[data-tipo=Numeric]").number(true, 0, ",", ".");

    $("input[data-tipo=Telefono]").number(false, 0);


    /* ================ SOLO LETRAS ================*/
    $("input[data-tipo=Alfa]").keypress(function (e) {
        tecla = (document.all) ? e.keyCode : e.which;
        if (tecla == 8) return true;
        patron = /[A-Z a-z á é í ó ú Á É Í Ó Ú ä ë ï ö ü Ä Ë Ï Ö Ü ñ Ñ]/;
        te = String.fromCharCode(tecla);
        return patron.test(te);
    });


    /* ================ ALFA NUMERICO ================*/
    $("input[data-tipo=AlfaNumeric]").keypress(function (e) {
        tecla = (document.all) ? e.keyCode : e.which;
        if (tecla == 8) return true;
        patron = /[A-Z a-z á é í ó ú Á É Í Ó Ú ä ë ï ö ü Ä Ë Ï Ö Ü ñ Ñ 0-9]/;
        te = String.fromCharCode(tecla);
        return patron.test(te);
    });

    $("input[data-tipo=Descripcion]").keypress(function (e) {
        tecla = (document.all) ? e.keyCode : e.which;
        if (tecla == 8) return true;
        patron = /[A-Z a-z á é í ó ú Á É Í Ó Ú ä ë ï ö ü Ä Ë Ï Ö Ü ñ Ñ 0-9 . : ; () , -]/;
        te = String.fromCharCode(tecla);
        return patron.test(te);
    });

    /* ================ EMAIL ================*/

    $('input[data-tipo=Email]').blur(function () {
        var regex = /[\w-\.]{2,}@([\w-]{2,}\.)*([\w-]{2,}\.)[\w-]{2,4}/;
        if ($(this).val() != '') {
            if (!regex.test($(this).val().trim())) {
                $(this).val('');
                $(this).focus();
            }
        }
    })

    /* ================ SOLO NUMERO ================*/
    $("input[data-tipo=SoloNumero]").keypress(function (e) {
        tecla = (document.all) ? e.keyCode : e.which;
        if (tecla == 8) return true;
        patron = /[^0-9]/g;
        te = String.fromCharCode(tecla);
        return !patron.test(te);
    });  

    /* ================ VALIDA RUT CHILENO ================*/
    $("input[data-tipo=RUT]").keypress(function (e) {
        tecla = (document.all) ? e.keyCode : e.which;
        if (tecla == 8) return true;
        patron = /[0-9 K k -]/;
        te = String.fromCharCode(tecla);
        return patron.test(te);
    });

    $("input[data-tipo=DecimalNegativo]").keypress(function (evt) {
        var charCode = (evt.which) ? evt.which : event.keyCode;
        var number = $(this).val().split(',');
        // permitir el signo de - (45)
        if (charCode != 45 && charCode != 44 && charCode > 31 && (charCode < 48 || charCode > 57)) {
            return false;
        }
        //just one dot
        if (number.length > 1 && charCode == 44) {
            return false;
        }
        //get the carat position
        var caratPos = $.getSelectionStart($(this));
        // no permitir que se ponega el - en una posicion diferente de la inicial
        if (caratPos > 0 && charCode == 45) {
            return false;
        }
        // no permtir mas de un - en el numero
        if (charCode == 45 && $(this).val().charAt(0) == "-") {
            return false;
        }
        var dotPos = $(this).val().indexOf(",");
        if (caratPos > dotPos && dotPos > -1 && (number[1].length > 1)) {
            return false;
        }
        return true;
    });

    $.getSelectionStart = function (o) {
        if (o.createTextRange) {
            var r = document.selection.createRange().duplicate()
            r.moveEnd('character', o.value.length)
            if (r.text == '') return o.value.length
            return o.value.lastIndexOf(r.text)
        } else { return o.selectionStart }
    }

    //$(document).on("change", "input[data-tipo=RUT]", function () {

    //    var rutformato = $.Rut.formatear($(this).val(), true);

    //    if ($.Rut.validar(rutformato)) {
    //        $(this).val(rutformato);
    //    }
    //    else {
    //        $(this).val('');
    //        $(this).focus();
    //        UI.alert("ERROR:", "Rut ingresado no es valido.");
    //    }
    //})

    $(document).on('click', 'a[name=EliminarFila]', function (event) {
        event.preventDefault();
        var Fila = $(this).closest('tr');

        swal({
            title: "Eliminar",
            text: "¿Esta seguro de eliminar el Dato seleccionado.?",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        })
        .then((willDelete) => {
            if (willDelete) {
                Fila.remove();
            }
        });
    });

    $.FormatoFechaJson = function (pFecha) {

        //var DateJson = "/Date(" + pFecha + ")/";
        var DateJson = pFecha;
        var nowDate = new Date(parseInt(DateJson.replace(/(^.*\()|([+-].*$)/g, '')));
        var dia = nowDate.getDate();
        var mes = (nowDate.getMonth() + 1);
        var anio = nowDate.getFullYear();

        if (dia < 10) dia = "0" + dia;
        if (mes < 10) mes = "0" + mes;

        var result = dia + "-" + mes + "-" + anio;

        return result;
    }

    $.InicializaDataTable = function (Obj) {
        $('#' + Obj).DataTable({

            paging: true,
            lengthChange: false,
            ordering: true,
            info: false,
            searching: false,
            pagingType: "full_numbers",
            bSort: false,
            destroy: true,
            pageLength: 10,
            order: [[0, "desc"]],

            language: {
                decimal: "",
                emptyTable: "No hay datos disponibles en nuestra base de datos",
                info: "Mostrando _START_ de _END_ hasta _TOTAL_ entradas",
                infoEmpty: "Mostrando 0 de 0 hasta 0 entradas",
                infoFiltered: "(filtrando desde _MAX_ total entradas)",
                infoPostFix: "",
                thousands: ",",
                lengthMenu: "Mostrar _MENU_ entradas",
                loadingRecords: "Loading...",
                processing: "Processing...",
                search: "Buscar:",
                zeroRecords: "No existen registros",
                paginate: {
                    first: "Primero",
                    last: "Último",
                    next: "",
                    previous: ""
                },
                aria: {
                    sortAscending: ": activa orden ascendente",
                    sortDescending: ": activar orden descending"
                }
            }

        });
    }

    $.InicializaDataTableModal = function (Obj) {
        $('#' + Obj).DataTable({

            "paging": true,
            "lengthChange": false,
            "ordering": false,
            "info": false,
            "searching": false,
            "pagingType": "full_numbers",
            "bSort": false,
            "destroy": true,
            "pageLength": 10,
            "order": [[0, "desc"]],

            "language": {
                "decimal": "",
                "emptyTable": "No hay datos disponibles en nuestra base de datos",
                "info": "Mostrando _START_ de _END_ hasta _TOTAL_ entradas",
                "infoEmpty": "Mostrando 0 de 0 hasta 0 entradas",
                "infoFiltered": "(filtrando desde _MAX_ total entradas)",
                "infoPostFix": "",
                "thousands": ",",
                "lengthMenu": "Mostrar _MENU_ entradas",
                "loadingRecords": "Loading...",
                "processing": "Processing...",
                "search": "Buscar:",
                "zeroRecords": "No existen registros",
                "paginate": {
                    "first": "Primero",
                    "last": "Último",
                    "next": "",
                    "previous": ""
                },
                "aria": {
                    "sortAscending": ": activa orden ascendente",
                    "sortDescending": ": activar orden descending"
                }
            }

        });
    }

    $.InicializaDataTableOrderFecha = function (Obj) {
        $('#' + Obj).DataTable({

            paging: true,
            lengthChange: false,
            ordering: true,
            info: false,
            searching: false,
            pagingType: "full_numbers",
            bSort: false,
            destroy: true,
            pageLength: 10,
            columnDefs: [{ type: 'date', 'targets': [0] }],
            order: [[0, "desc"]],

            language: {
                decimal: "",
                emptyTable: "No hay datos disponibles en nuestra base de datos",
                info: "Mostrando _START_ de _END_ hasta _TOTAL_ entradas",
                infoEmpty: "Mostrando 0 de 0 hasta 0 entradas",
                infoFiltered: "(filtrando desde _MAX_ total entradas)",
                infoPostFix: "",
                thousands: ",",
                lengthMenu: "Mostrar _MENU_ entradas",
                loadingRecords: "Loading...",
                processing: "Processing...",
                search: "Buscar:",
                zeroRecords: "No existen registros",
                paginate: {
                    first: "Primero",
                    last: "Último",
                    next: "",
                    previous: ""
                },
                aria: {
                    sortAscending: ": activa orden ascendente",
                    sortDescending: ": activar orden descending"
                }
            }

        });
    }

    $(document).on('change', 'input[type="file"]', function () {

        const fileName = this.files[0].name;
        const fileSize = this.files[0].size;
        const allowedExtensions = /(.xls|.xlsx|.csv)$/i;

        if (!allowedExtensions.exec(fileName)) {

            swal("¡Tipo Archivo!", "Debe seleccionar archivo EXCEL ", "warning");
            this.value = '';
        }

        if (fileSize > 4000000) {

            swal("¡Tamaño Archivo!", "El archivo no debe superar los 4MB", "warning");
            this.value = '';
        }

    });


})