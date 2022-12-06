"use strict"

export function loadLib(dotnetCallbacks) {
    try {
        if (typeof (Jcrop) === 'undefined') {
            const script = document.createElement('script');
            script.type = 'text/javascript';
            script.async = true;
            script.onload = function () {
                dotnetCallbacks.invokeMethodAsync('Completed');
            };

            script.src = 'lib/jcrop/dist/jcrop.js';
            document.getElementsByTagName('head')[0].appendChild(script);
        }
        else {
            dotnetCallbacks.invokeMethodAsync('Completed');
        }
    }
    catch (e) {
        console.log(e);
        dotnetCallbacks.invokeMethodAsync('Error', e.message);
    }    
}


export function initJcrop(imageId, dotnetCallbacks) {
    try {        
        const stage = Jcrop.attach(imageId);
        const dotnetRef = DotNet.createJSObjectReference(stage);

        dotnetCallbacks.invokeMethodAsync('Completed', dotnetRef);
    }
    catch (e) {
        console.log(e);
        dotnetCallbacks.invokeMethodAsync('Error', e.message);
    }
}


export function getSelection(jcrop, imageId, dotnetCallbacks) {    
    try {
        const image = document.getElementById(imageId);

        let selectedArea = { x: 0, y: 0, w: 0, h: 0 };

        if (jcrop.active) {
            selectedArea = jcrop.active.pos;
        }

        const data = {
            selLeft: selectedArea.x,
            selTop: selectedArea.y,
            selWidth: selectedArea.w,
            selHeight: selectedArea.h,
            imageNaturalHeight: image.naturalHeight,
            imageNaturalWidth: image.naturalWidth,
            ImageHeight: image.height,
            imageWidth: image.width
        };

        dotnetCallbacks.invokeMethodAsync('Completed', data);
    }
    catch (e) {
        console.log(e);
        dotnetCallbacks.invokeMethodAsync('Error', e.message);
    }
}


export function disposeJcrop(jcrop, dotnetCallbacks) {
    try {
        jcrop.destroy();
        DotNet.disposeJSObjectReference(jcrop);
        dotnetCallbacks.invokeMethodAsync('Completed');
    }
    catch (e) {
        console.log(e);
        dotnetCallbacks.invokeMethodAsync('Error', e.message);
    }
}