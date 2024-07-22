$(() => {
    var currentPath = window.location.pathname.toLowerCase();

    $('.navbar-nav .nav-link').each(function () {
        var linkPath = $(this).attr('href');

        if (linkPath) { // Check if linkPath is defined and not null
            linkPath = linkPath.toLowerCase(); // Convert to lower case
            if (currentPath === linkPath) {
                $(this).addClass('active');
            } else {
                $(this).removeClass('active');
            }
        }
    });
});
