var UI = UI || {};

UI.alert = function (titulo, mensaje, callback) {

    //esta rutina depende del html (modalAlert) incrustado en la vista "_Layout"

    if (callback) {
        $('#modalAlert').on('hidden.bs.modal', function () {
            callback();
            $('#modalAlert').off('hidden.bs.modal');
        });
    }

    $('#titulo', $('#modalAlert')).html(titulo);
    $('#mensaje', $('#modalAlert')).html(mensaje);

    $('#modalAlert').modal({ backdrop: 'static', keyboard: false, show: true });
};

