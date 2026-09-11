'use strict'

export let JcropInstance = function () {
    return {
        attach: function (imageId) {

            if (this.jcrop) {
                this.jcrop.destroy();
                this.jcrop = null;
            }

            this.image = document.getElementById(imageId);
            this.jcrop = Jcrop.attach(this.image);
        },

        detach: function () {

            if (this.jcrop) {
                this.jcrop.destroy();
            }

            this.jcrop = null;
            this.image = null;
        },

        getSelection: function () {
            let selectedArea = { x: 0, y: 0, w: 0, h: 0 };

            if (this.jcrop.active) {
                selectedArea = this.jcrop.active.pos;
            }

            const data = {
                selLeft: selectedArea.x,
                selTop: selectedArea.y,
                selWidth: selectedArea.w,
                selHeight: selectedArea.h,
                imageNaturalHeight: this.image.naturalHeight,
                imageNaturalWidth: this.image.naturalWidth,
                ImageHeight: this.image.height,
                imageWidth: this.image.width
            };

            return data;
        },

        image: null,
        jcrop: null
    };    
};