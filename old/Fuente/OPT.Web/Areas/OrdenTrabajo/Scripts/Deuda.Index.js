var ObjOT = {};

$(document).ready(function () {

    $(".number").number(true, 0, ",", ".");

    $(document).on('click', 'a[name=Ver]', function (e) {
        e.preventDefault();
        var url = $(this).attr('href');
        $('#my-modal-cont').load(url, function (result) {
            $('#myModal').modal({ backdrop: 'static', keyboard: false, show: true });
            $(".number").number(true, 0, ",", ".");
        });
    });
    
})