(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.sc_kioskPersonSearch = (function () {
        var exports = {
            initialize: function (options) {
                if (!options.id) {
                    throw 'id is required';
                }

                var $control = $('#' + options.id);
                var lastPromise = null;
                var timer = null;

                $('.js-text-field', $control).on('input', function () {
                    var term = $(this).val();
                    var $container = $('.person-search-results', $control);

                    var performSearch = function () {
                        if (term.length >= 3) {
                            lastPromise = $.ajax({
                                url: '/api/People/Search?name=' + term + '&includeHtml=false&includeDetails=true&includeBusinesses=false&includeDeceased=false',
                                dataType: 'json'
                            });

                            lastPromise.done(function (data, status, jqXHR) {
                                if (jqXHR !== lastPromise) {
                                    return;
                                }

                                var newElements = [];
                                $.map(data, function (item) {
                                    var $result = $('<div class="person-result well"></div>');
                                    $result.append($('<div class="person-image">' + item.ImageHtmlTag.replace(/maxwidth=50/, 'maxwidth=150').replace(/maxheight=50/, 'maxheight=150').replace(/50px/g, '150px') + '</div>'));

                                    var $details = $('<div href="#" class="person-details"></div>').appendTo($result);

                                    var name = item.Name;
                                    if (item.Age >= 0) {
                                        name = name + ' (' + item.Age + ')';
                                    }

                                    $('<div></div>').append('<span>Name: </span>').append('<span>' + name + '</span>').appendTo($details);
                                    if (item.Email !== null && item.Email !== '') {
                                        $('<div></div>').append('<span>Email: </span>').append('<span>' + item.Email + '</span>').appendTo($details);
                                    }
                                    if (item.Address !== null && item.Address !== '') {
                                        $('<div></div>').append('<span>Address: </span>').append('<span>' + item.Address.replace('\r\n', '<br>') + '</span>').appendTo($details);
                                    }

                                    var $item = $('<div class="col-lg-4 col-sm-6"></div>');
                                    $item.append($result);

                                    $result.on('click', function () {
                                        $('input[type="hidden"]', $control).val(item.Id);
                                        $('.js-click', $control).get(0).click();
                                    });

                                    newElements.push($item);
                                });

                                $container.empty().append(newElements);
                            });
                        }
                        else {
                            $container.empty();
                        }
                    };

                    if (timer !== null) {
                        clearTimeout(timer);
                        timer = null;
                    }

                    timer = setTimeout(performSearch, 250);
                });
            }
        };

        return exports;
    }());
}(jQuery));
