async function shareFileAsync(b64, mimeType, fileName, title) {
    if (isMobile()) {
        const blob = new Blob([base64ToArrayBuffer(b64)], {type: mimeType});
        const file = new File([blob], fileName, {type: mimeType});

        if (navigator.share && navigator.canShare({files: [file]})) {
            await navigator.share({
                title: title,
                files: [file],
            });
            console.log(`${fileName} shared successfully`);
            return true;
        }
    }

    const uri = `data:${mimeType};base64,${b64}`;
    const link = document.createElement('a');
    link.href = uri;
    link.download = fileName;
    link.click();

    console.log(`${fileName} share unsuccessfull`);
    return false;
}

function base64ToArrayBuffer(base64) {
    var binaryString = atob(base64);
    var bytes = new Uint8Array(binaryString.length);
    for (var i = 0; i < binaryString.length; i++) {
        bytes[i] = binaryString.charCodeAt(i);
    }
    return bytes.buffer;
}

function closeShare() {
    if (document.body.lastChild.tagName === 'IFRAME') {
        document.body.lastChild.remove();
    }
}