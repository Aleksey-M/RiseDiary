'use strict'

let tooltipList;

function initializeTooltips() {
    tooltipList = [].slice
        .call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
        .map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl)
        });
}

function loadImageOnThumbnailClick(imageId) {
    const popupBodySelector = 'div#fullImage-' + imageId;

    if (document.querySelector(popupBodySelector + ' img')) return;

    const image = new Image();
    image.className = 'img-fluid';
    image.src = '/api/v1.0/image-file/' + imageId;

    document.querySelector(popupBodySelector + ' div.modal-body').appendChild(image);
}