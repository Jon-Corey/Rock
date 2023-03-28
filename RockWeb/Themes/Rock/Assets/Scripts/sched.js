// jquery ready
$(function () {
    columnResize();

    // if window resized
    $(window).resize(function () {
        columnResize();
    });

    const nextButton = $('.schedule-header .next');
    const prevButton = $('.schedule-header .prev');
    const snappyContainer = $('.snap-container');

    function handleNextPrevClick(e) {
        e.preventDefault();
        const currentScheduleCol = $(this).closest('.schedule-column');
        let $nextPrev = null;

        if ($(this).hasClass('next')) {
            const nextScheduleCol = currentScheduleCol.next('.schedule-column');
            $nextPrev = nextScheduleCol.length ? nextScheduleCol : $(this).closest('.date').next('.date').find('.schedule-column').first();
            $nextPrev = $nextPrev.length ? $nextPrev : $('.date').first().find('.schedule-column').first();
        } else if ($(this).hasClass('prev')) {
            const prevScheduleCol = currentScheduleCol.prev('.schedule-column');
            $nextPrev = prevScheduleCol.length ? prevScheduleCol : $(this).closest('.date').prev('.date').find('.schedule-column').last();
            $nextPrev = $nextPrev.length ? $nextPrev : $('.date').last().find('.schedule-column').last();
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
    // for each .schedule-column get the height of each .location
    $('.schedule-column').each(function () {
        var $this = $(this);
        $this.find('.location').each(function (i) {
            if (locationSize[i] === undefined) {
                locationSize[i] = $(this).height();
            } else if (locationSize[i] < $(this).height()) {
                locationSize[i] = $(this).height();
            }
        });
    });

    // for each .schedule-column set the height of each .location
    $('.schedule-column').each(function () {
        var $this = $(this);
        $this.find('.location').each(function (i) {
            $(this).height(locationSize[i]);
        });
    });
});

function columnResize() {
    // for each .schedule-column set the height of each .location
    // if window size greater than 768px
    if ($(window).width() > 768) {
        const scheduleColumns = document.querySelectorAll('.schedule-column');
        const locationSize = [];

        scheduleColumns.forEach((column) => {
            const locations = column.querySelectorAll('.location');

            locations.forEach((location, i) => {
                if (locationSize[i] === undefined) {
                    locationSize[i] = location.offsetHeight;
                } else if (locationSize[i] < location.offsetHeight) {
                    locationSize[i] = location.offsetHeight;
                }
            });
        });

        $('.schedule-column').each(function () {
            var $this = $(this);
            $this.find('.location').each(function (i) {
                $(this).css('min-height', locationSize[i]);
            });
        });
    } else {
        $('.schedule-column').each(function () {
            var $this = $(this);
            $this.find('.location').each(function (i) {
                $(this).css('min-height', '');
            });
        });
    }
}

// observe the .schedule-column
$('.schedule-column').each(function () {
    resizeObserver.observe(this);
});
