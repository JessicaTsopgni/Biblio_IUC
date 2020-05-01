var t_end_page = window.setTimeout(function () {
    var end_page = $('#sidebarToggleTop');
    if (end_page != null)
        window.clearTimeout(t_end_page);
    "use strict"; // Start of use strict
    // Toggle the side navigation
    $("#sidebarToggle, #sidebarToggleTop").on('click', function (e) {
        $("body").toggleClass("sidebar-toggled");
        $(".sidebar").toggleClass("toggled");
        if ($(".sidebar").hasClass("toggled")) {
            $('.sidebar .collapse').collapse('hide');
        };
    });

    // Close any open menu accordions when window is resized below 768px
    $(window).resize(function () {
        if ($(window).width() < 768) {
            $('.sidebar .collapse').hide();
        };
    });

    // Prevent the content wrapper from scrolling when the fixed side navigation hovered over
    $('body.fixed-nav .sidebar').on('mousewheel DOMMouseScroll wheel', function (e) {
        if ($(window).width() > 768) {
            var e0 = e.originalEvent,
                delta = e0.wheelDelta || -e0.detail;
            this.scrollTop += (delta < 0 ? 1 : -1) * 30;
            e.preventDefault();
        }
    });

    // Scroll to top button appear
    $(document).on('scroll', function () {
        var scrollDistance = $(this).scrollTop();
        if (scrollDistance > 100) {
            $('.scroll-to-top').fadeIn();
        } else {
            $('.scroll-to-top').fadeOut();
        }
    });

    // Smooth scrolling using jQuery easing
    $(document).on('click', 'a.scroll-to-top', function (e) {
        var $anchor = $(this);
        $('html, body').stop().animate({
            scrollTop: ($($anchor.attr('href')).offset().top)
        }, 1000, 'easeInOutExpo');
        e.preventDefault();
    });

}, 2000);

default_content = '';
count_change = 0;

function readURL(input, target, size, width, height, callback = null) {
    if (count_change === 0)
        default_content = $(target).html();
    if (input.files && input.files[0]) {
        if (input.files[0].size <= (size * 1024)) {
            var reader = new FileReader();
            reader.onload = function (e) {
                if (input.files[0].type.match('image/*')) {
                    var img = new Image();
                    img.onload = function () {
                        var pw = 0;
                        var ph = 0;
                        if (width > 0) {
                            pw = img.width / (width - 5);
                        }
                        if (height > 0) {
                            ph = img.height / (height - 5);
                        }
                        var p = pw > ph ? pw : ph;
                        if (p > 1) {
                            width = Math.floor(img.width / p);
                            height = Math.floor(img.height / p);
                        }
                        else {
                            width = img.width;
                            height = img.height;
                        }
                        $(target).html('<img id="new_img" src = "' + e.target.result + '" class="img-fluid rounded"  />');
                        //$(target).html('<img src = "' + e.target.result + '" class="img-fluid" />');
                        count_change++;
                        if (callback)
                            callback(document.getElementById('new_img'), default_content, (width - 5), (height - 5));
                    };
                    var _URL = null;
                    if (window.URL)
                        _URL = window.URL;
                     if (window.webkitURL)
                         _URL = window.webkitURL;
                    img.src = _URL.createObjectURL(input.files[0]);
                }
                else {
                    $(target).html(current.default_content);
                }
            };
            reader.readAsDataURL(input.files[0]);
        }
        else {
            input.value = '';
            alert('Allowed file size exceeded. (Max. ' + size + ' ko)');
        }
    }
    else {
        $(target).html(this.default_content);
    }
}

$('input, textarea, select').not('input[type="hidden"], input[type="checkbox"]').each(function () {
    var req = $(this).attr('data-val-required');
    var disabled = $(this).attr('disabled');
    if (undefined !== req && disabled === undefined) {
        var label = $('label[for="' + $(this).attr('id') + '"]').not('label[data-asterix="false"]');
        var text = label.text();
        var html = label.html();
        var asterix = '<span style="color:red"> *</span>';
        if (text.length > 0 && html.indexOf(asterix) === -1) {
            label.append(asterix);
        }
    }
});

$(function () {
    $('form').not('form[action="multipart/form-data"]').submit(function (e) {
        if ($(this).valid()) {
            $(this).find(":submit").attr('disabled', 'disabled');
        }
    });
});

function init_model(title, message, bg_class, text_class,
    cancel_btn_text, cancel_btn_class, success_btn_text, success_btn_class, success_btn_link) {
    console.log(bg_class);
    $('#myModalContent').removeClass(["bg-primary", "bg-danger", "bg-success", "bg-info", "bg-warning"]);
    $('#myModalContent').addClass(bg_class);
    $('#myModalContent').removeClass(["text-dark", "text-white", "text-primary", "text-danger", "text-success", "text-info", "text-warning"]);
    $('#myModalContent').addClass(text_class);

    $('#myModalTitle').html(title);
    $('#myModalMessage').html(message);

    if (!cancel_btn_text)
        $('#myModalCancelBtn').hide();
    else
        $('#myModalCancelBtn').html(cancel_btn_text);
    $('#myModalCancelBtn').addClass(cancel_btn_class);

    if (!success_btn_text)
        $('#myModalOkBtn').hide();
    else
        $('#myModalOkBtn').html(success_btn_text);
    $('#myModalOkBtn').attr('href', success_btn_link);
    $('#myModalOkBtn').addClass(success_btn_class);
    $('#myModal').modal('show');
}

