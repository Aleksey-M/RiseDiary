'use strict'

export async function loadLib() {
    await new Promise((resolve) => {
        try {
            if (typeof (Jcrop) === 'undefined') {
                const script = document.createElement('script');
                script.type = 'text/javascript';
                script.async = true;
                script.onload = function () {
                    resolve();
                };

                script.src = 'lib/jcrop/dist/jcrop.js';
                document.getElementsByTagName('head')[0].appendChild(script);
            }
            else {
                resolve();
            }
        }
        catch (err) {
            console.error(err);
        }
    });
}

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