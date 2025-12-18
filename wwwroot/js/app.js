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
    // Create scanner without format restriction - scan all formats
    html5QrCode = new Html5Qrcode(elementId, { verbose: true });
    
    // Scanner config - scan full frame for better detection
    const config = { 
        fps: 10,
        qrbox: function(viewfinderWidth, viewfinderHeight) {
            // Use 80% of viewfinder for scanning area
            let minEdge = Math.min(viewfinderWidth, viewfinderHeight);
            let qrboxSize = Math.floor(minEdge * 0.8);
            return { width: qrboxSize, height: Math.floor(qrboxSize * 0.5) };
        },
        experimentalFeatures: {
            useBarCodeDetectorIfSupported: true
        }
    };

    console.log('Starting scanner with config:', config);

    html5QrCode.start(
        { facingMode: "environment" },
        config,
        (decodedText, decodedResult) => {
            // Success callback
            console.log('✅ SCANNED:', decodedText);
            console.log('Format:', decodedResult.result.format.formatName);
            
            // Vibrate on success (mobile)
            if (navigator.vibrate) {
                navigator.vibrate(200);
            }
            
            dotNetRef.invokeMethodAsync('HandleScannerResult', decodedText);
        },
        (errorMessage) => {
            // Log every 100th error to see if scanner is working
            if (!window._scanErrorCount) window._scanErrorCount = 0;
            window._scanErrorCount++;
            if (window._scanErrorCount % 100 === 0) {
                console.log('Scanning... (no barcode found yet, attempt:', window._scanErrorCount, ')');
            }
        }
    ).then(() => {
        scannerRunning = true;
        window._scanErrorCount = 0;
        console.log('✅ Scanner started successfully - point camera at barcode');
    }).catch(err => {
        console.error('❌ Error starting scanner:', err);
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


