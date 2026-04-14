// File download helper function for Blazor
function downloadFile(fileName, base64Content) {
    const link = document.createElement('a');
    link.href = 'data:application/octet-stream;base64,' + base64Content;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

// Abre una URL en una nueva pestaña del navegador
function openInNewTab(url) {
    window.open(url, '_blank');
}

// Controla el aviso de beforeunload para cambios sin guardar
let _beforeUnloadEnabled = false;
function setBeforeUnloadWarning(enabled) {
    _beforeUnloadEnabled = enabled;
    if (enabled) {
        window.addEventListener('beforeunload', _beforeUnloadHandler);
    } else {
        window.removeEventListener('beforeunload', _beforeUnloadHandler);
    }
}

function _beforeUnloadHandler(e) {
    if (_beforeUnloadEnabled) {
        e.preventDefault();
        e.returnValue = '';
    }
}

