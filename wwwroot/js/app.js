// Print barcode function
window.printBarcode = function(html) {
    const printWindow = window.open('', '_blank');
    if (!printWindow) {
        alert('Пожалуйста, разрешите всплывающие окна для печати штрихкодов');
        return;
    }
    printWindow.document.write(html);
    printWindow.document.close();
    setTimeout(() => {
        printWindow.print();
    }, 250);
};

// Download file function
window.downloadFile = function(filename, content) {
    const blob = new Blob([content], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', filename);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

// ESC key listener for modals
window.addEscListener = function(dotNetRef) {
    const handler = function(e) {
        if (e.key === 'Escape') {
            dotNetRef.invokeMethodAsync('HandleEscKey');
            document.removeEventListener('keydown', handler);
        }
    };
    document.addEventListener('keydown', handler);
};

// Download PDF function
window.downloadPdf = function(filename, byteArray) {
    const blob = new Blob([new Uint8Array(byteArray)], { type: 'application/pdf' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

// Register Service Worker for PWA
if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => {
        navigator.serviceWorker.register('/service-worker.js')
            .then(reg => console.log('SW registered'))
            .catch(err => console.log('SW failed', err));
    });
}

// Camera Scanner logic
let html5QrCode = null;
let scannerRunning = false;

window.startScanner = async function(dotNetRef, elementId) {
    console.log('startScanner called, scannerRunning:', scannerRunning);
    
    // Clean up previous scanner if exists
    if (html5QrCode) {
        if (scannerRunning) {
            try {
                await html5QrCode.stop();
                console.log('Previous scanner stopped');
            } catch(e) {
                console.log('Stop error (ignored):', e.message);
            }
        }
        html5QrCode = null;
    }
    
    scannerRunning = false;
    
    // Clear the element
    const element = document.getElementById(elementId);
    if (element) {
        element.innerHTML = '';
    }
    
    initializeScanner(dotNetRef, elementId);
};

function initializeScanner(dotNetRef, elementId) {
    // Supported barcode formats
    const formatsToSupport = [
        Html5QrcodeSupportedFormats.QR_CODE,
        Html5QrcodeSupportedFormats.CODE_128,
        Html5QrcodeSupportedFormats.CODE_39,
        Html5QrcodeSupportedFormats.CODE_93,
        Html5QrcodeSupportedFormats.EAN_13,
        Html5QrcodeSupportedFormats.EAN_8,
        Html5QrcodeSupportedFormats.UPC_A,
        Html5QrcodeSupportedFormats.UPC_E,
        Html5QrcodeSupportedFormats.ITF,
        Html5QrcodeSupportedFormats.CODABAR
    ];

    // Create scanner with format support
    html5QrCode = new Html5Qrcode(elementId, { formatsToSupport: formatsToSupport, verbose: false });
    
    // Scanner config - rectangular area better for barcodes
    const config = { 
        fps: 15, 
        qrbox: { width: 280, height: 120 },
        aspectRatio: 1.777778 // 16:9
    };

    html5QrCode.start(
        { facingMode: "environment" },
        config,
        (decodedText, decodedResult) => {
            // Success callback
            console.log('Scanned:', decodedText, decodedResult);
            
            // Vibrate on success (mobile)
            if (navigator.vibrate) {
                navigator.vibrate(200);
            }
            
            dotNetRef.invokeMethodAsync('HandleScannerResult', decodedText);
        },
        (errorMessage) => {
            // Ignore scan errors (happens every frame when no barcode found)
        }
    ).then(() => {
        scannerRunning = true;
        console.log('Scanner started successfully');
    }).catch(err => {
        console.error('Error starting scanner:', err);
        scannerRunning = false;
        alert('Ошибка запуска камеры: ' + err);
    });
}

window.stopScanner = async function() {
    console.log('stopScanner called, scannerRunning:', scannerRunning);
    
    if (html5QrCode && scannerRunning) {
        try {
            await html5QrCode.stop();
            console.log('Scanner stopped');
        } catch(e) {
            console.log('Stop error (ignored):', e.message);
        }
    }
    
    scannerRunning = false;
    html5QrCode = null;
    
    return Promise.resolve();
};


