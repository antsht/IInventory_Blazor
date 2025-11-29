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


