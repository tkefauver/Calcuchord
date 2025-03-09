async function shareFileAsync(b64, mimeType, fileName) {
    if (isMobile()) {
        const blob = new Blob([base64ToArrayBuffer(b64)], {type: mimeType});
        const file = new File([blob], fileName, {type: mimeType});

        if (navigator.share && navigator.canShare({files: [file]})) {
            await navigator.share({
                title: fileName,
                files: [file],
            });
            return;
        }
    }

    const uri = `data:${mimeType};base64,${b64}`;
    const link = document.createElement('a');
    link.href = uri;
    link.download = fileName;
    link.click();
}

function base64ToArrayBuffer(base64) {
    var binaryString = atob(base64);
    var bytes = new Uint8Array(binaryString.length);
    for (var i = 0; i < binaryString.length; i++) {
        bytes[i] = binaryString.charCodeAt(i);
    }
    return bytes.buffer;
}