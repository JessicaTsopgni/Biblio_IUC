$(function () {
    $('.jsdemo-notification-button button').on('click', function () {
        var placementFrom = $(this).data('placement-from');
        var placementAlign = $(this).data('placement-align');
        var animateEnter = $(this).data('animate-enter');
        var animateExit = $(this).data('animate-exit');
        var colorBg = $(this).data('color-name');

        showNotification(colorBg, null, placementFrom, placementAlign, animateEnter, animateExit);
    });
});


function showNotification(colorBg, colorText, appName, tile, text, icon, placementFrom, placementAlign, animateEnter, animateExit) {
    if (colorBg === null || colorBg === '') { colorBg = 'bg-black'; }
    if (text === null || text === '') { text = '...'; }
    if (animateEnter === null || animateEnter === '') { animateEnter = 'animated fadeInDown'; }
    if (animateExit === null || animateExit === '') { animateExit = 'animated fadeOutUp'; }
    var allowDismiss = true;

    $.notify({
        title: tile,
        message: text
    },
        {
            type: colorBg,
            allow_dismiss: allowDismiss,
            newest_on_top: true,
            timer: 1000,
            placement: {
                from: placementFrom,
                align: placementAlign
            },
            animate: {
                enter: animateEnter,
                exit: animateExit
            },
            template: '<div data-delay="5000" class="toast bg-{0} ' + colorText + '" role="alert" aria-live="assertive" aria-atomic="true">' +
                '<div class="toast-header bg-{0}">' +
                '<strong class="mr-auto">' +
                '<span><i class="' + icon + '"></i></span> ' +
                '<span>{1}</span></strong><span class="mr-1 ' + colorText + '"><small>' + appName + '</small></span>' +
                '<button type="button" onclick="$(\'#toast_close\').click()" class="close" data-dismiss="toast" aria-label="Close">' +
                '<span aria-hidden="true">&times;</span>' +
                '</button>' +
                '<button id="toast_close" class="d-none" data-notify="dismiss"></button>' +
                '</div>' +
                '<div class="toast-body">{2}</div>' +
                '</div>'
        });
    $('.toast').toast('show');
}