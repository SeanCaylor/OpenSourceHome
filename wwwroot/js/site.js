// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
google.maps.event.addDomListener(window, "load", function () {
    var places = new google.maps.places.Autocomplete(
        document.getElementById("txtLocation")
    );
    google.maps.event.addListener(places, "place_changed", function () {
        var place = places.getPlace();
        document.getElementById("txtLatitude").value =
            place.geometry.location.lat();
        document.getElementById("txtLongitude").value =
            place.geometry.location.lng();
    });
});
