$(document).ready(function () {

    $.InicializaDataTable("TableAnamnesis");
    $.InicializaDataTable("TableReceta");
    $.InicializaDataTable("TableOT");

    $(".number").number(true, 0, ",", ".");   

    $(document).on('click', 'a[name=VerDetalleOT]', function (e) {
        e.preventDefault();
        const url = $(this).attr('href');
        $('#my-modal-cont').load(url, function (result) {
            $('#myModal').modal({ backdrop: 'static', keyboard: false, show: true });
            $(".number").number(true, 0, ",", ".");
        });

    });

    $(document).on('click', 'a[name=VerDetalleAnamnesis]', function (e) {
        e.preventDefault();
        const url = $(this).attr('href');
        $('#my-modal-cont').load(url, function (result) {
            $('#myModal').modal({ backdrop: 'static', keyboard: false, show: true });
            $(".number").number(true, 0, ",", ".");
        });

    });

    $(document).on('click', '#NuevaAnamnesis', function (e) {
        e.preventDefault();
        const url = $(this).attr('href');
        $('#my-modal-cont').load(url, function (result) {
            $('#myModal').modal({ backdrop: 'static', keyboard: false, show: true });
        });

    });

    $(document).on('click', '#NuevaReceta', function (e) {
        e.preventDefault();
        const url = $(this).attr('href');
        $('#my-modal-cont').load(url, function (result) {
            $('#myModal').modal({ backdrop: 'static', keyboard: false, show: true });
        });

    });

    $(document).on('click', 'a[name=VerDetalleReceta]', function (e) {
        e.preventDefault();
        const url = $(this).attr('href');
        $('#my-modal-cont').load(url, function (result) {
            $('#myModal').modal({ backdrop: 'static', keyboard: false, show: true });
            $(".number").number(true, 0, ",", ".");
        });
    });

    $(document).on('click', 'a[name=VerDetalleCristales]', function (e) {
        e.preventDefault();
        const url = $(this).attr('href');
        $('#my-modal-cont').load(url, function (result) {
            $('#myModal').modal({ backdrop: 'static', keyboard: false, show: true });
        });

    });

})