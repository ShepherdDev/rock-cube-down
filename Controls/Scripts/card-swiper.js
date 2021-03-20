(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.sc_cardSwiper = (function () {
        var exports = {
            initialize: function (options) {
                if (!options.id) {
                    throw 'id is required';
                }

                var $swiper = $('#' + options.id);
                var lastKeyPress = 0;
                var keyboardBuffer = '';
                var swipeProcessing = false;

                //
                // Setup swipe detection
                //
                $(document).keypress(function (e) {
                    var date = new Date();

                    if ($swiper.length > 0) {
                        var sinceLastKeyPress = date.getTime() - lastKeyPress;

                        if (e.which === 37 && sinceLastKeyPress > 500) {
                            // Start buffering if first character of the swipe (always '%')
                            keyboardBuffer = String.fromCharCode(e.which);
                        }
                        else if (sinceLastKeyPress < 100) {
                            // Continuing the reading into the buffer if the stream of characters is still coming
                            keyboardBuffer += String.fromCharCode(e.which);
                        }

                        // If the character is a line break stop buffering and call postback
                        if (e.which === 13 && keyboardBuffer.length !== 0 && !swipeProcessing) {
                            swipeProcessing = true;

                            window.location = "javascript:__doPostBack('" + options.postback + "', '" + encodeURIComponent(keyboardBuffer.trim()) + "')";
                            keyboardBuffer = '';
                        }

                        // Stop the keypress
                        e.preventDefault();
                    }

                    lastKeyPress = date.getTime();
                });

                //
                // Setup test card handler.
                //
                {
                    var lastClick = 0;
                    var clickCount = 0;

                    $swiper.on('click', function (e) {
                        var date = new Date();
                        var timeSince = date.getTime() - lastClick;

                        lastClick = date.getTime();

                        if (timeSince > 500) {
                            clickCount = 0;
                        }

                        clickCount += 1;

                        if (clickCount === 5) {
                            var sampleCard = '%B4111111111111111^DECKER TED                ^30121200000000000000**411******?*';
                            window.location = "javascript:__doPostBack('" + options.postback + "', '" + encodeURIComponent(sampleCard) + "')";
                        }
                    });
                }
            }
        };

        return exports;
    }());
}(jQuery));
