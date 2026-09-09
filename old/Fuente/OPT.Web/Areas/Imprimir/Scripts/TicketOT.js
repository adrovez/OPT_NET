$(document).ready(function () {

    $(document).on("click", "btnImprimirOT", function (e) {
        e.preventDefault();

        const Url = $(this).attr("href");
        $.ImprimirOT(Url);
      
    })

    $.ImprimirOT = function (Url) {

        $.ajax({
            url: Url,
            cache: false,
            type: "GET",
            dataType: "html"
        })
        .done(function (resp) {

            $("#divImprimir").empty().append(resp);
            const doc = document.querySelector("#main .card");
            const duplier = document.querySelector("#hh");
            const duplier2 = document.querySelector("#hh2");

            duplier.appendChild(doc.cloneNode(true));
            duplier2.appendChild(doc.cloneNode(true));

            window.print();
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
})

