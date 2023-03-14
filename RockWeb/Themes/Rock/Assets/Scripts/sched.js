// jquery ready
$(function () {
    columnResize();

    // if window resized
    $(window).resize(function () {
        columnResize();
    });

    const nextButton = $('.service-header .next');
    const prevButton = $('.service-header .prev');
    const snappyContainer = $('.snap-container');

    function handleNextPrevClick(e) {
        e.preventDefault();
        const currentServiceCol = $(this).closest('.service-column');
        let $nextPrev = null;

        if ($(this).hasClass('next')) {
            const nextServiceCol = currentServiceCol.next('.service-column');
            $nextPrev = nextServiceCol.length ? nextServiceCol : $(this).closest('.date').next('.date').find('.service-column').first();
            $nextPrev = $nextPrev.length ? $nextPrev : $('.date').first().find('.service-column').first();
        } else if ($(this).hasClass('prev')) {
            const prevServiceCol = currentServiceCol.prev('.service-column');
            $nextPrev = prevServiceCol.length ? prevServiceCol : $(this).closest('.date').prev('.date').find('.service-column').last();
            $nextPrev = $nextPrev.length ? $nextPrev : $('.date').last().find('.service-column').last();
        }

        const offset = $nextPrev.offset().left + snappyContainer.scrollLeft();
        snappyContainer.get(0).scrollTo({
            left: offset
        });
    }

    nextButton.click(handleNextPrevClick);
    prevButton.click(handleNextPrevClick);
});

// create ResizeObserver
var resizeObserver = new ResizeObserver(function (entries) {
    // for each .serrvice-column get the height of each .location
    $('.service-column').each(function () {
        var $this = $(this);
        $this.find('.location').each(function (i) {
            if (locationSize[i] === undefined) {
                locationSize[i] = $(this).height();
            } else if (locationSize[i] < $(this).height()) {
                locationSize[i] = $(this).height();
            }
        });
    });

    // for each .serrvice-column set the height of each .location
    $('.service-column').each(function () {
        var $this = $(this);
        $this.find('.location').each(function (i) {
            $(this).height(locationSize[i]);
        });
    });
});

function columnResize() {
    // for each .service-column set the height of each .location
    // if window size greater than 768px
    if ($(window).width() > 768) {
        const serviceColumns = document.querySelectorAll('.service-column');
        const locationSize = [];

        serviceColumns.forEach((column) => {
            const locations = column.querySelectorAll('.location');

            locations.forEach((location, i) => {
                if (locationSize[i] === undefined) {
                    locationSize[i] = location.offsetHeight;
                } else if (locationSize[i] < location.offsetHeight) {
                    locationSize[i] = location.offsetHeight;
                }
            });
        });

        $('.service-column').each(function () {
            var $this = $(this);
            $this.find('.location').each(function (i) {
                $(this).css('min-height', locationSize[i]);
            });
        });
    } else {
        $('.service-column').each(function () {
            var $this = $(this);
            $this.find('.location').each(function (i) {
                $(this).css('min-height', '');
            });
        });
    }
}

// observe the .service-column
$('.service-column').each(function () {
    resizeObserver.observe(this);
});
