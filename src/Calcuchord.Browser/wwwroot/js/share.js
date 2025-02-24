async function shareHtml(htmlString, title) {
    let success = false;
    let domParser = new DOMParser();
    let doc = domParser.parseFromString(htmlString, 'text/html');
    try {
        // let pdf_doc = new window.jspdf.jsPDF();
        // var svgElements = doc.body.querySelectorAll('svg');
        // svgElements.forEach(function (item) {
        //     item.setAttribute("width", item.getBoundingClientRect().width);
        //     item.setAttribute("height", item.getBoundingClientRect().height);
        //     item.style.width = null;
        //     item.style.height = null;
        // });
        // await pdf_doc.html(doc.documentElement.outerHTML);
        // let base_64 = pdf_doc.output('datauristring').split('data:application/pdf;filename=generated.pdf;base64,')[1];
        //
        // const blob = new Blob([base_64], {type: 'application/pdf'});
        // const file = new File([blob], title + ".pdf", {type: 'application/pdf'});

        const blob = new Blob([htmlString], {type: 'text/html'});
        const file = new File([blob], 'shared_content.html', {type: 'text/html'});

        if (navigator.share && navigator.canShare({files: [file]})) {
            await navigator.share({
                title: title,
                files: [file],
            });
            console.log('Content shared successfully');
            success = true;
        } else {
            // pdf_doc.save(title + '.pdf');
            // success = true;
        }
    } catch (error) {
        console.error('Error sharing content:', error);
    }
    if (success) {
        return;
    }

    let close_btn = doc.createElement('button');
    close_btn.setAttribute('class', 'close-btn');
    close_btn.setAttribute('onclick', 'window.parent.closeShare();');
    close_btn.InnerHtml = 'Close';
    doc.body.appendChild(close_btn);

    const iframe = document.createElement('iframe');
    iframe.srcdoc = doc.documentElement.outerHTML;
    iframe.setAttribute('class', "share");
    document.body.appendChild(iframe);
}

function closeShare() {
    if (document.body.lastChild.tagName === 'IFRAME') {
        document.body.lastChild.remove();
    }
}