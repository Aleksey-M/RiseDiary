'use strict'

export async function getImageDimensions(content) {

    const url = URL.createObjectURL(new Blob([await content.arrayBuffer()]));

    const dimensions = await new Promise(resolve => {

        const img = new Image();

        img.onload = function () {
            const data = { width: img.naturalWidth, height: img.naturalHeight };
            URL.revokeObjectURL(url);

            resolve(data);
        };

        img.src = url;
    });

    return dimensions;
}