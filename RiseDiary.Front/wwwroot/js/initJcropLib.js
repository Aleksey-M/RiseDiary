'use strict'

export async function initJcropLib() {
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