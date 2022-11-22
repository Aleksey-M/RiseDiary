'use strict'

let tooltipList;

function initializeTooltips() {
    tooltipList = [].slice
        .call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
        .map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl)
        });
}